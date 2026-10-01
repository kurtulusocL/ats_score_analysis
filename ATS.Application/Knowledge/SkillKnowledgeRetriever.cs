using ATS.Application.Abstract.AI;
using ATS.Application.Options;
using ATS.Application.Results;
using ATS.Core.Helpers;

namespace ATS.Application.Knowledge
{
    public class SkillKnowledgeRetriever : ISkillKnowledgeRetriever
    {
        private readonly ISkillKnowledgeSource _skillKnowledgeSource;
        private readonly IEmbeddingService _embeddingService;
        private readonly SemanticOptions _semanticOptions;

        public SkillKnowledgeRetriever(
            ISkillKnowledgeSource skillKnowledgeSource,
            IEmbeddingService embeddingService,
            SemanticOptions semanticOptions)
        {
            _skillKnowledgeSource = skillKnowledgeSource;
            _embeddingService = embeddingService;
            _semanticOptions = semanticOptions;

            if (_semanticOptions.KnowledgeHitsPerRequirement <= 0)
                throw new InvalidOperationException("KnowledgeHitsPerRequirement must be greater than zero.");
        }

        public async Task<IReadOnlyList<RequirementKnowledge>> RetrieveAsync(
            IReadOnlyList<ExtractedRequirement> requirements,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(requirements);

            if (requirements.Count == 0)
                return Array.Empty<RequirementKnowledge>();

            var entries = await _skillKnowledgeSource.GetActiveEntriesAsync(cancellationToken);
            if (entries.Count == 0)
                return requirements.Select(requirement => new RequirementKnowledge(requirement, Array.Empty<SkillKnowledgeHit>())).ToList();

            // Her taksonomi kaydı tek parçadır. Gömme servisi vektörleri önbelleğe yazar; sonraki analizlerde yeniden üretilmez.
            var textsToEmbed = entries
                .Select(SkillKnowledgeTextBuilder.Build)
                .Concat(requirements.Select(requirement => requirement.Name))
                .ToList();

            var embeddings = await _embeddingService.GetEmbeddingsAsync(textsToEmbed, cancellationToken);

            if (embeddings.Count != textsToEmbed.Count)
                throw new InvalidOperationException(
                    $"Embedding service returned {embeddings.Count} vectors for {textsToEmbed.Count} texts.");

            var entryVectors = embeddings.Take(entries.Count).ToList();

            return requirements
                .Select((requirement, index) => new RequirementKnowledge(
                    requirement,
                    SkillKnowledgeRanker.Rank(
                        embeddings[entries.Count + index],
                        entries,
                        entryVectors,
                        _semanticOptions.KnowledgeHitsPerRequirement,
                        _semanticOptions.KnowledgeSimilarityThreshold)))
                .ToList();
        }
    }
}
