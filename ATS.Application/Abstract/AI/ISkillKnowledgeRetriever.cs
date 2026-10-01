using ATS.Application.Knowledge;
using ATS.Application.Results;

namespace ATS.Application.Abstract.AI
{
    public interface ISkillKnowledgeRetriever
    {
        Task<IReadOnlyList<RequirementKnowledge>> RetrieveAsync(IReadOnlyList<ExtractedRequirement> requirements, CancellationToken cancellationToken = default);
    }
}
