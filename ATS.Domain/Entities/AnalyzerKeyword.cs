using ATS.Domain.Entities.Base;

namespace ATS.Domain.Entities
{
    public class AnalyzerKeyword : BaseEntity
    {
        public string Word { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
