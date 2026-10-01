using ATS.Application.Results;
using ATS.Domain.Entities;
using ATS.Domain.Enums;
using System.Globalization;

namespace ATS.Application.Reporting
{
    public static class RequirementReportBuilder
    {
        private const int DisagreementDistance = 2;

        public static RequirementReport Build(CvScan cvScan)
        {
            ArgumentNullException.ThrowIfNull(cvScan);

            var requirementMatches = cvScan.RequirementMatches
                .OrderByDescending(requirementMatch => requirementMatch.JobRequirement.IsMandatory)
                .ThenBy(requirementMatch => requirementMatch.Id)
                .ToList();

            if (requirementMatches.Count == 0)
                return RequirementReport.Empty;

            var modelIdentity = requirementMatches
                .Select(requirementMatch => requirementMatch.RequirementInterpretation?.ModelIdentity)
                .FirstOrDefault(identity => !string.IsNullOrWhiteSpace(identity));

            return new RequirementReport(requirementMatches.Select(CreateRow).ToList(), BuildScoreComponentsText(cvScan), modelIdentity);
        }

        private static RequirementReportRow CreateRow(RequirementMatch requirementMatch)
        {
            var interpretation = requirementMatch.RequirementInterpretation;
            MatchStatus? modelStatus = interpretation?.Status;

            return new RequirementReportRow(
                requirementMatch.JobRequirement.Name,
                requirementMatch.JobRequirement.IsMandatory,
                requirementMatch.Status,
                requirementMatch.Similarity,
                modelStatus,
                interpretation?.EvidenceQuote,
                interpretation?.Explanation,
                interpretation?.Suggestion,
                modelStatus.HasValue && Math.Abs((int)requirementMatch.Status - (int)modelStatus.Value) >= DisagreementDistance);
        }

        private static string? BuildScoreComponentsText(CvScan cvScan)
        {
            var audit = cvScan.AnalysisAudit;
            var jobMatchSectionScore = cvScan.SectionScores
                .FirstOrDefault(sectionScore => sectionScore.SectionName == OverallScoreCalculator.JobMatchSectionName);

            if (audit == null || audit.DeterministicJobMatchScore == null || audit.HybridJobMatchScore == null
                || jobMatchSectionScore == null || jobMatchSectionScore.MaxScore <= 0)
            {
                return null;
            }

            var maximumScore = jobMatchSectionScore.MaxScore;
            var parts = new List<string>
            {
                string.Format(CultureInfo.InvariantCulture, "Keyword score {0}/{1}", audit.DeterministicJobMatchScore.Value, maximumScore)
            };

            if (audit.SemanticJobMatchScore.HasValue)
                parts.Add(string.Format(CultureInfo.InvariantCulture, "Semantic score {0:0.0}/{1}", audit.SemanticJobMatchScore.Value, maximumScore));

            parts.Add(string.Format(CultureInfo.InvariantCulture, "Hybrid score {0}/{1}", audit.HybridJobMatchScore.Value, maximumScore));

            return string.Join(" | ", parts);
        }
    }
}
