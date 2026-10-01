using ATS.Application.Abstract.AI;
using ATS.Application.Options;
using ATS.Infrastructure.Concrete.ServiceManagers;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;

namespace ATS.Tests
{
    public class OllamaAvailabilityManagerTests
    {
        private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
        {
            public int CallCount { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                CallCount++;
                return Task.FromResult(respond(request));
            }
        }

        private static HttpResponseMessage TagsResponse(params string[] modelNames)
        {
            var models = string.Join(",", modelNames.Select(n => $"{{\"name\":\"{n}\",\"model\":\"{n}\"}}"));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"{{\"models\":[{models}]}}", Encoding.UTF8, "application/json")
            };
        }

        private static OllamaAvailabilityManager CreateService(StubHandler handler, string chatModel = "qwen3:4b", string embeddingModel = "nomic-embed-text")
        {
            var aiOptions = new AiOptions
            {
                Enabled = true,
                Ollama = new OllamaProviderOptions { ChatModel = chatModel, EmbeddingModel = embeddingModel }
            };

            return new OllamaAvailabilityManager(
                new HttpClient(handler) { BaseAddress = new Uri("http://localhost:11434") },
                Options.Create(aiOptions));
        }

        [Fact]
        public async Task CheckAsync_ReturnsNotConfigured_WithoutCallingOllama_WhenModelsAreBlank()
        {
            var handler = new StubHandler(_ => throw new InvalidOperationException("must not be called"));
            var service = CreateService(handler, chatModel: "", embeddingModel: "");

            var result = await service.CheckAsync();

            Assert.Equal(AiStatus.NotConfigured, result.Status);
            Assert.Equal(0, handler.CallCount);
        }

        [Fact]
        public async Task CheckAsync_ReturnsUnreachable_WhenConnectionFails()
        {
            var handler = new StubHandler(_ => throw new HttpRequestException("connection refused"));
            var service = CreateService(handler);

            var result = await service.CheckAsync();

            Assert.Equal(AiStatus.ProviderUnreachable, result.Status);
            Assert.Contains("Ollama", result.Message);
        }

        [Fact]
        public async Task CheckAsync_ReturnsModelMissing_WhenNoModelInstalled()
        {
            var handler = new StubHandler(_ => TagsResponse());
            var service = CreateService(handler);

            var result = await service.CheckAsync();

            Assert.Equal(AiStatus.ModelMissing, result.Status);
        }

        [Fact]
        public async Task CheckAsync_ReturnsModelMissing_AndNamesOnlyTheMissingModel()
        {
            var handler = new StubHandler(_ => TagsResponse("qwen3:4b"));
            var service = CreateService(handler);

            var result = await service.CheckAsync();

            Assert.Equal(AiStatus.ModelMissing, result.Status);
            Assert.Contains("nomic-embed-text", result.Message);
            Assert.DoesNotContain("qwen3:4b", result.Message);
        }

        [Fact]
        public async Task CheckAsync_ReturnsAvailable_WhenAllModelsInstalled_EvenIfLatestTagOmitted()
        {
            var handler = new StubHandler(_ => TagsResponse("qwen3:4b", "nomic-embed-text:latest"));
            var service = CreateService(handler);

            var result = await service.CheckAsync();

            Assert.Equal(AiStatus.Available, result.Status);
            Assert.True(result.IsAvailable);
            Assert.Equal("Ollama", result.ProviderName);
        }

        [Fact]
        public async Task CheckAsync_PropagatesCancellation_WhenCallerCancels()
        {
            var handler = new StubHandler(_ => TagsResponse("qwen3:4b", "nomic-embed-text"));
            var service = CreateService(handler);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.CheckAsync(cts.Token));
        }
    }
}
