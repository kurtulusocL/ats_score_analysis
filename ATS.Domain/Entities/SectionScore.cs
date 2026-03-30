using ATS.Domain.Entities.Base;

namespace ATS.Domain.Entities
{
    public class SectionScore : BaseEntity
    {
        public string SectionName { get; set; } = string.Empty;
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public bool IsPassed { get; set; }

        public int CvScanId { get; set; }
        public virtual CvScan CvScan { get; set; } = null!;
    }
}
