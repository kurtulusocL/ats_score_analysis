using ATS.Domain.Entities.Base;

namespace ATS.Domain.Entities
{
    public class JobRequirement:BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public bool IsMandatory { get; set; } = true;
        public string Category { get; set; } = string.Empty;

        public int JobPostingId { get; set; }
        public virtual JobPosting JobPosting { get; set; } = null!;

        public virtual ICollection<RequirementMatch> RequirementMatches { get; set; } = new List<RequirementMatch>();
    }
}
