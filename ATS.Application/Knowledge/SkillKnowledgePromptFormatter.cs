
namespace ATS.Application.Knowledge
{
    public static class SkillKnowledgePromptFormatter
    {
        public static string? Format(IReadOnlyList<RequirementKnowledge> requirementKnowledge)
        {
            ArgumentNullException.ThrowIfNull(requirementKnowledge);

            var blocks = requirementKnowledge
                .Where(knowledge => knowledge.Hits.Count > 0)
                .Select(knowledge => string.Join("\n",
                    new[] { "Requirement: " + knowledge.Requirement.Name }
                        .Concat(knowledge.Hits.Select(hit => "- " + SkillKnowledgeTextBuilder.Build(hit.Entry)))))
                .ToList();

            return blocks.Count == 0 ? null : string.Join("\n\n", blocks);
        }
    }
}
