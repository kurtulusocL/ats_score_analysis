

namespace ATS.Infrastructure.Persistence.Seed
{
    public sealed record SkillTaxonomySeedEntry(string Name, string Category, string Description, params string[] Aliases);
}
