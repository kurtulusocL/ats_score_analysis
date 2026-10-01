

namespace ATS.Application.Abstract.AI
{
    public interface IAiAvailabilityService
    {
        Task<AiAvailability> CheckAsync(CancellationToken cancellationToken = default);
    }
}
