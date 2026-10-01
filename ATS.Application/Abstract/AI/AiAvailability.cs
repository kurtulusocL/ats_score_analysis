

namespace ATS.Application.Abstract.AI
{
    public sealed record AiAvailability(AiStatus Status, string Message, string? ProviderName = null)
    {
        private const string DeterministicFallbackNote = "Running deterministic analysis only.";

        public bool IsAvailable => Status == AiStatus.Available;

        public static AiAvailability Ok(string? providerName = null) =>
            new(AiStatus.Available, providerName == null ? "AI layer is available." : $"AI layer is available (provider: {providerName}).", providerName);

        public static AiAvailability Disabled() => new(AiStatus.Disabled, $"AI layer is disabled in configuration. {DeterministicFallbackNote}");

        public static AiAvailability NotConfigured(string providerName) => new(AiStatus.NotConfigured, $"{providerName} is not configured.", providerName);

        public static AiAvailability Unreachable(string providerName, string endpoint) => new(AiStatus.ProviderUnreachable, $"{providerName} is not reachable at {endpoint}.", providerName);

        public static AiAvailability MissingModel(string providerName, string models) => new(AiStatus.ModelMissing, $"Required model(s) not available in {providerName}: {models}.", providerName);

        public static AiAvailability InvalidCredentials(string providerName, bool apiKeyWasSent) =>
            new(AiStatus.InvalidCredentials,
                apiKeyWasSent
                    ? $"{providerName} rejected the configured API key."
                    : $"{providerName} requires an API key, but none is configured.", providerName);

        public static AiAvailability CheckFailed(string providerName) => new(AiStatus.ProviderUnreachable, $"The availability check for {providerName} failed.", providerName);

        public static AiAvailability NoProviderAvailable(IReadOnlyList<AiAvailability> failures)
        {
            var meaningfulFailures = failures.Where(failure => failure.Status != AiStatus.NotConfigured).ToList();

            if (meaningfulFailures.Count == 0)
                return new AiAvailability(AiStatus.NotConfigured, $"AI is enabled but no provider is configured. {DeterministicFallbackNote}");

            var problems = string.Join(" ", meaningfulFailures.Select(failure => failure.Message));
            return new AiAvailability(meaningfulFailures[0].Status, $"{problems} {DeterministicFallbackNote}");
        }
    }
}
