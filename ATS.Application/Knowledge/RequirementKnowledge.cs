using ATS.Application.Results;

namespace ATS.Application.Knowledge
{
    public sealed record RequirementKnowledge(ExtractedRequirement Requirement, IReadOnlyList<SkillKnowledgeHit> Hits);
}
