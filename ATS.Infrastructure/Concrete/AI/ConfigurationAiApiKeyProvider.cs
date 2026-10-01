using ATS.Application.Abstract.AI;
using Microsoft.Extensions.Configuration;

namespace ATS.Infrastructure.Concrete.AI
{
    public class ConfigurationAiApiKeyProvider : IAiApiKeyProvider
    {
        public const string ConfigurationKey = "Ai:OpenAiCompatible:ApiKey";

        private readonly IConfiguration _configuration;

        public ConfigurationAiApiKeyProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string? GetApiKey()
        {
            var apiKey = _configuration[ConfigurationKey];
            return string.IsNullOrWhiteSpace(apiKey) ? null : apiKey.Trim();
        }
    }
}
