using ATS.Application.Options;
using ATS.Application.Results;
using ATS.Domain.Enums;

namespace ATS.Application.Semantic
{
    public class HybridScoreCalculator
    {
        private readonly SemanticOptions _semanticOptions;

        public HybridScoreCalculator(SemanticOptions semanticOptions)
        {
            _semanticOptions = semanticOptions;

            if (_semanticOptions.DeterministicWeight < 0 || _semanticOptions.SemanticWeight < 0
                || _semanticOptions.DeterministicWeight + _semanticOptions.SemanticWeight <= 0)
                throw new InvalidOperationException("DeterministicWeight and SemanticWeight must not be negative and must not both be zero.");
        }

        public HybridScoreResult Calculate(
            int deterministicScore,
            int maximumScore,
            IReadOnlyList<RequirementMatchResult> requirementMatchResults)
        {
            ArgumentNullException.ThrowIfNull(requirementMatchResults);

            if (requirementMatchResults.Count == 0)
                return new HybridScoreResult(deterministicScore, deterministicScore, null, false);

            double totalWeight = 0;
            double earnedWeight = 0;

            foreach (var requirementMatchResult in requirementMatchResults)
            {
                var weight = requirementMatchResult.ExtractedRequirement.IsMandatory
                    ? _semanticOptions.MandatoryRequirementWeight
                    : _semanticOptions.OptionalRequirementWeight;

                totalWeight += weight;
                earnedWeight += weight * GetCredit(requirementMatchResult.Status);
            }

            if (totalWeight <= 0)
                return new HybridScoreResult(deterministicScore, deterministicScore, null, false);

            var semanticScore = earnedWeight / totalWeight * maximumScore;

            var weightSum = _semanticOptions.DeterministicWeight + _semanticOptions.SemanticWeight;
            var hybridScore = (deterministicScore * _semanticOptions.DeterministicWeight
                               + semanticScore * _semanticOptions.SemanticWeight) / weightSum;

            var roundedHybridScore = (int)Math.Round(hybridScore, MidpointRounding.AwayFromZero);
            roundedHybridScore = Math.Clamp(roundedHybridScore, 0, maximumScore);

            return new HybridScoreResult(roundedHybridScore, deterministicScore, semanticScore, true);
        }

        private double GetCredit(MatchStatus status)
        {
            return status switch
            {
                MatchStatus.Met => 1.0,
                MatchStatus.Partial => _semanticOptions.PartialMatchCredit,
                _ => 0.0
            };
        }
    }
}
