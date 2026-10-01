
namespace ATS.Application.Abstract.AI
{
    public interface IEmbeddingService
    {
        Task<IReadOnlyList<float[]>> GetEmbeddingsAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default);
    }
}
