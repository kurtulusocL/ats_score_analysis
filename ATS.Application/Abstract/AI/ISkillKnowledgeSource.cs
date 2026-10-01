using ATS.Application.Knowledge;

namespace ATS.Application.Abstract.AI
{
    public interface ISkillKnowledgeSource
    {
        Task<IReadOnlyList<SkillKnowledgeEntry>> GetActiveEntriesAsync(CancellationToken cancellationToken = default);
    }
}
