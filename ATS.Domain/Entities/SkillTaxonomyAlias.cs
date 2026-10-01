using ATS.Domain.Entities.Base;

namespace ATS.Domain.Entities
{
    public class SkillTaxonomyAlias:BaseEntity
    {
        public string Alias { get; set; } = string.Empty;

        public int SkillTaxonomyEntryId { get; set; }
        public virtual SkillTaxonomyEntry SkillTaxonomyEntry { get; set; } = null!;
    }
}
