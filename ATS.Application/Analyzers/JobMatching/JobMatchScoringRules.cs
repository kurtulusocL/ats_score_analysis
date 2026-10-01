using ATS.Domain.Enums;

namespace ATS.Application.Analyzers.JobMatching
{
    public static class JobMatchScoringRules
    {
        public const double MetCoverageThreshold = 0.80;
        public const double PartialCoverageThreshold = 0.40;
        public const double PartialYearsRatio = 0.50;
        public const double PartialCredit = 0.5;
        public const double MandatoryWeight = 2.0;
        public const double OptionalWeight = 1.0;
        public const double PassingScoreRatio = 0.5;
        public static bool IsPassing(int score, int maximumScore) => score >= maximumScore * PassingScoreRatio;
        public const double MaximumCreditRatio = 0.70;

        public static double GetCredit(MatchStatus status) => status switch
        {
            MatchStatus.Met => 1.0,
            MatchStatus.Partial => PartialCredit,
            _ => 0.0
        };

        public static double GetWeight(JobPostingRequirement requirement) =>
            requirement.IsMandatory ? MandatoryWeight : OptionalWeight;
    }
}
