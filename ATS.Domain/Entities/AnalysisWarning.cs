using ATS.Domain.Entities.Base;

namespace ATS.Domain.Entities
{
    public class AnalysisWarning:BaseEntity
    {
        public string Message { get; set; } = string.Empty;

        public int CvScanId { get; set; }
        public virtual CvScan CvScan { get; set; } = null!;
    }
}
