using ATS.Domain.Entities.Base;
using ATS.Domain.Enums;

namespace ATS.Domain.Entities
{
    public class SecurityFinding:BaseEntity
    {
        public SecurityFindingType Type { get; set; }
        public SecurityFindingSeverity Severity { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Snippet { get; set; } = string.Empty;

        public int CvScanId { get; set; }
        public virtual CvScan CvScan { get; set; } = null!;
    }
}
