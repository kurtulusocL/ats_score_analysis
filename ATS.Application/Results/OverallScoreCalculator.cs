using ATS.Application.Analyzers.Base.Models;

namespace ATS.Application.Results
{
    public static class OverallScoreCalculator
    {
        public const int MaximumTotalScore = 100;
        public const int GeneralPassThreshold = 75;
        public const string JobMatchSectionName = "Job Match";
        public const double JobMatchWeight = 0.70;

        public static OverallScore Calculate(IReadOnlyList<AnalyzerResult> analyzerResults)
        {
            var uncappedTotalScore = CalculateUncappedTotalScore(analyzerResults);

            return new OverallScore(
                Math.Min(uncappedTotalScore, MaximumTotalScore),
                MaximumTotalScore,
                uncappedTotalScore >= GeneralPassThreshold);
        }

        private static int CalculateUncappedTotalScore(IReadOnlyList<AnalyzerResult> analyzerResults)
        {
            var jobMatchResult = analyzerResults.FirstOrDefault(analyzerResult => analyzerResult.SectionName == JobMatchSectionName);

            if (jobMatchResult == null || jobMatchResult.MaxScore <= 0)
                return analyzerResults.Sum(analyzerResult => analyzerResult.Score);

            var qualityResults = analyzerResults.Where(analyzerResult => analyzerResult != jobMatchResult).ToList();
            var qualityMaxScore = qualityResults.Sum(analyzerResult => analyzerResult.MaxScore);
            var qualityRatio = qualityMaxScore > 0 ? (double)qualityResults.Sum(analyzerResult => analyzerResult.Score) / qualityMaxScore : 0;
            var jobMatchRatio = (double)jobMatchResult.Score / jobMatchResult.MaxScore;

            var weightedRatio = (1 - JobMatchWeight) * qualityRatio + JobMatchWeight * jobMatchRatio;
            return (int)Math.Round(MaximumTotalScore * weightedRatio, MidpointRounding.AwayFromZero);
        }
    }
}
