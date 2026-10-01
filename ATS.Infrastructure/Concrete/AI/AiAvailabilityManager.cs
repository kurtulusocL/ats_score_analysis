using ATS.Application.Abstract.AI;
using ATS.Application.Options;
using Microsoft.Extensions.Options;
using Serilog;

namespace ATS.Infrastructure.Concrete.AI
{
    public class AiAvailabilityManager : IAiAvailabilityService
    {
        private readonly IEnumerable<IAiProviderAvailabilityChecker> _providerAvailabilityCheckers;
        private readonly AiOptions _aiOptions;
        private readonly AiProviderSelection _aiProviderSelection;
        private readonly ILogger _logger = Log.ForContext<AiAvailabilityManager>();

        public AiAvailabilityManager(IEnumerable<IAiProviderAvailabilityChecker> providerAvailabilityCheckers, IOptions<AiOptions> aiOptions, AiProviderSelection aiProviderSelection)
        {
            _providerAvailabilityCheckers = providerAvailabilityCheckers;
            _aiOptions = aiOptions.Value;
            _aiProviderSelection = aiProviderSelection;
        }

        public async Task<AiAvailability> CheckAsync(CancellationToken cancellationToken = default)
        {
            _aiProviderSelection.Clear();

            if (!_aiOptions.Enabled)
                return AiAvailability.Disabled();

            var failures = new List<AiAvailability>();

            foreach (var providerName in _aiOptions.GetEffectiveProviderPriority())
            {
                var providerAvailabilityChecker = _providerAvailabilityCheckers.FirstOrDefault(checker =>
                    string.Equals(checker.ProviderName, providerName, StringComparison.OrdinalIgnoreCase));

                if (providerAvailabilityChecker == null)
                {
                    _logger.Debug("No availability checker is registered for provider {Provider}. Skipping.", providerName);
                    continue;
                }

                AiAvailability availability;
                try
                {
                    availability = await providerAvailabilityChecker.CheckAsync(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    _logger.Warning(exception, "The availability check for provider {Provider} failed.", providerName);
                    availability = AiAvailability.CheckFailed(providerAvailabilityChecker.ProviderName);
                }

                if (availability.IsAvailable)
                {
                    _aiProviderSelection.Select(availability.ProviderName ?? providerName);
                    _logger.Information("AI provider selected: {Provider}", availability.ProviderName ?? providerName);
                    return availability;
                }

                _logger.Information("AI provider {Provider} is not usable: {Status}", providerName, availability.Status);
                failures.Add(availability);
            }

            return AiAvailability.NoProviderAvailable(failures);
        }
    }
}
