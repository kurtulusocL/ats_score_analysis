using ATS.Domain.Entities.Base;

namespace ATS.Domain.Entities
{
    public class SkillTaxonomyEntry:BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public virtual ICollection<SkillTaxonomyAlias> SkillTaxonomyAliases { get; set; } = new List<SkillTaxonomyAlias>();
    }
}
