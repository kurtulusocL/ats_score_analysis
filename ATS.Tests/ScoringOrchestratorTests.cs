using ATS.Application.Analyzers;
using ATS.Application.Analyzers.Base;
using ATS.Application.Analyzers.Base.Models;

namespace ATS.Tests
{
    public class ScoringOrchestratorTests
    {
        private sealed class FakeAnalyzer(string name, int score, int max) : IAnalyzer
        {
            public string SectionName => name;
            public int MaxScore => max;

            public AnalyzerResult Analyze(string cvText, string? jobDescription = null) =>
                new() { SectionName = name, Score = score, MaxScore = max };
        }

        [Fact]
        public void Run_WithoutJobPosting_ScalesScoresBy125()
        {
            var orchestrator = new ScoringOrchestrator(new IAnalyzer[]
            {
            new FakeAnalyzer("A", 20, 40),
            new FakeAnalyzer("B", 20, 40)
            });

            var result = orchestrator.Run("cv text", "cv.pdf");

            Assert.Equal(50, result.TotalScore);
            Assert.Equal(100, result.TotalMaxScore);
            Assert.All(result.AnalyzerResults, r => Assert.Equal(25, r.Score));
            Assert.False(result.IsGenerallyPassed);
        }

        [Fact]
        public void Run_WithoutJobDescription_SkipsJobMatchAnalyzer()
        {
            var orchestrator = new ScoringOrchestrator(new IAnalyzer[]
            {
            new FakeAnalyzer("Format", 10, 20),
            new FakeAnalyzer("Job Match", 10, 20)
            });

            var result = orchestrator.Run("cv text", "cv.pdf");

            Assert.Single(result.AnalyzerResults);
            Assert.DoesNotContain(result.AnalyzerResults, r => r.SectionName == "Job Match");
        }

        [Fact]
        public void Run_WithJobMatch_DoesNotScale()
        {
            var orchestrator = new ScoringOrchestrator(new IAnalyzer[]
            {
                new FakeAnalyzer("Format", 10, 20),
                new FakeAnalyzer("Job Match", 20, 20)
            });

            var result = orchestrator.Run("cv text", "cv.pdf", "job text");

            Assert.Equal(2, result.AnalyzerResults.Count);
            Assert.Equal(10, result.AnalyzerResults[0].Score);
            Assert.Equal(20, result.AnalyzerResults[0].MaxScore);
            Assert.Equal(20, result.AnalyzerResults[1].Score);
            Assert.Equal(20, result.AnalyzerResults[1].MaxScore);
            Assert.Equal(85, result.TotalScore);
        }

        [Fact]
        public void Run_CapsTotalScoreAt100()
        {
            var orchestrator = new ScoringOrchestrator(new IAnalyzer[]
            {
            new FakeAnalyzer("A", 60, 60),
            new FakeAnalyzer("Job Match", 60, 60)
            });

            var result = orchestrator.Run("cv text", "cv.pdf", "job text");

            Assert.Equal(100, result.TotalScore);
            Assert.True(result.IsGenerallyPassed);
        }
    }
}
