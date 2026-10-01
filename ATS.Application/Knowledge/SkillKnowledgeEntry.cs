
namespace ATS.Application.Knowledge
{
    public sealed record SkillKnowledgeEntry(string Name, string Category, string Description, IReadOnlyList<string> Aliases);
}
