using ATS.Application.Analyzers.Base.Models;

namespace ATS.Application.Results
{
    public class OrchestratorResult
    {        
        public string FileName { get; set; }
        public int TotalScore { get; set; }
        public int TotalMaxScore { get; set; }
        public double ScorePercentage { get; set; }
        public bool IsGenerallyPassed { get; set; }
        public List<AnalyzerResult> AnalyzerResults { get; set; } = new();
    }
}
