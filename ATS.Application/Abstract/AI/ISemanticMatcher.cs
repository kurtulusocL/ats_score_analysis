using ATS.Application.Results;

namespace ATS.Application.Abstract.AI
{
    public interface ISemanticMatcher
    {
        Task<IReadOnlyList<RequirementMatchResult>> MatchAsync(IReadOnlyList<ExtractedRequirement> extractedRequirements, string cvText, CancellationToken cancellationToken = default);
    }
}
