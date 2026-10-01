using ATS.Application.Abstract.AI;
using ATS.Application.Options;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OllamaSharp;

namespace ATS.Infrastructure.Concrete.AI
{
    public class OllamaClientFactory : IAiProviderClientFactory
    {
        public const string HttpClientName = "OllamaLlm";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OllamaProviderOptions _ollamaOptions;

        public OllamaClientFactory(IHttpClientFactory httpClientFactory, IOptions<AiOptions> aiOptions)
        {
            _httpClientFactory = httpClientFactory;
            _ollamaOptions = aiOptions.Value.Ollama;
        }

        public string ChatModelIdentity => $"{ProviderName}:{_ollamaOptions.ChatModel}";
        public string ProviderName => AiOptions.OllamaProviderName;       
        public string EmbeddingModelIdentity => $"{ProviderName}:{_ollamaOptions.EmbeddingModel}";

        public IChatClient CreateChatClient() =>
            new OllamaApiClient(_httpClientFactory.CreateClient(HttpClientName), _ollamaOptions.ChatModel);

        public IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator() =>
            new OllamaApiClient(_httpClientFactory.CreateClient(HttpClientName), _ollamaOptions.EmbeddingModel);
    }
}