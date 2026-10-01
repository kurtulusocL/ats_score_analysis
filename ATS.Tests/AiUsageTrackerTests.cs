using ATS.Application.Abstract.AI;

namespace ATS.Tests
{
    public  class AiUsageTrackerTests
    {
        [Fact]
        public void GetSnapshot_ReportsNothingMeasured_WhenNothingWasRecorded()
        {
            var snapshot = new AiUsageTracker().GetSnapshot();

            Assert.Equal(AiUsageSnapshot.Empty, snapshot);
            Assert.Null(snapshot.InputTokenCount);
            Assert.Null(snapshot.OutputTokenCount);
            Assert.Null(snapshot.EmbeddingCacheHitCount);
            Assert.Null(snapshot.EmbeddingGeneratedCount);
        }

        [Fact]
        public void RecordChatUsage_AddsUpTheCountsOfSeveralCalls()
        {
            var tracker = new AiUsageTracker();

            tracker.RecordChatUsage(100, 40);
            tracker.RecordChatUsage(20, 5);

            var snapshot = tracker.GetSnapshot();
            Assert.Equal(120, snapshot.InputTokenCount);
            Assert.Equal(45, snapshot.OutputTokenCount);
        }

        [Fact]
        public void RecordChatUsage_KeepsAReportedZeroAsZero()
        {
            var tracker = new AiUsageTracker();

            tracker.RecordChatUsage(0, 0);

            var snapshot = tracker.GetSnapshot();
            Assert.Equal(0, snapshot.InputTokenCount);
            Assert.Equal(0, snapshot.OutputTokenCount);
        }

        [Fact]
        public void RecordChatUsage_TracksTheInputAndTheOutputCountSeparately()
        {
            var tracker = new AiUsageTracker();

            tracker.RecordChatUsage(50, null);

            var snapshot = tracker.GetSnapshot();
            Assert.Equal(50, snapshot.InputTokenCount);
            Assert.Null(snapshot.OutputTokenCount);
        }

        [Fact]
        public void RecordChatUsage_IgnoresMissingAndNegativeCounts()
        {
            var tracker = new AiUsageTracker();

            tracker.RecordChatUsage(null, null);
            tracker.RecordChatUsage(-5, -1);

            var snapshot = tracker.GetSnapshot();
            Assert.Null(snapshot.InputTokenCount);
            Assert.Null(snapshot.OutputTokenCount);
        }

        [Fact]
        public void RecordEmbeddingUsage_AddsUpTheCountsOfSeveralRequests()
        {
            var tracker = new AiUsageTracker();

            tracker.RecordEmbeddingUsage(cacheHitCount: 3, generatedCount: 2);
            tracker.RecordEmbeddingUsage(cacheHitCount: 1, generatedCount: 4);

            var snapshot = tracker.GetSnapshot();
            Assert.Equal(4, snapshot.EmbeddingCacheHitCount);
            Assert.Equal(6, snapshot.EmbeddingGeneratedCount);
        }

        [Fact]
        public void RecordEmbeddingUsage_ReportsZeroGeneratedTexts_ForARequestThatWasFullyCached()
        {
            var tracker = new AiUsageTracker();

            tracker.RecordEmbeddingUsage(cacheHitCount: 5, generatedCount: 0);

            var snapshot = tracker.GetSnapshot();
            Assert.Equal(5, snapshot.EmbeddingCacheHitCount);
            Assert.Equal(0, snapshot.EmbeddingGeneratedCount);
        }

        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, -1)]
        public void RecordEmbeddingUsage_Throws_WhenACountIsNegative(int cacheHitCount, int generatedCount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AiUsageTracker().RecordEmbeddingUsage(cacheHitCount, generatedCount));
        }

        [Fact]
        public void Reset_ForgetsEverythingThatWasRecorded()
        {
            var tracker = new AiUsageTracker();
            tracker.RecordChatUsage(100, 40);
            tracker.RecordEmbeddingUsage(3, 2);

            tracker.Reset();

            Assert.Equal(AiUsageSnapshot.Empty, tracker.GetSnapshot());
        }

        [Fact]
        public void GetSnapshot_LimitsLargeCountsToTheMaximumOfAnInt()
        {
            var tracker = new AiUsageTracker();

            tracker.RecordChatUsage(3_000_000_000, 3_000_000_000);

            var snapshot = tracker.GetSnapshot();
            Assert.Equal(int.MaxValue, snapshot.InputTokenCount);
            Assert.Equal(int.MaxValue, snapshot.OutputTokenCount);
        }

        [Fact]
        public void RecordChatUsage_DoesNotOverflow_WhenTheSumExceedsTheLimitsOfALong()
        {
            var tracker = new AiUsageTracker();

            tracker.RecordChatUsage(long.MaxValue, 1);
            tracker.RecordChatUsage(long.MaxValue, 1);

            var snapshot = tracker.GetSnapshot();
            Assert.Equal(int.MaxValue, snapshot.InputTokenCount);
            Assert.Equal(2, snapshot.OutputTokenCount);
        }

        [Fact]
        public void RecordChatUsage_IsExact_WhenCalledFromSeveralThreadsAtOnce()
        {
            var tracker = new AiUsageTracker();

            Parallel.For(0, 1000, _ => tracker.RecordChatUsage(1, 2));

            var snapshot = tracker.GetSnapshot();
            Assert.Equal(1000, snapshot.InputTokenCount);
            Assert.Equal(2000, snapshot.OutputTokenCount);
        }
    }
}
