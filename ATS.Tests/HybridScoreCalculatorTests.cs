using ATS.Application.Options;
using ATS.Application.Results;
using ATS.Application.Semantic;
using ATS.Domain.Enums;

namespace ATS.Tests
{
    public class HybridScoreCalculatorTests
    {
        private static RequirementMatchResult CreateResult(bool isMandatory, MatchStatus status) =>
        new(new ExtractedRequirement("requirement", isMandatory, "General"), 0.9, status, null);

        private static HybridScoreCalculator CreateCalculator(double deterministicWeight = 0.5, double semanticWeight = 0.5) =>
            new(new SemanticOptions { DeterministicWeight = deterministicWeight, SemanticWeight = semanticWeight });

        [Fact]
        public void Calculate_ReturnsDeterministicScore_WhenThereAreNoRequirementMatchResults()
        {
            var result = CreateCalculator().Calculate(11, 20, Array.Empty<RequirementMatchResult>());

            Assert.Equal(11, result.HybridScore);
            Assert.Equal(11, result.DeterministicScore);
            Assert.Null(result.SemanticScore);
            Assert.False(result.IsSemanticScoreApplied);
        }

        [Fact]
        public void Calculate_AveragesDeterministicAndSemanticScores_WhenAllRequirementsAreMet()
        {
            var result = CreateCalculator().Calculate(10, 20, new[] { CreateResult(true, MatchStatus.Met) });

            Assert.Equal(20.0, result.SemanticScore!.Value, 6);
            Assert.Equal(15, result.HybridScore);
            Assert.True(result.IsSemanticScoreApplied);
        }

        [Fact]
        public void Calculate_LowersScore_WhenAllRequirementsAreMissing()
        {
            var result = CreateCalculator().Calculate(20, 20, new[] { CreateResult(true, MatchStatus.Missing) });

            Assert.Equal(0.0, result.SemanticScore!.Value, 6);
            Assert.Equal(10, result.HybridScore);
        }

        [Fact]
        public void Calculate_WeightsMandatoryRequirementsTwiceAsMuchAsOptionalOnes()
        {
            var results = new[]
            {
            CreateResult(true, MatchStatus.Met),
            CreateResult(false, MatchStatus.Missing)
        };

            var result = CreateCalculator().Calculate(0, 20, results);

            Assert.Equal(20.0 * 2 / 3, result.SemanticScore!.Value, 6);
            Assert.Equal(7, result.HybridScore);
        }

        [Fact]
        public void Calculate_GivesPartialCreditForPartialMatches()
        {
            var result = CreateCalculator().Calculate(10, 20, new[] { CreateResult(true, MatchStatus.Partial) });

            Assert.Equal(10.0, result.SemanticScore!.Value, 6);
            Assert.Equal(10, result.HybridScore);
        }

        [Fact]
        public void Calculate_NormalizesWeights_WhenTheyDoNotSumToOne()
        {
            var result = CreateCalculator(deterministicWeight: 1, semanticWeight: 3)
                .Calculate(0, 20, new[] { CreateResult(true, MatchStatus.Met) });

            Assert.Equal(15, result.HybridScore);
        }

        [Fact]
        public void Constructor_Throws_WhenBothWeightsAreZero()
        {
            Assert.Throws<InvalidOperationException>(() => CreateCalculator(0, 0));
        }

        [Fact]
        public void Constructor_Throws_WhenAWeightIsNegative()
        {
            Assert.Throws<InvalidOperationException>(() => CreateCalculator(-1, 2));
        }
    }
}
