using ATS.Application.Abstract.AI;
using ATS.Application.Options;
using ATS.Infrastructure.Concrete.ServiceManagers;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;

namespace ATS.Tests
{
    public class OpenAiCompatibleAvailabilityManagerTests
    {
        private sealed class StubHandler(Func<HttpResponseMessage> respond) : HttpMessageHandler
        {
            public int CallCount { get; private set; }
            public string? RequestedUri { get; private set; }
            public string? AuthorizationHeader { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                CallCount++;
                RequestedUri = request.RequestUri?.ToString();
                AuthorizationHeader = request.Headers.Authorization?.ToString();
                return Task.FromResult(respond());
            }
        }

        private sealed class FakeApiKeyProvider(string? apiKey) : IAiApiKeyProvider
        {
            public string? GetApiKey() => apiKey;
        }

        private static HttpResponseMessage ModelsResponse(params string[] modelIds)
        {
            var models = string.Join(",", modelIds.Select(modelId => $"{{\"id\":\"{modelId}\",\"object\":\"model\"}}"));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"{{\"object\":\"list\",\"data\":[{models}]}}", Encoding.UTF8, "application/json")
            };
        }

        private static HttpResponseMessage StatusResponse(HttpStatusCode statusCode) => new(statusCode);

        private static OpenAiCompatibleAvailabilityManager CreateManager(
            StubHandler handler,
            string? apiKey = "secret",
            string endpoint = "https://example.test/v1",
            string chatModel = "chat-model",
            string embeddingModel = "embedding-model",
            bool verifyModelsWithProvider = true)
        {
            var aiOptions = new AiOptions
            {
                Enabled = true,
                OpenAiCompatible = new OpenAiCompatibleProviderOptions
                {
                    Endpoint = endpoint,
                    ChatModel = chatModel,
                    EmbeddingModel = embeddingModel,
                    VerifyModelsWithProvider = verifyModelsWithProvider
                }
            };

            return new OpenAiCompatibleAvailabilityManager(new HttpClient(handler), Options.Create(aiOptions), new FakeApiKeyProvider(apiKey));
        }

        [Fact]
        public async Task CheckAsync_ReturnsNotConfigured_WithoutCallingTheServer_WhenModelsAreBlank()
        {
            var handler = new StubHandler(() => throw new InvalidOperationException("must not be called"));

            var result = await CreateManager(handler, chatModel: "", embeddingModel: "").CheckAsync();

            Assert.Equal(AiStatus.NotConfigured, result.Status);
            Assert.Equal(0, handler.CallCount);
        }

        [Theory]
        [InlineData("https://example.test/v1")]
        [InlineData("https://example.test/v1/")]
        public async Task CheckAsync_ReturnsAvailable_AndCallsTheModelListWithTheBearerKey(string endpoint)
        {
            var handler = new StubHandler(() => ModelsResponse("chat-model", "embedding-model"));

            var result = await CreateManager(handler, apiKey: "secret", endpoint: endpoint).CheckAsync();

            Assert.Equal(AiStatus.Available, result.Status);
            Assert.Equal("OpenAiCompatible", result.ProviderName);
            Assert.Equal("https://example.test/v1/models", handler.RequestedUri);
            Assert.Equal("Bearer secret", handler.AuthorizationHeader);
        }

        [Fact]
        public async Task CheckAsync_SendsNoAuthorizationHeader_WhenNoApiKeyIsConfigured()
        {
            var handler = new StubHandler(() => ModelsResponse("chat-model", "embedding-model"));

            var result = await CreateManager(handler, apiKey: null).CheckAsync();

            Assert.Equal(AiStatus.Available, result.Status);
            Assert.Null(handler.AuthorizationHeader);
        }

        [Fact]
        public async Task CheckAsync_ReturnsInvalidCredentials_AndMentionsMissingKey_WhenServerRequiresKeyButNoneIsConfigured()
        {
            var handler = new StubHandler(() => StatusResponse(HttpStatusCode.Unauthorized));

            var result = await CreateManager(handler, apiKey: null).CheckAsync();

            Assert.Equal(AiStatus.InvalidCredentials, result.Status);
            Assert.Contains("none is configured", result.Message);
        }

        [Fact]
        public async Task CheckAsync_ReturnsInvalidCredentials_AndMentionsRejectedKey_WhenServerRejectsTheKey()
        {
            var handler = new StubHandler(() => StatusResponse(HttpStatusCode.Unauthorized));

            var result = await CreateManager(handler, apiKey: "wrong").CheckAsync();

            Assert.Equal(AiStatus.InvalidCredentials, result.Status);
            Assert.Contains("rejected", result.Message);
        }

        [Fact]
        public async Task CheckAsync_TreatsForbiddenLikeUnauthorized()
        {
            var handler = new StubHandler(() => StatusResponse(HttpStatusCode.Forbidden));

            var result = await CreateManager(handler).CheckAsync();

            Assert.Equal(AiStatus.InvalidCredentials, result.Status);
        }

        [Fact]
        public async Task CheckAsync_ReturnsModelMissing_AndNamesOnlyTheMissingModel()
        {
            var handler = new StubHandler(() => ModelsResponse("chat-model"));

            var result = await CreateManager(handler).CheckAsync();

            Assert.Equal(AiStatus.ModelMissing, result.Status);
            Assert.Contains("embedding-model", result.Message);
            Assert.DoesNotContain("chat-model", result.Message);
        }

        [Fact]
        public async Task CheckAsync_IgnoresTheModelsPrefixInListedModelIds()
        {
            var handler = new StubHandler(() => ModelsResponse("models/chat-model", "models/embedding-model"));

            var result = await CreateManager(handler).CheckAsync();

            Assert.Equal(AiStatus.Available, result.Status);
        }

        [Fact]
        public async Task CheckAsync_ReturnsUnreachable_WhenConnectionFails()
        {
            var handler = new StubHandler(() => throw new HttpRequestException("connection refused"));

            var result = await CreateManager(handler).CheckAsync();

            Assert.Equal(AiStatus.ProviderUnreachable, result.Status);
            Assert.Contains("OpenAiCompatible", result.Message);
        }

        [Fact]
        public async Task CheckAsync_ReturnsUnreachable_WhenServerAnswersWithAnErrorStatus()
        {
            var handler = new StubHandler(() => StatusResponse(HttpStatusCode.InternalServerError));

            var result = await CreateManager(handler).CheckAsync();

            Assert.Equal(AiStatus.ProviderUnreachable, result.Status);
        }

        [Fact]
        public async Task CheckAsync_DoesNotCompareModels_WhenVerificationIsSwitchedOff()
        {
            var handler = new StubHandler(() => ModelsResponse("some-other-model"));

            var result = await CreateManager(handler, verifyModelsWithProvider: false).CheckAsync();

            Assert.Equal(AiStatus.Available, result.Status);
        }

        [Fact]
        public async Task CheckAsync_TreatsANotFoundModelListAsAvailable_WhenVerificationIsSwitchedOff()
        {
            var handler = new StubHandler(() => StatusResponse(HttpStatusCode.NotFound));

            var result = await CreateManager(handler, verifyModelsWithProvider: false).CheckAsync();

            Assert.Equal(AiStatus.Available, result.Status);
        }

        [Fact]
        public async Task CheckAsync_StillReportsInvalidCredentials_WhenVerificationIsSwitchedOff()
        {
            var handler = new StubHandler(() => StatusResponse(HttpStatusCode.Unauthorized));

            var result = await CreateManager(handler, verifyModelsWithProvider: false).CheckAsync();

            Assert.Equal(AiStatus.InvalidCredentials, result.Status);
        }

        [Fact]
        public async Task CheckAsync_ReturnsAFailedCheck_WhenTheModelListIsNotValidJson()
        {
            var handler = new StubHandler(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("<html>not json</html>", Encoding.UTF8, "text/html")
            });

            var result = await CreateManager(handler).CheckAsync();

            Assert.Equal(AiStatus.ProviderUnreachable, result.Status);
            Assert.Contains("availability check", result.Message);
        }

        [Fact]
        public async Task CheckAsync_PropagatesCancellation_WhenCallerCancels()
        {
            var handler = new StubHandler(() => ModelsResponse("chat-model", "embedding-model"));
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                CreateManager(handler).CheckAsync(cancellationTokenSource.Token));
        }
    }
}
