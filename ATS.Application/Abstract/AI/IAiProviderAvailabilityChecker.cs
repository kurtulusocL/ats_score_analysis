

namespace ATS.Application.Abstract.AI
{
    public interface IAiProviderAvailabilityChecker
    {
        string ProviderName { get; }

        Task<AiAvailability> CheckAsync(CancellationToken cancellationToken = default);
    }
}
