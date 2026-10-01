using ATS.Domain.Entities.Base;

namespace ATS.Domain.Entities
{
    public class AnalysisAudit:BaseEntity
    {
        public string AiStatus { get; set; } = string.Empty;
        public string? AiProviderName { get; set; }

        public int? DeterministicJobMatchScore { get; set; }
        public double? SemanticJobMatchScore { get; set; }
        public int? HybridJobMatchScore { get; set; }

        public long? DurationMilliseconds { get; set; }
        public int? InputTokenCount { get; set; }
        public int? OutputTokenCount { get; set; }
        public int? EmbeddingCacheHitCount { get; set; }
        public int? EmbeddingGeneratedCount { get; set; }

        public int CvScanId { get; set; }
        public virtual CvScan CvScan { get; set; } = null!;
    }
}
