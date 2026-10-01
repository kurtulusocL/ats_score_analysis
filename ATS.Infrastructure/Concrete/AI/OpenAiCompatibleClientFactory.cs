using ATS.Application.Abstract.AI;
using ATS.Application.Options;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using System.ClientModel;
using System.ClientModel.Primitives;
using OpenAI;

namespace ATS.Infrastructure.Concrete.AI
{
    public class OpenAiCompatibleClientFactory : IAiProviderClientFactory
    {
        public const string HttpClientName = "OpenAiCompatibleLlm";
        private const string PlaceholderApiKey = "not-needed";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OpenAiCompatibleProviderOptions _openAiCompatibleOptions;
        private readonly IAiApiKeyProvider _aiApiKeyProvider;

        public OpenAiCompatibleClientFactory(IHttpClientFactory httpClientFactory, IOptions<AiOptions> aiOptions, IAiApiKeyProvider aiApiKeyProvider)
        {
            _httpClientFactory = httpClientFactory;
            _openAiCompatibleOptions = aiOptions.Value.OpenAiCompatible;
            _aiApiKeyProvider = aiApiKeyProvider;
        }

        public string ProviderName => AiOptions.OpenAiCompatibleProviderName;
        public string ChatModelIdentity => $"{ProviderName}:{_openAiCompatibleOptions.ChatModel}";
        public string EmbeddingModelIdentity => $"{ProviderName}:{_openAiCompatibleOptions.EmbeddingModel}";

        public IChatClient CreateChatClient() =>
            CreateOpenAiClient().GetChatClient(_openAiCompatibleOptions.ChatModel).AsIChatClient();

        public IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator() =>
            CreateOpenAiClient().GetEmbeddingClient(_openAiCompatibleOptions.EmbeddingModel).AsIEmbeddingGenerator();

        private OpenAIClient CreateOpenAiClient()
        {
            if (!_openAiCompatibleOptions.IsConfigured)
                throw new InvalidOperationException("The OpenAI compatible provider is not configured.");

            var clientOptions = new OpenAIClientOptions
            {
                Endpoint = new Uri(_openAiCompatibleOptions.Endpoint.Trim().TrimEnd('/')),
                Transport = new HttpClientPipelineTransport(_httpClientFactory.CreateClient(HttpClientName))
            };

            var apiKey = _aiApiKeyProvider.GetApiKey() ?? PlaceholderApiKey;

            return new OpenAIClient(new ApiKeyCredential(apiKey), clientOptions);
        }
    }
}
