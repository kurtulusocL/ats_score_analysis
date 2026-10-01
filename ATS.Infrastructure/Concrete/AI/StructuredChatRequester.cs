using Microsoft.Extensions.AI;
using Serilog;

namespace ATS.Infrastructure.Concrete.AI
{
    public class StructuredChatRequester
    {
        private const int MaximumAttempts = 2;
        private const float Temperature = 0f;
        private readonly ILogger _logger = Log.ForContext<StructuredChatRequester>();

        public async Task<TResult?> RequestAsync<TResult>(IChatClient chatClient, IReadOnlyList<ChatMessage> messages, Func<string?, (TResult? Result, IReadOnlyList<string> Errors)> check, CancellationToken cancellationToken = default) where TResult : class
        {
            var conversation = messages;

            for (var attempt = 1; attempt <= MaximumAttempts; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var response = await chatClient.GetResponseAsync(conversation, CreateOptions(), cancellationToken);
                var (result, errors) = check(response.Text);

                if (result != null)
                    return result;

                _logger.Warning("The model reply was rejected. Attempt: {Attempt}, ErrorCount: {ErrorCount}", attempt, errors.Count);
                conversation = messages.Append(new ChatMessage(ChatRole.User, BuildCorrectionRequest(errors))).ToList();
            }

            return null;
        }

        private static ChatOptions CreateOptions() => new()
        {
            Temperature = Temperature,
            ResponseFormat = ChatResponseFormat.Json
        };

        private static string BuildCorrectionRequest(IReadOnlyList<string> errors) =>
            "Your previous answer was rejected for these reasons:\n"
            + string.Join("\n", errors.Select(error => "- " + error))
            + "\nAnswer again and follow the output format exactly.";
    }
}