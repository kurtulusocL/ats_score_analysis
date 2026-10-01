using ATS.Application.Analyzers.Base;
using ATS.Application.Analyzers.Base.Models;
using ATS.Core.Helpers;
using ATS.Domain.Enums;
using System.Globalization;

namespace ATS.Application.Analyzers.JobMatching
{
    public class JobMatchAnalyzer : IAnalyzer
    {
        private const int MaximumListedMissingRequirements = 8;
        private const int MaximumListedPartialRequirements = 5;
        private const int MaximumRequirementTextLength = 90;
        public string SectionName => "Job Match";
        public int MaxScore => 20;
        private readonly string _jobPostingText;
        private readonly string _jobTitle;
        private readonly TimeProvider _timeProvider;

        public JobMatchAnalyzer(string jobPostingText, string jobTitle = "", TimeProvider? timeProvider = null)
        {
            _jobPostingText = jobPostingText;
            _jobTitle = jobTitle;
            _timeProvider = timeProvider ?? TimeProvider.System;
        }

        public AnalyzerResult Analyze(string cvText, string? jobDescription = null)
        {
            var result = new AnalyzerResult
            {
                SectionName = SectionName,
                MaxScore = MaxScore
            };

            if (string.IsNullOrWhiteSpace(_jobPostingText))
            {
                result.Suggestions.Add("No job posting provided. Skipping job match analysis.");
                result.IsPassed = false;
                return result;
            }

            var requirements = BuildRequirements();
            if (requirements.Count == 0)
            {
                result.Suggestions.Add("Job posting text could not be parsed.");
                result.IsPassed = false;
                return result;
            }

            var cvStems = RequirementTermExtractor.ExtractCvStems(cvText);
            var cvYears = ExperienceYearsCalculator.Calculate(cvText, _timeProvider.GetUtcNow().UtcDateTime);
            var coverages = RequirementCoverageCalculator.Calculate(requirements, cvStems, cvYears);

            var totalWeight = coverages.Sum(coverage => JobMatchScoringRules.GetWeight(coverage.Requirement));
            var earnedWeight = coverages.Sum(coverage =>
                JobMatchScoringRules.GetWeight(coverage.Requirement) * JobMatchScoringRules.GetCredit(coverage.Status));
            var creditRatio = earnedWeight / totalWeight;

            result.Score = (int)Math.Round(
                MaxScore * Math.Min(creditRatio, JobMatchScoringRules.MaximumCreditRatio),
                MidpointRounding.AwayFromZero);
            result.IsPassed = JobMatchScoringRules.IsPassing(result.Score, MaxScore);

            AddFeedback(result, coverages, creditRatio);
            return result;
        }

        private List<JobPostingRequirement> BuildRequirements()
        {
            var requirements = JobPostingRequirementParser.Parse(_jobPostingText).ToList();

            var titleTerms = RequirementTermExtractor.ExtractRequirementTerms(_jobTitle);
            if (titleTerms.Count > 0)
                requirements.Add(new JobPostingRequirement(_jobTitle.Trim(), true, null, titleTerms));

            return requirements;
        }

        private static void AddFeedback(AnalyzerResult result, IReadOnlyList<RequirementCoverage> coverages, double creditRatio)
        {
            var metCount = coverages.Count(coverage => coverage.Status == MatchStatus.Met);
            var partialCount = coverages.Count(coverage => coverage.Status == MatchStatus.Partial);
            var missingCount = coverages.Count - metCount - partialCount;

            result.Feedbacks.Add($"✓ {metCount} of {coverages.Count} requirements met, {partialCount} partially met, {missingCount} not met.");

            foreach (var coverage in coverages
                .Where(coverage => coverage.Status == MatchStatus.Partial && coverage.MissingTerms.Count > 0)
                .Take(MaximumListedPartialRequirements))
            {
                result.Suggestions.Add(
                    $"Partially covered: \"{Shorten(coverage.Requirement.Text)}\". Not found in the CV: {string.Join(", ", coverage.MissingTerms)}.");
            }

            foreach (var coverage in coverages
                .Where(coverage => coverage.Status == MatchStatus.Missing && coverage.Requirement.IsMandatory)
                .Take(MaximumListedMissingRequirements))
            {
                result.MissingItems.Add(Shorten(coverage.Requirement.Text));
            }

            if (creditRatio > JobMatchScoringRules.MaximumCreditRatio)
            {
                result.Feedbacks.Add(
                    $"Keyword matching cannot prove competence, so the Job Match score is limited to {FormatPercentage(JobMatchScoringRules.MaximumCreditRatio)} of its maximum.");
            }
        }

        private static string FormatPercentage(double ratio) =>
            (ratio * 100).ToString("0", CultureInfo.InvariantCulture) + "%";

        private static string Shorten(string text) => TextTruncationHelper.Truncate(text, MaximumRequirementTextLength);
    }
}
