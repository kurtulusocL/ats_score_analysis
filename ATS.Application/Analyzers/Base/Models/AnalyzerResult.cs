
namespace ATS.Application.Analyzers.Base.Models
{
    public class AnalyzerResult
    {
        public string SectionName { get; set; } = string.Empty;
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public bool IsPassed { get; set; }
        public List<string> Feedbacks { get; set; } = new();
        public List<string> MissingItems { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();

        public double ScorePercentage => MaxScore == 0 ? 0 : Math.Round((double)Score / MaxScore * 100, 1);
    }
}
