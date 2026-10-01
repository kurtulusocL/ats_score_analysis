using ATS.Domain.Enums;

namespace ATS.Application.Results
{
    public sealed record RequirementInterpretationResult(
        string RequirementName,
        MatchStatus Status,
        string? EvidenceQuote,
        string Explanation,
        string? Suggestion);
}
