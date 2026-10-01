using ATS.Application.Abstract.AI;
using Microsoft.Extensions.AI;
using Serilog;
using System.Diagnostics;

namespace ATS.Infrastructure.Concrete.AI
{
    public sealed class LlmCallLoggingChatClient : DelegatingChatClient
    {
        private readonly ILogger _logger = Log.ForContext<LlmCallLoggingChatClient>();
        private readonly AiUsageTracker _aiUsageTracker;

        public LlmCallLoggingChatClient(IChatClient innerClient, AiUsageTracker aiUsageTracker) : base(innerClient)
        {
            ArgumentNullException.ThrowIfNull(aiUsageTracker);
            _aiUsageTracker = aiUsageTracker;
        }

        public override async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var response = await base.GetResponseAsync(messages, options, cancellationToken);
                stopwatch.Stop();

                _aiUsageTracker.RecordChatUsage(response.Usage?.InputTokenCount, response.Usage?.OutputTokenCount);

                _logger.Information(
                    "LLM call completed. Model: {Model}, DurationMs: {DurationMs}, InputTokens: {InputTokens}, OutputTokens: {OutputTokens}",
                    response.ModelId ?? options?.ModelId,
                    stopwatch.ElapsedMilliseconds,
                    response.Usage?.InputTokenCount,
                    response.Usage?.OutputTokenCount);

                return response;
            }
            catch (OperationCanceledException)
            {
                _logger.Warning("LLM call cancelled after {DurationMs} ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "LLM call failed after {DurationMs} ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
