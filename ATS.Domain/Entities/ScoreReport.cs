using ATS.Domain.Entities.Base;

namespace ATS.Domain.Entities
{
    public class ScoreReport : BaseEntity
    {
        public string ReportPath { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty; 
        public bool IsGenerated { get; set; }

        public int CvScanId { get; set; }
        public virtual CvScan CvScan { get; set; } = null!;
    }
}
