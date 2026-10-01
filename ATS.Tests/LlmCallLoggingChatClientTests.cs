using ATS.Application.Abstract.AI;
using ATS.Infrastructure.Concrete.AI;
using Microsoft.Extensions.AI;

namespace ATS.Tests
{
    public class LlmCallLoggingChatClientTests
    {
        private sealed class FakeChatClient(Func<ChatResponse> respond) : IChatClient
        {
            public Task<ChatResponse> GetResponseAsync(
                IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
                Task.FromResult(respond());

            public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
                IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
                throw new NotSupportedException();

            public object? GetService(Type serviceType, object? serviceKey = null) => null;

            public void Dispose() { }
        }

        private static readonly ChatMessage[] Prompt = { new(ChatRole.User, "hello") };

        private static LlmCallLoggingChatClient CreateClient(Func<ChatResponse> respond, AiUsageTracker? aiUsageTracker = null) =>
            new(new FakeChatClient(respond), aiUsageTracker ?? new AiUsageTracker());

        private static ChatResponse ResponseWithUsage(long? inputTokenCount, long? outputTokenCount) =>
            new(new ChatMessage(ChatRole.Assistant, "ok"))
            {
                Usage = new UsageDetails { InputTokenCount = inputTokenCount, OutputTokenCount = outputTokenCount }
            };

        [Fact]
        public async Task GetResponseAsync_ReturnsInnerResponseUnchanged()
        {
            var expected = new ChatResponse(new ChatMessage(ChatRole.Assistant, "ok"));
            using var client = CreateClient(() => expected);

            var actual = await client.GetResponseAsync(Prompt);

            Assert.Same(expected, actual);
        }

        [Fact]
        public async Task GetResponseAsync_RethrowsInnerException()
        {
            using var client = CreateClient(() => throw new InvalidOperationException("model crashed"));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetResponseAsync(Prompt));

            Assert.Equal("model crashed", ex.Message);
        }

        [Fact]
        public async Task GetResponseAsync_RethrowsCancellation()
        {
            using var client = CreateClient(() => throw new OperationCanceledException());

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetResponseAsync(Prompt));
        }

        [Fact]
        public async Task GetResponseAsync_RecordsTheReportedTokenCounts()
        {
            var aiUsageTracker = new AiUsageTracker();
            using var client = CreateClient(() => ResponseWithUsage(12, 7), aiUsageTracker);

            await client.GetResponseAsync(Prompt);

            var snapshot = aiUsageTracker.GetSnapshot();
            Assert.Equal(12, snapshot.InputTokenCount);
            Assert.Equal(7, snapshot.OutputTokenCount);
        }

        [Fact]
        public async Task GetResponseAsync_AddsUpTheTokenCountsOfSeveralCalls()
        {
            var aiUsageTracker = new AiUsageTracker();
            using var client = CreateClient(() => ResponseWithUsage(12, 7), aiUsageTracker);

            await client.GetResponseAsync(Prompt);
            await client.GetResponseAsync(Prompt);

            var snapshot = aiUsageTracker.GetSnapshot();
            Assert.Equal(24, snapshot.InputTokenCount);
            Assert.Equal(14, snapshot.OutputTokenCount);
        }

        [Fact]
        public async Task GetResponseAsync_RecordsOnlyTheCountThatWasReported()
        {
            var aiUsageTracker = new AiUsageTracker();
            using var client = CreateClient(() => ResponseWithUsage(12, null), aiUsageTracker);

            await client.GetResponseAsync(Prompt);

            var snapshot = aiUsageTracker.GetSnapshot();
            Assert.Equal(12, snapshot.InputTokenCount);
            Assert.Null(snapshot.OutputTokenCount);
        }

        [Fact]
        public async Task GetResponseAsync_RecordsNothing_WhenTheProviderReportsNoUsage()
        {
            var aiUsageTracker = new AiUsageTracker();
            using var client = CreateClient(() => new ChatResponse(new ChatMessage(ChatRole.Assistant, "ok")), aiUsageTracker);

            await client.GetResponseAsync(Prompt);

            Assert.Equal(AiUsageSnapshot.Empty, aiUsageTracker.GetSnapshot());
        }

        [Fact]
        public async Task GetResponseAsync_RecordsNothing_WhenTheCallFails()
        {
            var aiUsageTracker = new AiUsageTracker();
            using var client = CreateClient(() => throw new InvalidOperationException("model crashed"), aiUsageTracker);

            await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetResponseAsync(Prompt));

            Assert.Equal(AiUsageSnapshot.Empty, aiUsageTracker.GetSnapshot());
        }

        [Fact]
        public void Constructor_Throws_WhenTheUsageTrackerIsMissing()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new LlmCallLoggingChatClient(new FakeChatClient(() => ResponseWithUsage(1, 1)), null!));
        }
    }
}
