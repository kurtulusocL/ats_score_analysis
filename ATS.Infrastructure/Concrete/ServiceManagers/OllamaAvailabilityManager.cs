using ATS.Application.Abstract.AI;
using ATS.Application.Options;
using Microsoft.Extensions.Options;
using OllamaSharp;
using Serilog;

namespace ATS.Infrastructure.Concrete.ServiceManagers
{
    public class OllamaAvailabilityManager : IAiProviderAvailabilityChecker
    {
        private static readonly TimeSpan _probeTimeout = TimeSpan.FromSeconds(5);

        private readonly HttpClient _httpClient;
        private readonly OllamaProviderOptions _ollamaOptions;
        private readonly ILogger _logger = Log.ForContext<OllamaAvailabilityManager>();

        public OllamaAvailabilityManager(HttpClient httpClient, IOptions<AiOptions> aiOptions)
        {
            _httpClient = httpClient;
            _ollamaOptions = aiOptions.Value.Ollama;
        }

        public string ProviderName => AiOptions.OllamaProviderName;

        public async Task<AiAvailability> CheckAsync(CancellationToken cancellationToken = default)
        {
            if (!_ollamaOptions.IsConfigured)
                return AiAvailability.NotConfigured(ProviderName);

            List<string> installed;
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(_probeTimeout);

                var ollama = new OllamaApiClient(_httpClient);
                var models = await ollama.ListLocalModelsAsync(cts.Token);
                installed = models.Select(m => m.Name).ToList();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Ollama is not reachable at {Endpoint}", _ollamaOptions.Endpoint);
                return AiAvailability.Unreachable(ProviderName, _ollamaOptions.Endpoint);
            }

            var required = new[] { _ollamaOptions.ChatModel, _ollamaOptions.EmbeddingModel }
                .Select(Normalize)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var installedNormalized = installed.Select(Normalize).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var missing = required.Where(r => !installedNormalized.Contains(r)).ToList();

            if (missing.Count > 0)
            {
                _logger.Warning("Ollama models missing: {Models}", string.Join(", ", missing));
                return AiAvailability.MissingModel(ProviderName, string.Join(", ", missing));
            }

            return AiAvailability.Ok(ProviderName);
        }

        private static string Normalize(string model)
        {
            var name = model.Trim();
            return name.Contains(':') ? name : name + ":latest";
        }
    }
}
