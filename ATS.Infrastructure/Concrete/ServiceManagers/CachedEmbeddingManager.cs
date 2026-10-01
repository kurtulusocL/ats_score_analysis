using ATS.Application.Abstract.AI;
using ATS.Core.Helpers;
using ATS.Domain.Entities;
using ATS.Infrastructure.Concrete.AI;
using ATS.Infrastructure.Persistence.Context.Mssql;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Cryptography;
using System.Text;

namespace ATS.Infrastructure.Concrete.ServiceManagers
{
    public class CachedEmbeddingManager : IEmbeddingService
    {
        private const int GenerationBatchSize = 32;
        private readonly AiProviderClientResolver _aiProviderClientResolver;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly AiUsageTracker _aiUsageTracker;
        private readonly ILogger _logger = Log.ForContext<CachedEmbeddingManager>();

        public CachedEmbeddingManager(AiProviderClientResolver aiProviderClientResolver, ApplicationDbContext applicationDbContext, AiUsageTracker aiUsageTracker)
        {
            _aiProviderClientResolver = aiProviderClientResolver;
            _applicationDbContext = applicationDbContext;
            _aiUsageTracker = aiUsageTracker;
        }

        public async Task<IReadOnlyList<float[]>> GetEmbeddingsAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(texts);

            var aiProviderClientFactory = _aiProviderClientResolver.Resolve();
            var model = aiProviderClientFactory.EmbeddingModelIdentity;
            if (string.IsNullOrWhiteSpace(model))
                throw new InvalidOperationException("Embedding model is not configured.");

            if (texts.Count == 0)
                return Array.Empty<float[]>();

            var normalizedTexts = texts.Select(NormalizeText).ToList();
            if (normalizedTexts.Any(string.IsNullOrEmpty))
                throw new ArgumentException("Texts must not be blank.", nameof(texts));

            var textHashes = normalizedTexts.Select(ComputeTextHash).ToList();
            var distinctTextHashes = textHashes.Distinct().ToList();

            var cachedEntries = await _applicationDbContext.EmbeddingCacheEntries
                .AsNoTracking()
                .Where(entry => entry.Model == model && distinctTextHashes.Contains(entry.TextHash))
                .ToListAsync(cancellationToken);

            var vectorsByTextHash = new Dictionary<string, float[]>();
            foreach (var cachedEntry in cachedEntries)
                vectorsByTextHash[cachedEntry.TextHash] = VectorSerializationHelper.ToVector(cachedEntry.Vector);

            var missingItems = new List<(string TextHash, string Text)>();
            var seenTextHashes = new HashSet<string>();
            for (int index = 0; index < textHashes.Count; index++)
            {
                var textHash = textHashes[index];
                if (vectorsByTextHash.ContainsKey(textHash) || !seenTextHashes.Add(textHash))
                    continue;

                missingItems.Add((textHash, normalizedTexts[index]));
            }

            var cacheHitCount = distinctTextHashes.Count - missingItems.Count;

            _logger.Information(
                "Embedding request. Texts: {TextCount}, CacheHits: {CacheHitCount}, ToGenerate: {GenerateCount}, Model: {Model}",
                texts.Count, cacheHitCount, missingItems.Count, model);

            var newEntries = new List<EmbeddingCacheEntry>();
            if (missingItems.Count > 0)
            {
                var embeddingGenerator = aiProviderClientFactory.CreateEmbeddingGenerator();

                foreach (var batch in missingItems.Chunk(GenerationBatchSize))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var generatedEmbeddings = await embeddingGenerator.GenerateAsync(
                        batch.Select(item => item.Text).ToList(),
                        cancellationToken: cancellationToken);

                    if (generatedEmbeddings.Count != batch.Length)
                        throw new InvalidOperationException(
                            $"Embedding generator returned {generatedEmbeddings.Count} vectors for {batch.Length} texts.");

                    for (int index = 0; index < batch.Length; index++)
                    {
                        var vector = generatedEmbeddings[index].Vector.ToArray();
                        vectorsByTextHash[batch[index].TextHash] = vector;

                        newEntries.Add(new EmbeddingCacheEntry
                        {
                            TextHash = batch[index].TextHash,
                            Model = model,
                            Dimensions = vector.Length,
                            Vector = VectorSerializationHelper.ToBytes(vector)
                        });
                    }
                }
            }
            _aiUsageTracker.RecordEmbeddingUsage(cacheHitCount, missingItems.Count);
            await TrySaveNewEntriesAsync(newEntries, cancellationToken);
            return textHashes.Select(textHash => vectorsByTextHash[textHash]).ToList();
        }

        private async Task TrySaveNewEntriesAsync(List<EmbeddingCacheEntry> newEntries, CancellationToken cancellationToken)
        {
            if (newEntries.Count == 0)
                return;

            try
            {
                await _applicationDbContext.EmbeddingCacheEntries.AddRangeAsync(newEntries, cancellationToken);
                await _applicationDbContext.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.Warning(exception, "Embedding cache could not be saved. Vectors are still returned for this request.");

                foreach (var newEntry in newEntries)
                    _applicationDbContext.Entry(newEntry).State = EntityState.Detached;
            }
        }

        private static string NormalizeText(string text)
        {
            return (text ?? string.Empty).Replace("\r\n", "\n").Trim();
        }

        private static string ComputeTextHash(string normalizedText)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalizedText)));
        }
    }
}
