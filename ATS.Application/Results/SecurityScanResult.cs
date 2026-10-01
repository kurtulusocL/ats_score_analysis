

namespace ATS.Application.Results
{
    public sealed record SecurityScanResult(
       IReadOnlyList<SecurityFindingResult> Findings,
       IReadOnlyList<string> Warnings);
}
