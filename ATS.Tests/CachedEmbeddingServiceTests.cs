using ATS.Application.Abstract.AI;
using ATS.Infrastructure.Concrete.AI;
using ATS.Infrastructure.Concrete.ServiceManagers;
using ATS.Infrastructure.Persistence.Context.Mssql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

namespace ATS.Tests
{
    public class CachedEmbeddingServiceTests
    {
        private sealed class FakeEmbeddingGenerator(Exception? exceptionToThrow = null) : IEmbeddingGenerator<string, Embedding<float>>
        {
            public List<List<string>> ReceivedBatches { get; } = new();

            public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
                IEnumerable<string> values,
                EmbeddingGenerationOptions? options = null,
                CancellationToken cancellationToken = default)
            {
                var batch = values.ToList();
                ReceivedBatches.Add(batch);

                if (exceptionToThrow != null)
                    throw exceptionToThrow;

                var embeddings = batch
                    .Select(text => new Embedding<float>(new float[] { text.Length, text.Sum(character => character) % 97, 1f }))
                    .ToList();

                return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(embeddings));
            }

            public object? GetService(Type serviceType, object? serviceKey = null) => null;

            public void Dispose() { }
        }

        private sealed class FakeProviderClientFactory(string embeddingModelIdentity, FakeEmbeddingGenerator generator) : IAiProviderClientFactory
        {
            public string ProviderName => "Fake";
            public string ChatModelIdentity => "Fake:chat-model";
            public string EmbeddingModelIdentity => embeddingModelIdentity;
            public IChatClient CreateChatClient() => throw new NotSupportedException();
            public IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator() => generator;
        }

        private static CachedEmbeddingManager CreateService(
            FakeEmbeddingGenerator generator, string databaseName, string model, AiUsageTracker? aiUsageTracker = null)
        {
            var applicationDbContext = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName).Options);

            var aiProviderSelection = new AiProviderSelection();
            aiProviderSelection.Select("Fake");

            var resolver = new AiProviderClientResolver(
                aiProviderSelection,
                new IAiProviderClientFactory[] { new FakeProviderClientFactory(model, generator) });

            return new CachedEmbeddingManager(resolver, applicationDbContext, aiUsageTracker ?? new AiUsageTracker());
        }

        [Fact]
        public async Task GetEmbeddingsAsync_SecondCallWithSameTexts_DoesNotCallGeneratorAgain()
        {
            var databaseName = Guid.NewGuid().ToString();
            var generator = new FakeEmbeddingGenerator();

            var firstResult = await CreateService(generator, databaseName, "model-a").GetEmbeddingsAsync(new[] { "C#", "SQL" });
            var secondResult = await CreateService(generator, databaseName, "model-a").GetEmbeddingsAsync(new[] { "C#", "SQL" });

            Assert.Single(generator.ReceivedBatches);
            Assert.Equal(firstResult[0], secondResult[0]);
            Assert.Equal(firstResult[1], secondResult[1]);
        }

        [Fact]
        public async Task GetEmbeddingsAsync_SendsOnlyMissingTextsToGenerator()
        {
            var databaseName = Guid.NewGuid().ToString();
            var generator = new FakeEmbeddingGenerator();

            await CreateService(generator, databaseName, "model-a").GetEmbeddingsAsync(new[] { "alpha", "beta" });
            await CreateService(generator, databaseName, "model-a").GetEmbeddingsAsync(new[] { "beta", "gamma" });

            Assert.Equal(2, generator.ReceivedBatches.Count);
            Assert.Equal(new[] { "gamma" }, generator.ReceivedBatches[1]);
        }

        [Fact]
        public async Task GetEmbeddingsAsync_KeepsInputOrder_AndSendsDuplicatesOnce()
        {
            var generator = new FakeEmbeddingGenerator();
            var service = CreateService(generator, Guid.NewGuid().ToString(), "model-a");

            var result = await service.GetEmbeddingsAsync(new[] { "first", "second", "first" });

            Assert.Equal(3, result.Count);
            Assert.Equal(result[0], result[2]);
            Assert.NotEqual(result[0], result[1]);
            Assert.Equal(new[] { "first", "second" }, generator.ReceivedBatches.Single());
        }

        [Fact]
        public async Task GetEmbeddingsAsync_DoesNotShareCacheBetweenModelIdentities()
        {
            var databaseName = Guid.NewGuid().ToString();
            var generator = new FakeEmbeddingGenerator();

            await CreateService(generator, databaseName, "Ollama:model-a").GetEmbeddingsAsync(new[] { "Docker" });
            await CreateService(generator, databaseName, "OpenAiCompatible:model-a").GetEmbeddingsAsync(new[] { "Docker" });

            Assert.Equal(2, generator.ReceivedBatches.Count);
        }

        [Fact]
        public async Task GetEmbeddingsAsync_TreatsWhitespaceAndLineEndingDifferencesAsSameText()
        {
            var databaseName = Guid.NewGuid().ToString();
            var generator = new FakeEmbeddingGenerator();

            await CreateService(generator, databaseName, "model-a").GetEmbeddingsAsync(new[] { "Kubernetes" });
            await CreateService(generator, databaseName, "model-a").GetEmbeddingsAsync(new[] { "  Kubernetes \r\n" });

            Assert.Single(generator.ReceivedBatches);
        }

        [Fact]
        public async Task GetEmbeddingsAsync_DoesNotCreateGenerator_WhenEverythingIsCached()
        {
            var databaseName = Guid.NewGuid().ToString();
            var generator = new FakeEmbeddingGenerator();

            await CreateService(generator, databaseName, "model-a").GetEmbeddingsAsync(new[] { "Redis" });
            var cachedOnlyResult = await CreateService(generator, databaseName, "model-a").GetEmbeddingsAsync(new[] { "Redis" });

            Assert.Single(cachedOnlyResult);
            Assert.Single(generator.ReceivedBatches);
        }

        [Fact]
        public async Task GetEmbeddingsAsync_Throws_WhenEmbeddingModelIsNotConfigured()
        {
            var service = CreateService(new FakeEmbeddingGenerator(), Guid.NewGuid().ToString(), string.Empty);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetEmbeddingsAsync(new[] { "text" }));
        }

        [Fact]
        public async Task GetEmbeddingsAsync_Throws_WhenNoProviderHasBeenSelected()
        {
            var applicationDbContext = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            var resolver = new AiProviderClientResolver(new AiProviderSelection(), Array.Empty<IAiProviderClientFactory>());
            var service = new CachedEmbeddingManager(resolver, applicationDbContext, new AiUsageTracker());

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetEmbeddingsAsync(new[] { "text" }));
        }

        [Fact]
        public async Task GetEmbeddingsAsync_Throws_WhenATextIsBlank()
        {
            var service = CreateService(new FakeEmbeddingGenerator(), Guid.NewGuid().ToString(), "model-a");

            await Assert.ThrowsAsync<ArgumentException>(() => service.GetEmbeddingsAsync(new[] { "valid", "   " }));
        }

        [Fact]
        public async Task GetEmbeddingsAsync_RecordsEachGeneratedTextOnce_EvenWhenItIsRepeatedInTheRequest()
        {
            var aiUsageTracker = new AiUsageTracker();
            var service = CreateService(new FakeEmbeddingGenerator(), Guid.NewGuid().ToString(), "model-a", aiUsageTracker);

            await service.GetEmbeddingsAsync(new[] { "first", "second", "first" });

            var snapshot = aiUsageTracker.GetSnapshot();
            Assert.Equal(0, snapshot.EmbeddingCacheHitCount);
            Assert.Equal(2, snapshot.EmbeddingGeneratedCount);
        }

        [Fact]
        public async Task GetEmbeddingsAsync_RecordsCacheHitsAndZeroGeneratedTexts_ForAFullyCachedRequest()
        {
            var databaseName = Guid.NewGuid().ToString();
            var generator = new FakeEmbeddingGenerator();
            await CreateService(generator, databaseName, "model-a").GetEmbeddingsAsync(new[] { "alpha", "beta" });
            var aiUsageTracker = new AiUsageTracker();

            await CreateService(generator, databaseName, "model-a", aiUsageTracker).GetEmbeddingsAsync(new[] { "alpha", "beta" });

            var snapshot = aiUsageTracker.GetSnapshot();
            Assert.Equal(2, snapshot.EmbeddingCacheHitCount);
            Assert.Equal(0, snapshot.EmbeddingGeneratedCount);
        }

        [Fact]
        public async Task GetEmbeddingsAsync_RecordsBothCounts_ForARequestThatIsPartlyCached()
        {
            var databaseName = Guid.NewGuid().ToString();
            var generator = new FakeEmbeddingGenerator();
            await CreateService(generator, databaseName, "model-a").GetEmbeddingsAsync(new[] { "alpha", "beta" });
            var aiUsageTracker = new AiUsageTracker();

            await CreateService(generator, databaseName, "model-a", aiUsageTracker).GetEmbeddingsAsync(new[] { "beta", "gamma" });

            var snapshot = aiUsageTracker.GetSnapshot();
            Assert.Equal(1, snapshot.EmbeddingCacheHitCount);
            Assert.Equal(1, snapshot.EmbeddingGeneratedCount);
        }

        [Fact]
        public async Task GetEmbeddingsAsync_RecordsNothing_WhenTheRequestIsRejected()
        {
            var aiUsageTracker = new AiUsageTracker();
            var service = CreateService(new FakeEmbeddingGenerator(), Guid.NewGuid().ToString(), "model-a", aiUsageTracker);

            await Assert.ThrowsAsync<ArgumentException>(() => service.GetEmbeddingsAsync(new[] { "valid", "   " }));

            Assert.Equal(AiUsageSnapshot.Empty, aiUsageTracker.GetSnapshot());
        }

        [Fact]
        public async Task GetEmbeddingsAsync_RecordsNothing_WhenTheGenerationFails()
        {
            var aiUsageTracker = new AiUsageTracker();
            var generator = new FakeEmbeddingGenerator(new InvalidOperationException("embedding model crashed"));
            var service = CreateService(generator, Guid.NewGuid().ToString(), "model-a", aiUsageTracker);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetEmbeddingsAsync(new[] { "text" }));

            Assert.Equal(AiUsageSnapshot.Empty, aiUsageTracker.GetSnapshot());
        }
    }
}
