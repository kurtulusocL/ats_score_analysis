using ATS.Application.Analyzers.Base;
using ATS.Application.Analyzers.Base.Models;
using ATS.Application.Results;

namespace ATS.Application.Analyzers
{
    public class ScoringOrchestrator
    {
        private readonly IEnumerable<IAnalyzer> _analyzers;

        public ScoringOrchestrator(IEnumerable<IAnalyzer> analyzers)
        {
            _analyzers = analyzers;
        }

        public OrchestratorResult Run(string cvText, string fileName, string? jobDescription = null)
        {
            var results = new List<AnalyzerResult>();

            foreach (var analyzer in _analyzers)
            {
                if (analyzer.SectionName == "Job Match" && string.IsNullOrWhiteSpace(jobDescription))
                    continue;

                var result = analyzer.Analyze(cvText, jobDescription);
                results.Add(result);
            }
            return BuildResult(results, fileName);
        }

        private OrchestratorResult BuildResult(List<AnalyzerResult> results, string fileName)
        {
            var hasJobMatch = results.Any(r => r.SectionName == "Job Match");
            if (!hasJobMatch)
            {
                foreach (var res in results)
                {
                    res.Score = (int)Math.Round(res.Score * 1.25);
                    res.MaxScore = (int)Math.Round(res.MaxScore * 1.25);
                }
            }
            var totalScore = results.Sum(r => r.Score);
            var totalMaxScore = results.Sum(r => r.MaxScore);

            return new OrchestratorResult
            {
                FileName = fileName,
                AnalyzerResults = results,
                TotalScore = Math.Min(totalScore, 100),
                TotalMaxScore = 100,
                IsGenerallyPassed = totalScore >= 60
            };
        }
    }
}
