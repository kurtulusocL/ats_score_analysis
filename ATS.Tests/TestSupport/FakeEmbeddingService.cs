using ATS.Application.Abstract.AI;

namespace ATS.Tests.TestSupport
{
    public sealed class FakeEmbeddingService(Func<IReadOnlyList<string>, IReadOnlyList<float[]>> respond) : IEmbeddingService
    {
        public List<IReadOnlyList<string>> ReceivedTextLists { get; } = new();

        public Task<IReadOnlyList<float[]>> GetEmbeddingsAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default)
        {
            ReceivedTextLists.Add(texts);
            return Task.FromResult(respond(texts));
        }
    }
}
