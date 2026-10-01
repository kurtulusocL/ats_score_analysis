
namespace ATS.Application.Abstract.AI
{
    public class AiUsageTracker
    {
        private readonly object _syncRoot = new();
        private long _inputTokenCount;
        private bool _hasInputTokenCount;
        private long _outputTokenCount;
        private bool _hasOutputTokenCount;
        private bool _hasEmbeddingUsage;
        private long _embeddingCacheHitCount;
        private long _embeddingGeneratedCount;
        public void RecordChatUsage(long? inputTokenCount, long? outputTokenCount)
        {
            lock (_syncRoot)
            {
                if (inputTokenCount is >= 0)
                {
                    _inputTokenCount = AddSaturating(_inputTokenCount, inputTokenCount.Value);
                    _hasInputTokenCount = true;
                }

                if (outputTokenCount is >= 0)
                {
                    _outputTokenCount = AddSaturating(_outputTokenCount, outputTokenCount.Value);
                    _hasOutputTokenCount = true;
                }
            }
        }

        public void RecordEmbeddingUsage(int cacheHitCount, int generatedCount)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(cacheHitCount);
            ArgumentOutOfRangeException.ThrowIfNegative(generatedCount);

            lock (_syncRoot)
            {
                _embeddingCacheHitCount = AddSaturating(_embeddingCacheHitCount, cacheHitCount);
                _embeddingGeneratedCount = AddSaturating(_embeddingGeneratedCount, generatedCount);
                _hasEmbeddingUsage = true;
            }
        }

        public void Reset()
        {
            lock (_syncRoot)
            {
                _inputTokenCount = 0;
                _hasInputTokenCount = false;
                _outputTokenCount = 0;
                _hasOutputTokenCount = false;
                _hasEmbeddingUsage = false;
                _embeddingCacheHitCount = 0;
                _embeddingGeneratedCount = 0;
            }
        }

        public AiUsageSnapshot GetSnapshot()
        {
            lock (_syncRoot)
            {
                return new AiUsageSnapshot(
                    _hasInputTokenCount ? ToInt(_inputTokenCount) : null,
                    _hasOutputTokenCount ? ToInt(_outputTokenCount) : null,
                    _hasEmbeddingUsage ? ToInt(_embeddingCacheHitCount) : null,
                    _hasEmbeddingUsage ? ToInt(_embeddingGeneratedCount) : null);
            }
        }

        private static long AddSaturating(long current, long value) =>
            value > long.MaxValue - current ? long.MaxValue : current + value;

        private static int ToInt(long value) => (int)Math.Min(value, int.MaxValue);
    }
}
