using ATS.Domain.Enums;

namespace ATS.Application.Security
{
    public sealed record SecurityReportLine(SecurityFindingSeverity Severity, string Text);
}
