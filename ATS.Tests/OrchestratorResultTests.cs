using ATS.Application.Analyzers.Base.Models;
using ATS.Application.Results;

namespace ATS.Tests
{
    public class OrchestratorResultTests
    {
        [Fact]
        public void RecalculateTotals_CapsTotalScoreAt100_ButUsesUncappedTotalForPassThreshold()
        {
            var orchestratorResult = new OrchestratorResult
            {
                AnalyzerResults = new List<AnalyzerResult>
            {
                new() { SectionName = "A", Score = 70, MaxScore = 70 },
                new() { SectionName = "B", Score = 50, MaxScore = 50 }
            }
            };

            orchestratorResult.RecalculateTotals();

            Assert.Equal(100, orchestratorResult.TotalScore);
            Assert.Equal(100, orchestratorResult.TotalMaxScore);
            Assert.True(orchestratorResult.IsGenerallyPassed);
        }

        [Fact]
        public void RecalculateTotals_MarksAsNotPassed_WhenTotalIsBelowThreshold()
        {
            var orchestratorResult = new OrchestratorResult
            {
                AnalyzerResults = new List<AnalyzerResult>
            {
                new() { SectionName = "A", Score = 30, MaxScore = 50 },
                new() { SectionName = "B", Score = 29, MaxScore = 50 }
            }
            };

            orchestratorResult.RecalculateTotals();

            Assert.Equal(59, orchestratorResult.TotalScore);
            Assert.False(orchestratorResult.IsGenerallyPassed);
        }
    }
}
