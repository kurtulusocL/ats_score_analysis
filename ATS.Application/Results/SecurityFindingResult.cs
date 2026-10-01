using ATS.Domain.Enums;

namespace ATS.Application.Results
{
    public sealed record SecurityFindingResult(
        SecurityFindingType Type,
        SecurityFindingSeverity Severity,
        string Description,
        string Snippet);
}
