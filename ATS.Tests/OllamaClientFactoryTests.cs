using ATS.Application.Options;
using ATS.Infrastructure.Concrete.AI;
using Microsoft.Extensions.Options;

namespace ATS.Tests
{
    public class OllamaClientFactoryTests
    {
        private sealed class StubHttpClientFactory : IHttpClientFactory
        {
            public HttpClient CreateClient(string name) => new();
        }

        [Fact]
        public void Identities_IncludeTheProviderNameSoCachesAndResultsAreNotMixedUp()
        {
            var aiOptions = new AiOptions
            {
                Ollama = new OllamaProviderOptions { ChatModel = "chat-model", EmbeddingModel = "embedding-model" }
            };

            var factory = new OllamaClientFactory(new StubHttpClientFactory(), Options.Create(aiOptions));

            Assert.Equal("Ollama", factory.ProviderName);
            Assert.Equal("Ollama:chat-model", factory.ChatModelIdentity);
            Assert.Equal("Ollama:embedding-model", factory.EmbeddingModelIdentity);
        }
    }
}
