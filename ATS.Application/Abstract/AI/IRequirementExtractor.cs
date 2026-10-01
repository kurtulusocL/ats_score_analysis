using ATS.Application.Results;

namespace ATS.Application.Abstract.AI
{
    public interface IRequirementExtractor
    {
        Task<IReadOnlyList<ExtractedRequirement>> ExtractAsync(string jobPostingText, CancellationToken cancellationToken = default);
    }
}
