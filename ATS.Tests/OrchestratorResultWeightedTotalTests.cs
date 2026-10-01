using ATS.Application.Analyzers.Base.Models;
using ATS.Application.Results;

namespace ATS.Tests
{
    public class OrchestratorResultWeightedTotalTests
    {
        private static AnalyzerResult Section(string name, int score, int maxScore) =>
            new() { SectionName = name, Score = score, MaxScore = maxScore };

        // CV kalitesi 75/80, yani %93.75.
        private static List<AnalyzerResult> QualitySections() => new()
        {
            Section("Consistency", 24, 25),
            Section("Format", 15, 15),
            Section("Keyword", 16, 20),
            Section("Section Presence", 20, 20)
        };

        private static OrchestratorResult Recalculate(int jobMatchScore, int jobMatchMaxScore = 20)
        {
            var sections = QualitySections();
            sections.Add(Section("Job Match", jobMatchScore, jobMatchMaxScore));
            var result = new OrchestratorResult { AnalyzerResults = sections };
            result.RecalculateTotals();
            return result;
        }

        [Fact]
        public void RecalculateTotals_TakesSeventyPercentFromTheJobMatch_WhenThereIsAJobPosting()
        {
            Assert.Equal(60, Recalculate(jobMatchScore: 9).TotalScore);
            Assert.Equal(46, Recalculate(jobMatchScore: 5).TotalScore);
            Assert.Equal(28, Recalculate(jobMatchScore: 0).TotalScore);
        }

        [Fact]
        public void RecalculateTotals_CannotReachAHighTotal_WhenTheCvDoesNotFitThePosting()
        {
            Assert.True(Recalculate(jobMatchScore: 5).TotalScore < 50);
        }

        [Fact]
        public void RecalculateTotals_Gives100_WhenTheCvIsPerfectAndMatchesThePostingCompletely()
        {
            var sections = new List<AnalyzerResult> { Section("Consistency", 25, 25), Section("Job Match", 20, 20) };
            var result = new OrchestratorResult { AnalyzerResults = sections };

            result.RecalculateTotals();

            Assert.Equal(100, result.TotalScore);
            Assert.True(result.IsGenerallyPassed);
        }

        [Fact]
        public void RecalculateTotals_SumsTheScores_WhenThereIsNoJobMatch()
        {
            var result = new OrchestratorResult
            {
                AnalyzerResults = new List<AnalyzerResult> { Section("Consistency", 30, 31), Section("Format", 19, 19) }
            };

            result.RecalculateTotals();

            Assert.Equal(49, result.TotalScore);
            Assert.Equal(100, result.TotalMaxScore);
        }

        [Fact]
        public void RecalculateTotals_CapsANoJobMatchTotalAt100_AndStillPasses()
        {
            var result = new OrchestratorResult
            {
                AnalyzerResults = new List<AnalyzerResult> { Section("A", 70, 70), Section("B", 40, 40) }
            };

            result.RecalculateTotals();

            Assert.Equal(100, result.TotalScore);
            Assert.True(result.IsGenerallyPassed);
        }

        [Fact]
        public void RecalculateTotals_TreatsAJobMatchWithoutAMaximumAsMissing()
        {
            var result = Recalculate(jobMatchScore: 0, jobMatchMaxScore: 0);

            Assert.Equal(75, result.TotalScore);
        }

        private static OrchestratorResult RecalculateWithoutJobMatch(int score)
        {
            var result = new OrchestratorResult
            {
                AnalyzerResults = new List<AnalyzerResult> { Section("Consistency", score, 100) }
            };
            result.RecalculateTotals();
            return result;
        }

        [Fact]
        public void RecalculateTotals_PassesAtExactlyTheThreshold_AndFailsBelowIt()
        {
            Assert.True(RecalculateWithoutJobMatch(OverallScoreCalculator.GeneralPassThreshold).IsGenerallyPassed);
            Assert.False(RecalculateWithoutJobMatch(OverallScoreCalculator.GeneralPassThreshold - 1).IsGenerallyPassed);
        }

        [Fact]
        public void RecalculateTotals_WithAJobMatch_PassesFromAJobMatchScoreOf14()
        {
            Assert.False(Recalculate(jobMatchScore: 13).IsGenerallyPassed);
            Assert.True(Recalculate(jobMatchScore: 14).IsGenerallyPassed);
        }

        [Fact]
        public void RecalculateTotals_FollowsAJobMatchScoreThatChangesLater()
        {
            var result = Recalculate(jobMatchScore: 5);
            Assert.Equal(46, result.TotalScore);

            result.AnalyzerResults.Single(section => section.SectionName == "Job Match").Score = 12;
            result.RecalculateTotals();

            Assert.Equal(70, result.TotalScore);
        }
    }
}
