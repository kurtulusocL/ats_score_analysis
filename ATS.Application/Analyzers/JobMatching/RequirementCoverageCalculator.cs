using ATS.Domain.Enums;

namespace ATS.Application.Analyzers.JobMatching
{
    public class RequirementCoverageCalculator
    {
        private const double Tolerance = 1e-9;

        public static IReadOnlyList<RequirementCoverage> Calculate(
            IReadOnlyList<JobPostingRequirement> requirements,
            IReadOnlySet<string> cvStems,
            double? cvYears)
        {
            var documentFrequencies = CountDocumentFrequencies(requirements);

            return requirements
                .Select(requirement => requirement.RequiredYears.HasValue
                    ? EvaluateYears(requirement, cvYears)
                    : EvaluateTerms(requirement, cvStems, documentFrequencies, requirements.Count))
                .ToList();
        }

        private static Dictionary<string, int> CountDocumentFrequencies(IReadOnlyList<JobPostingRequirement> requirements)
        {
            var frequencies = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var term in requirements.SelectMany(requirement => requirement.Terms))
                frequencies[term.Stem] = frequencies.GetValueOrDefault(term.Stem) + 1;

            return frequencies;
        }

        private static RequirementCoverage EvaluateTerms(
            JobPostingRequirement requirement,
            IReadOnlySet<string> cvStems,
            Dictionary<string, int> documentFrequencies,
            int requirementCount)
        {
            double totalWeight = 0;
            double foundWeight = 0;
            var missingTerms = new List<string>();

            foreach (var term in requirement.Terms)
            {
                var weight = Math.Log(1.0 + (double)requirementCount / documentFrequencies[term.Stem]);
                totalWeight += weight;

                if (cvStems.Contains(term.Stem))
                    foundWeight += weight;
                else
                    missingTerms.Add(term.Display);
            }

            var coverage = totalWeight > 0 ? foundWeight / totalWeight : 0;
            var status = coverage >= JobMatchScoringRules.MetCoverageThreshold - Tolerance ? MatchStatus.Met
                : coverage >= JobMatchScoringRules.PartialCoverageThreshold - Tolerance ? MatchStatus.Partial
                : MatchStatus.Missing;

            return new RequirementCoverage(requirement, coverage, status, missingTerms);
        }

        private static RequirementCoverage EvaluateYears(JobPostingRequirement requirement, double? cvYears)
        {
            var requiredYears = requirement.RequiredYears!.Value;
            var coverage = cvYears.HasValue && requiredYears > 0 ? Math.Min(1.0, cvYears.Value / requiredYears) : 0;
            var status = coverage >= 1.0 - Tolerance ? MatchStatus.Met
                : coverage >= JobMatchScoringRules.PartialYearsRatio - Tolerance ? MatchStatus.Partial
                : MatchStatus.Missing;

            return new RequirementCoverage(requirement, coverage, status, Array.Empty<string>());
        }
    }
}
