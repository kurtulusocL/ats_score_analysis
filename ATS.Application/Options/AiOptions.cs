
namespace ATS.Application.Options
{
    public class AiOptions
    {
        public const string SectionName = "Ai";
        public const string OllamaProviderName = "Ollama";
        public const string OpenAiCompatibleProviderName = "OpenAiCompatible";

        public bool Enabled { get; set; } = false;
        public List<string> ProviderPriority { get; set; } = new();
        public int TimeoutSeconds { get; set; } = 120;

        public OllamaProviderOptions Ollama { get; set; } = new();
        public OpenAiCompatibleProviderOptions OpenAiCompatible { get; set; } = new();

        public IReadOnlyList<string> GetEffectiveProviderPriority()
        {
            var configuredProviderNames = ProviderPriority
                .Where(providerName => !string.IsNullOrWhiteSpace(providerName))
                .Select(providerName => providerName.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (configuredProviderNames.Count > 0)
                return configuredProviderNames;

            return new[] { OllamaProviderName, OpenAiCompatibleProviderName };
        }
    }
}
