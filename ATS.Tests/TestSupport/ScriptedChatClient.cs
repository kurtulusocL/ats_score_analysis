using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATS.Tests.TestSupport
{
    public sealed class ScriptedChatClient : IChatClient
    {
        private readonly Queue<Func<string>> _replies;

        public ScriptedChatClient(params string[] replyTexts)
            : this(replyTexts.Select(replyText => (Func<string>)(() => replyText)).ToArray())
        {
        }

        public ScriptedChatClient(params Func<string>[] replies)
        {
            _replies = new Queue<Func<string>>(replies);
        }

        public List<IReadOnlyList<ChatMessage>> ReceivedConversations { get; } = new();
        public List<ChatOptions?> ReceivedOptions { get; } = new();

        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            ReceivedConversations.Add(messages.ToList());
            ReceivedOptions.Add(options);

            if (_replies.Count == 0)
                throw new InvalidOperationException("The scripted chat client has no reply left for this call.");

            var replyText = _replies.Dequeue()();
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, replyText)));
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }
}
