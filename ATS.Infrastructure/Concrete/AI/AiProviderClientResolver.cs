using ATS.Application.Abstract.AI;

namespace ATS.Infrastructure.Concrete.AI
{
    public class AiProviderClientResolver
    {
        private readonly AiProviderSelection _aiProviderSelection;
        private readonly IEnumerable<IAiProviderClientFactory> _aiProviderClientFactories;

        public AiProviderClientResolver(AiProviderSelection aiProviderSelection, IEnumerable<IAiProviderClientFactory> aiProviderClientFactories)
        {
            _aiProviderSelection = aiProviderSelection;
            _aiProviderClientFactories = aiProviderClientFactories;
        }

        public IAiProviderClientFactory Resolve()
        {
            var selectedProviderName = _aiProviderSelection.SelectedProviderName
                ?? throw new InvalidOperationException("No AI provider has been selected. The availability check must succeed first.");

            return _aiProviderClientFactories.FirstOrDefault(factory =>
                       string.Equals(factory.ProviderName, selectedProviderName, StringComparison.OrdinalIgnoreCase))
                   ?? throw new InvalidOperationException($"No client factory is registered for AI provider '{selectedProviderName}'.");
        }
    }
}
