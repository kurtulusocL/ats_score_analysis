using ATS.Application.Abstract.AI;
using ATS.Application.Options;
using ATS.Infrastructure.Concrete.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;

namespace ATS.Tests
{
    public class OpenAiCompatibleClientFactoryTests
    {
        private sealed record CapturedRequest(string Uri, string? Authorization, string Body);

        private sealed class CapturingHandler(Func<HttpResponseMessage> respond) : HttpMessageHandler
        {
            public List<CapturedRequest> Requests { get; } = new();

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var body = request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
                Requests.Add(new CapturedRequest(request.RequestUri!.ToString(), request.Headers.Authorization?.ToString(), body));
                return respond();
            }
        }

        private sealed class FakeHttpClientFactory(HttpClient httpClient) : IHttpClientFactory
        {
            public HttpClient CreateClient(string name) => httpClient;
        }

        private sealed class MutableApiKeyProvider(string? apiKey) : IAiApiKeyProvider
        {
            public string? ApiKey { get; set; } = apiKey;
            public string? GetApiKey() => ApiKey;
        }

        private const string ChatCompletionJson =
            "{\"id\":\"chatcmpl-1\",\"object\":\"chat.completion\",\"created\":1,\"model\":\"chat-model\"," +
            "\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"hi\"},\"finish_reason\":\"stop\"}]," +
            "\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":1,\"total_tokens\":2}}";

        private const string EmbeddingJson =
            "{\"object\":\"list\",\"data\":[{\"object\":\"embedding\",\"index\":0,\"embedding\":[0.1,0.2,0.3]}]," +
            "\"model\":\"embedding-model\",\"usage\":{\"prompt_tokens\":1,\"total_tokens\":1}}";

        private static HttpResponseMessage JsonResponse(string json) =>
            new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

        private static Task<ChatResponse> AskAsync(IChatClient chatClient, string text) =>
            chatClient.GetResponseAsync(new[] { new ChatMessage(ChatRole.User, text) });

        private static OpenAiCompatibleClientFactory CreateFactory(
            CapturingHandler handler,
            IAiApiKeyProvider apiKeyProvider,
            string endpoint = "https://example.test/v1",
            string chatModel = "chat-model",
            string embeddingModel = "embedding-model")
        {
            var aiOptions = new AiOptions
            {
                OpenAiCompatible = new OpenAiCompatibleProviderOptions
                {
                    Endpoint = endpoint,
                    ChatModel = chatModel,
                    EmbeddingModel = embeddingModel
                }
            };

            return new OpenAiCompatibleClientFactory(
                new FakeHttpClientFactory(new HttpClient(handler)),
                Options.Create(aiOptions),
                apiKeyProvider);
        }

        [Fact]
        public async Task CreateChatClient_SendsTheRequestToTheChatCompletionsEndpointWithTheKeyAndModel()
        {
            var handler = new CapturingHandler(() => JsonResponse(ChatCompletionJson));
            var factory = CreateFactory(handler, new MutableApiKeyProvider("secret"));

            var response = await AskAsync(factory.CreateChatClient(), "hello");

            var request = Assert.Single(handler.Requests);
            Assert.Equal("https://example.test/v1/chat/completions", request.Uri);
            Assert.Equal("Bearer secret", request.Authorization);
            Assert.Contains("chat-model", request.Body);
            Assert.Contains("hello", request.Body);
            Assert.Equal("hi", response.Text);
        }

        [Fact]
        public async Task CreateEmbeddingGenerator_SendsTheRequestToTheEmbeddingsEndpointWithTheKeyAndModel()
        {
            var handler = new CapturingHandler(() => JsonResponse(EmbeddingJson));
            var factory = CreateFactory(handler, new MutableApiKeyProvider("secret"));

            var embeddings = await factory.CreateEmbeddingGenerator().GenerateAsync(new List<string> { "some text" });

            var request = Assert.Single(handler.Requests);
            Assert.Equal("https://example.test/v1/embeddings", request.Uri);
            Assert.Equal("Bearer secret", request.Authorization);
            Assert.Contains("embedding-model", request.Body);
            Assert.Single(embeddings);
            Assert.Equal(3, embeddings[0].Vector.Length);
        }

        [Fact]
        public async Task CreateChatClient_StillWorks_WithoutAnyApiKey_UsingAPlaceholder()
        {
            var handler = new CapturingHandler(() => JsonResponse(ChatCompletionJson));
            var factory = CreateFactory(handler, new MutableApiKeyProvider(null));

            var response = await AskAsync(factory.CreateChatClient(), "hello");

            var request = Assert.Single(handler.Requests);
            Assert.Equal("Bearer not-needed", request.Authorization);
            Assert.Equal("hi", response.Text);
        }

        [Fact]
        public async Task CreateChatClient_PicksUpAKeyThatIsAddedAfterTheFactoryWasCreated()
        {
            var handler = new CapturingHandler(() => JsonResponse(ChatCompletionJson));
            var apiKeyProvider = new MutableApiKeyProvider(null);
            var factory = CreateFactory(handler, apiKeyProvider);

            await AskAsync(factory.CreateChatClient(), "first");
            apiKeyProvider.ApiKey = "added-later";
            await AskAsync(factory.CreateChatClient(), "second");

            Assert.Equal(2, handler.Requests.Count);
            Assert.Equal("Bearer not-needed", handler.Requests[0].Authorization);
            Assert.Equal("Bearer added-later", handler.Requests[1].Authorization);
        }

        [Fact]
        public void EmbeddingModelIdentity_IncludesTheProviderNameSoCachesAreNotSharedWithOtherProviders()
        {
            var factory = CreateFactory(new CapturingHandler(() => JsonResponse(EmbeddingJson)), new MutableApiKeyProvider(null));

            Assert.Equal("OpenAiCompatible", factory.ProviderName);
            Assert.Equal("OpenAiCompatible:embedding-model", factory.EmbeddingModelIdentity);
        }

        [Fact]
        public void CreateChatClient_Throws_WhenTheProviderIsNotConfigured()
        {
            var factory = CreateFactory(
                new CapturingHandler(() => JsonResponse(ChatCompletionJson)),
                new MutableApiKeyProvider(null),
                chatModel: "");

            Assert.Throws<InvalidOperationException>(() => factory.CreateChatClient());
        }

        [Fact]
        public void ChatModelIdentity_IncludesTheProviderName()
        {
            var factory = CreateFactory(new CapturingHandler(() => JsonResponse(ChatCompletionJson)), new MutableApiKeyProvider(null));

            Assert.Equal("OpenAiCompatible:chat-model", factory.ChatModelIdentity);
        }
    }
}
