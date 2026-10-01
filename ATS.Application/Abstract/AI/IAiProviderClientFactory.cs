using Microsoft.Extensions.AI;

namespace ATS.Application.Abstract.AI
{
    public interface IAiProviderClientFactory
    {
        string ProviderName { get; }
        string ChatModelIdentity { get; }
        string EmbeddingModelIdentity { get; }
        IChatClient CreateChatClient();
        IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator();
    }
}
