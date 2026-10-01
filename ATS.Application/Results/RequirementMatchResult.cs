using ATS.Domain.Enums;

namespace ATS.Application.Results
{
    public sealed record RequirementMatchResult(ExtractedRequirement ExtractedRequirement, double Similarity, MatchStatus Status, string? Evidence);
}
