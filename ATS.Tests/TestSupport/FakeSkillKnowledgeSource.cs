using ATS.Application.Abstract.AI;
using ATS.Application.Knowledge;

namespace ATS.Tests.TestSupport
{
    public sealed class FakeSkillKnowledgeSource(IReadOnlyList<SkillKnowledgeEntry> entries) : ISkillKnowledgeSource
    {
        public int CallCount { get; private set; }

        public Task<IReadOnlyList<SkillKnowledgeEntry>> GetActiveEntriesAsync(CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(entries);
        }
    }
}
