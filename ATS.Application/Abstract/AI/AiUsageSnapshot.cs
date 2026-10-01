

namespace ATS.Application.Abstract.AI
{
    public sealed record AiUsageSnapshot(
        int? InputTokenCount,
        int? OutputTokenCount,
        int? EmbeddingCacheHitCount,
        int? EmbeddingGeneratedCount)
    {
        public static AiUsageSnapshot Empty { get; } = new(null, null, null, null);
    }
}
