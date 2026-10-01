using ATS.Application.Results;
using ATS.Domain.Enums;
using System.Text;

namespace ATS.Application.Security
{
    public static class SecurityFindingMessageFormatter
    {
        public const int MaximumListedFindings = 20;
        private const string Header = "Possible prompt-injection content was found in the CV or the job posting. Hidden text in the CV was excluded from the score. Other flagged text is only reported and is still part of the analysis, so it may have influenced the score.";

        public static string? Format(IReadOnlyList<SecurityFindingResult> securityFindings)
        {
            var highSeverityFindings = securityFindings.Where(securityFinding => securityFinding.Severity == SecurityFindingSeverity.High).ToList();

            if (highSeverityFindings.Count == 0)
                return null;

            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine(Header);
            messageBuilder.AppendLine();

            foreach (var securityFinding in highSeverityFindings.Take(MaximumListedFindings))
            {
                messageBuilder.Append("• ")
                    .Append(GetTypeLabel(securityFinding.Type))
                    .Append(": ")
                    .AppendLine(SecurityFindingTextShortener.Shorten(securityFinding.Snippet, securityFinding.Description));
            }

            var remainingFindingCount = highSeverityFindings.Count - MaximumListedFindings;
            if (remainingFindingCount > 0)
            {
                messageBuilder.AppendLine();
                messageBuilder.Append("and ")
                    .Append(remainingFindingCount)
                    .Append(remainingFindingCount == 1 ? " more finding" : " more findings")
                    .AppendLine(". All findings are listed in the Feedback.");
            }

            return messageBuilder.ToString().TrimEnd();
        }

        public static string GetTypeLabel(SecurityFindingType securityFindingType) => securityFindingType switch
        {
            SecurityFindingType.HiddenText => "Hidden text",
            SecurityFindingType.InstructionPattern => "Instruction-like text",
            SecurityFindingType.JobPostingInstructionPattern => "Instruction-like text in job posting",
            _ => securityFindingType.ToString()
        };
    }
}
