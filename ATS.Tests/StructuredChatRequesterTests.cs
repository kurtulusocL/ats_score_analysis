using ATS.Infrastructure.Concrete.AI;
using ATS.Tests.TestSupport;
using Microsoft.Extensions.AI;

namespace ATS.Tests
{
    public class StructuredChatRequesterTests
    {
        private const string ValidReply = "valid reply";
        private const string RejectionReason = "The reply must be the valid reply.";

        private static readonly ChatMessage[] Messages =
        {
            new(ChatRole.System, "system text"),
            new(ChatRole.User, "user text")
        };

        private static (string? Result, IReadOnlyList<string> Errors) AcceptOnlyTheValidReply(string? replyText)
        {
            if (replyText == ValidReply)
                return (ValidReply, Array.Empty<string>());

            return (null, new[] { RejectionReason });
        }

        private static Task<string?> RequestAsync(ScriptedChatClient chatClient, CancellationToken cancellationToken = default) =>
            new StructuredChatRequester().RequestAsync<string>(chatClient, Messages, AcceptOnlyTheValidReply, cancellationToken);

        [Fact]
        public async Task RequestAsync_ReturnsTheResultOfAValidFirstReply_WithASingleCall()
        {
            var chatClient = new ScriptedChatClient(ValidReply);

            var result = await RequestAsync(chatClient);

            Assert.Equal(ValidReply, result);
            Assert.Single(chatClient.ReceivedConversations);
        }

        [Fact]
        public async Task RequestAsync_AsksAgainWithTheRejectionReasons_WhenTheFirstReplyIsInvalid()
        {
            var chatClient = new ScriptedChatClient("garbage", ValidReply);

            var result = await RequestAsync(chatClient);

            Assert.Equal(ValidReply, result);
            Assert.Equal(2, chatClient.ReceivedConversations.Count);
            Assert.Equal(2, chatClient.ReceivedConversations[0].Count);

            var secondConversation = chatClient.ReceivedConversations[1];
            Assert.Equal(3, secondConversation.Count);
            Assert.Equal("system text", secondConversation[0].Text);
            Assert.Equal("user text", secondConversation[1].Text);
            Assert.Equal(ChatRole.User, secondConversation[2].Role);
            Assert.Contains(RejectionReason, secondConversation[2].Text);
        }

        [Fact]
        public async Task RequestAsync_ReturnsNull_AfterTwoInvalidReplies()
        {
            var chatClient = new ScriptedChatClient("first garbage", "second garbage");

            var result = await RequestAsync(chatClient);

            Assert.Null(result);
            Assert.Equal(2, chatClient.ReceivedConversations.Count);
        }

        [Fact]
        public async Task RequestAsync_DoesNotSendTheRejectedModelOutputBack()
        {
            var chatClient = new ScriptedChatClient("SECRET-MODEL-OUTPUT", ValidReply);

            await RequestAsync(chatClient);

            Assert.All(chatClient.ReceivedConversations[1], message => Assert.DoesNotContain("SECRET-MODEL-OUTPUT", message.Text));
        }

        [Fact]
        public async Task RequestAsync_AsksForJsonWithTemperatureZero()
        {
            var chatClient = new ScriptedChatClient(ValidReply);

            await RequestAsync(chatClient);

            var options = Assert.Single(chatClient.ReceivedOptions);
            Assert.NotNull(options);
            Assert.Equal(0f, options!.Temperature);
            Assert.IsType<ChatResponseFormatJson>(options.ResponseFormat);
        }

        [Fact]
        public async Task RequestAsync_Throws_WithoutCallingTheModel_WhenAlreadyCancelled()
        {
            var chatClient = new ScriptedChatClient(ValidReply);
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => RequestAsync(chatClient, cancellationTokenSource.Token));

            Assert.Empty(chatClient.ReceivedConversations);
        }

        [Fact]
        public async Task RequestAsync_LetsAModelFailurePropagate()
        {
            var chatClient = new ScriptedChatClient(() => throw new InvalidOperationException("model crashed"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => RequestAsync(chatClient));
        }
    }
}
