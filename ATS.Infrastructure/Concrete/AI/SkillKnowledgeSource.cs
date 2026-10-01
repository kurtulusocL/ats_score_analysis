using ATS.Application.Abstract.AI;
using ATS.Application.Knowledge;
using ATS.Infrastructure.Persistence.Context.Mssql;
using Microsoft.EntityFrameworkCore;

namespace ATS.Infrastructure.Concrete.AI
{
    public class SkillKnowledgeSource : ISkillKnowledgeSource
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public SkillKnowledgeSource(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<IReadOnlyList<SkillKnowledgeEntry>> GetActiveEntriesAsync(CancellationToken cancellationToken = default)
        {
            var skillTaxonomyEntries = await _applicationDbContext.SkillTaxonomyEntries
                .AsNoTracking()
                .Include(skillTaxonomyEntry => skillTaxonomyEntry.SkillTaxonomyAliases)
                .Where(skillTaxonomyEntry => skillTaxonomyEntry.IsActive)
                .OrderBy(skillTaxonomyEntry => skillTaxonomyEntry.Name)
                .ToListAsync(cancellationToken);

            return skillTaxonomyEntries
                .Select(skillTaxonomyEntry => new SkillKnowledgeEntry(
                    skillTaxonomyEntry.Name,
                    skillTaxonomyEntry.Category,
                    skillTaxonomyEntry.Description,
                    skillTaxonomyEntry.SkillTaxonomyAliases
                        .Select(skillTaxonomyAlias => skillTaxonomyAlias.Alias)
                        .OrderBy(alias => alias, StringComparer.Ordinal)
                        .ToList()))
                .ToList();
        }
    }
}
