using ATS.Domain.Entities.Base;
using ATS.Domain.Enums;

namespace ATS.Domain.Entities
{
    public class RequirementMatch:BaseEntity
    {
        public double Similarity { get; set; }
        public MatchStatus Status { get; set; }
        public string? Evidence { get; set; }

        public int CvScanId { get; set; }
        public virtual CvScan CvScan { get; set; } = null!;

        public int JobRequirementId { get; set; }
        public virtual JobRequirement JobRequirement { get; set; } = null!;

        public virtual RequirementInterpretation? RequirementInterpretation { get; set; }
    }
}
