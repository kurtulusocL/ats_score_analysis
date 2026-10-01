using ATS.Application.Abstract.AI;
using ATS.Application.Options;
using ATS.Application.Results;
using ATS.Core.Helpers;
using ATS.Domain.Enums;

namespace ATS.Application.Semantic
{
    public class SemanticMatcher : ISemanticMatcher
    {
        private const int MaximumEvidenceLength = 2000;

        private readonly IEmbeddingService _embeddingService;
        private readonly SemanticOptions _semanticOptions;

        public SemanticMatcher(IEmbeddingService embeddingService, SemanticOptions semanticOptions)
        {
            _embeddingService = embeddingService;
            _semanticOptions = semanticOptions;

            if (_semanticOptions.PartialSimilarityThreshold > _semanticOptions.MetSimilarityThreshold)
                throw new InvalidOperationException("PartialSimilarityThreshold must not be greater than MetSimilarityThreshold.");
        }

        public async Task<IReadOnlyList<RequirementMatchResult>> MatchAsync(
            IReadOnlyList<ExtractedRequirement> extractedRequirements,
            string cvText,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(extractedRequirements);

            if (extractedRequirements.Count == 0)
                return Array.Empty<RequirementMatchResult>();

            var cvChunks = TextChunkerHelper.Split(cvText ?? string.Empty, _semanticOptions.MaxChunkCharacters);

            if (cvChunks.Count == 0)
            {
                return extractedRequirements
                    .Select(extractedRequirement => new RequirementMatchResult(extractedRequirement, 0, MatchStatus.Missing, null))
                    .ToList();
            }

            var textsToEmbed = extractedRequirements
                .Select(extractedRequirement => extractedRequirement.Name)
                .Concat(cvChunks)
                .ToList();

            var embeddings = await _embeddingService.GetEmbeddingsAsync(textsToEmbed, cancellationToken);

            if (embeddings.Count != textsToEmbed.Count)
                throw new InvalidOperationException(
                    $"Embedding service returned {embeddings.Count} vectors for {textsToEmbed.Count} texts.");

            var chunkEmbeddings = embeddings.Skip(extractedRequirements.Count).ToList();
            var requirementMatchResults = new List<RequirementMatchResult>(extractedRequirements.Count);

            for (int requirementIndex = 0; requirementIndex < extractedRequirements.Count; requirementIndex++)
            {
                var requirementEmbedding = embeddings[requirementIndex];
                var bestSimilarity = double.MinValue;
                var bestChunkIndex = -1;

                for (int chunkIndex = 0; chunkIndex < chunkEmbeddings.Count; chunkIndex++)
                {
                    var similarity = VectorMathHelper.CosineSimilarity(requirementEmbedding, chunkEmbeddings[chunkIndex]);
                    if (similarity > bestSimilarity)
                    {
                        bestSimilarity = similarity;
                        bestChunkIndex = chunkIndex;
                    }
                }

                var status = DetermineStatus(bestSimilarity);
                var evidence = status == MatchStatus.Missing
                    ? null
                    : TruncateEvidence(cvChunks[bestChunkIndex]);

                requirementMatchResults.Add(new RequirementMatchResult(
                    extractedRequirements[requirementIndex], bestSimilarity, status, evidence));
            }

            return requirementMatchResults;
        }

        private MatchStatus DetermineStatus(double similarity)
        {
            if (similarity >= _semanticOptions.MetSimilarityThreshold) return MatchStatus.Met;
            if (similarity >= _semanticOptions.PartialSimilarityThreshold) return MatchStatus.Partial;
            return MatchStatus.Missing;
        }

        private static string TruncateEvidence(string evidence)
        {
            return evidence.Length <= MaximumEvidenceLength ? evidence : evidence[..MaximumEvidenceLength];
        }
    }
}
