using ATS.Application.Abstract.AI;
using Microsoft.Extensions.AI;

namespace ATS.Tests.TestSupport
{
    public sealed class FakeAiProviderClientFactory(string providerName, string chatModelIdentity) : IAiProviderClientFactory
    {
        public string ProviderName => providerName;
        public string ChatModelIdentity => chatModelIdentity;
        public string EmbeddingModelIdentity => providerName + ":embedding-model";

        public IChatClient CreateChatClient() => throw new NotSupportedException();
        public IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator() => throw new NotSupportedException();
    }
}
