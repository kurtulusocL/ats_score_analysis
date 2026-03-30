using ATS.Domain.Entities.Base;

namespace ATS.Domain.Entities
{
    public class JobPosting:BaseEntity
    {       
        public string Title { get; set; } = string.Empty;
        public string RawText { get; set; } = string.Empty;
        public int MatchScore { get; set; }

        public int CvScanId { get; set; }
        public virtual CvScan CvScan { get; set; } = null!;
    }
}
