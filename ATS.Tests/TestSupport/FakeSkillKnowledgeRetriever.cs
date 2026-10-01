using ATS.Application.Abstract.AI;
using ATS.Application.Knowledge;
using ATS.Application.Results;

namespace ATS.Tests.TestSupport
{
    public sealed class FakeSkillKnowledgeRetriever(Func<IReadOnlyList<ExtractedRequirement>, IReadOnlyList<RequirementKnowledge>> respond) : ISkillKnowledgeRetriever
    {
        public int CallCount { get; private set; }
        public List<IReadOnlyList<ExtractedRequirement>> ReceivedRequirementLists { get; } = new();

        public static FakeSkillKnowledgeRetriever WithoutHits() =>
            new(requirements => requirements
                .Select(requirement => new RequirementKnowledge(requirement, Array.Empty<SkillKnowledgeHit>()))
                .ToList());

        public Task<IReadOnlyList<RequirementKnowledge>> RetrieveAsync(
            IReadOnlyList<ExtractedRequirement> requirements, CancellationToken cancellationToken = default)
        {
            CallCount++;
            ReceivedRequirementLists.Add(requirements);
            return Task.FromResult(respond(requirements));
        }
    }
}
