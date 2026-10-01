using ATS.Domain.Entities;
using ATS.Domain.Enums;

namespace ATS.Application.Security
{
    public static class SecurityReportLineBuilder
    {
        public static IReadOnlyList<SecurityReportLine> BuildFindingLines(IEnumerable<SecurityFinding>? securityFindings)
        {
            if (securityFindings == null)
                return Array.Empty<SecurityReportLine>();

            return OrderHighSeverityFirst(securityFindings.Where(securityFinding => securityFinding.Type != SecurityFindingType.HiddenText))
                .Select(securityFinding => new SecurityReportLine(
                    securityFinding.Severity,
                    $"{SecurityFindingMessageFormatter.GetTypeLabel(securityFinding.Type)}: {SecurityFindingTextShortener.Shorten(securityFinding.Snippet, securityFinding.Description)}"))
                .ToList();
        }

        public static IReadOnlyList<SecurityReportLine> BuildHiddenTextLines(IEnumerable<SecurityFinding>? securityFindings)
        {
            if (securityFindings == null)
                return Array.Empty<SecurityReportLine>();

            return OrderHighSeverityFirst(securityFindings.Where(securityFinding => securityFinding.Type == SecurityFindingType.HiddenText))
                .Select(securityFinding => new SecurityReportLine(securityFinding.Severity, BuildHiddenTextLineText(securityFinding)))
                .ToList();
        }

        public static IReadOnlyList<string> BuildWarningLines(IEnumerable<AnalysisWarning>? analysisWarnings)
        {
            if (analysisWarnings == null)
                return Array.Empty<string>();

            return analysisWarnings
                .Select(analysisWarning => analysisWarning.Message)
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .ToList();
        }

        private static IEnumerable<SecurityFinding> OrderHighSeverityFirst(IEnumerable<SecurityFinding> securityFindings) =>
            securityFindings.OrderBy(securityFinding => securityFinding.Severity == SecurityFindingSeverity.High ? 0 : 1);

        private static string BuildHiddenTextLineText(SecurityFinding securityFinding) =>
            string.IsNullOrWhiteSpace(securityFinding.Snippet)
                ? securityFinding.Description
                : $"{securityFinding.Description}: {securityFinding.Snippet}";
    }
}
