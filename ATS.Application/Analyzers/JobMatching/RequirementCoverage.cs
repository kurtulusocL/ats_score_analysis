using ATS.Domain.Enums;

namespace ATS.Application.Analyzers.JobMatching
{
    public sealed record RequirementCoverage
    (
        JobPostingRequirement Requirement,
        double Coverage,
        MatchStatus Status,
        IReadOnlyList<string> MissingTerms
    );
}
