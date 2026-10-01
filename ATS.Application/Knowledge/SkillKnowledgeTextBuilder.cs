
namespace ATS.Application.Knowledge
{
    public static class SkillKnowledgeTextBuilder
    {
        public static string Build(SkillKnowledgeEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);

            var parts = new List<string> { entry.Name.Trim() + "." };

            var aliases = entry.Aliases
                .Where(alias => !string.IsNullOrWhiteSpace(alias))
                .Select(alias => alias.Trim())
                .OrderBy(alias => alias, StringComparer.Ordinal)
                .ToList();

            if (aliases.Count > 0)
                parts.Add("Also known as: " + string.Join(", ", aliases) + ".");

            if (!string.IsNullOrWhiteSpace(entry.Description))
                parts.Add(entry.Description.Trim());

            return string.Join(" ", parts);
        }
    }
}
