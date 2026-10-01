using ATS.Application.Abstract.AI;
using ATS.Application.Options;
using Microsoft.Extensions.Options;
using Serilog;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ATS.Infrastructure.Concrete.ServiceManagers
{
    public class OpenAiCompatibleAvailabilityManager : IAiProviderAvailabilityChecker
    {
        private const string ModelIdPrefixToIgnore = "models/";
        private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(5);
        private readonly HttpClient _httpClient;
        private readonly OpenAiCompatibleProviderOptions _openAiCompatibleOptions;
        private readonly IAiApiKeyProvider _aiApiKeyProvider;
        private readonly ILogger _logger = Log.ForContext<OpenAiCompatibleAvailabilityManager>();

        public OpenAiCompatibleAvailabilityManager(HttpClient httpClient, IOptions<AiOptions> aiOptions, IAiApiKeyProvider aiApiKeyProvider)
        {
            _httpClient = httpClient;
            _openAiCompatibleOptions = aiOptions.Value.OpenAiCompatible;
            _aiApiKeyProvider = aiApiKeyProvider;
        }

        public string ProviderName => AiOptions.OpenAiCompatibleProviderName;

        public async Task<AiAvailability> CheckAsync(CancellationToken cancellationToken = default)
        {
            var baseUri = _openAiCompatibleOptions.GetBaseUri();
            if (baseUri == null || !_openAiCompatibleOptions.IsConfigured)
                return AiAvailability.NotConfigured(ProviderName);

            var apiKey = _aiApiKeyProvider.GetApiKey();
            var endpointText = baseUri.ToString();
            string responseBody;

            try
            {
                using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutSource.CancelAfter(ProbeTimeout);

                using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, "models"));
                if (apiKey != null)
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                using var response = await _httpClient.SendAsync(request, timeoutSource.Token);

                if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    _logger.Warning("{Provider} rejected the request. Status: {Status}, ApiKeySent: {ApiKeySent}",
                        ProviderName, (int)response.StatusCode, apiKey != null);
                    return AiAvailability.InvalidCredentials(ProviderName, apiKey != null);
                }

                if (!_openAiCompatibleOptions.VerifyModelsWithProvider)
                {
                    if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
                        return AiAvailability.Ok(ProviderName);

                    _logger.Warning("{Provider} answered with status {Status}.", ProviderName, (int)response.StatusCode);
                    return AiAvailability.Unreachable(ProviderName, endpointText);
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Warning("{Provider} answered the model list request with status {Status}.", ProviderName, (int)response.StatusCode);
                    return AiAvailability.Unreachable(ProviderName, endpointText);
                }

                responseBody = await response.Content.ReadAsStringAsync(timeoutSource.Token);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.Warning(exception, "{Provider} is not reachable at {Endpoint}.", ProviderName, endpointText);
                return AiAvailability.Unreachable(ProviderName, endpointText);
            }

            HashSet<string> availableModelIds;
            try
            {
                availableModelIds = ParseModelIds(responseBody);
            }
            catch (Exception exception)
            {
                _logger.Warning(exception, "The model list of {Provider} could not be read.", ProviderName);
                return AiAvailability.CheckFailed(ProviderName);
            }

            var missingModelIds = new[] { _openAiCompatibleOptions.ChatModel, _openAiCompatibleOptions.EmbeddingModel }
                .Select(NormalizeModelId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(requiredModelId => !availableModelIds.Contains(requiredModelId))
                .ToList();

            if (missingModelIds.Count > 0)
            {
                _logger.Warning("{Provider} models missing: {Models}", ProviderName, string.Join(", ", missingModelIds));
                return AiAvailability.MissingModel(ProviderName, string.Join(", ", missingModelIds));
            }

            return AiAvailability.Ok(ProviderName);
        }

        private static HashSet<string> ParseModelIds(string responseBody)
        {
            using var document = JsonDocument.Parse(responseBody);

            if (document.RootElement.ValueKind != JsonValueKind.Object
                || !document.RootElement.TryGetProperty("data", out var dataElement)
                || dataElement.ValueKind != JsonValueKind.Array)
            {
                throw new FormatException("The model list response does not contain a 'data' array.");
            }

            var modelIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var modelElement in dataElement.EnumerateArray())
            {
                if (modelElement.ValueKind == JsonValueKind.Object
                    && modelElement.TryGetProperty("id", out var idElement)
                    && idElement.ValueKind == JsonValueKind.String)
                {
                    modelIds.Add(NormalizeModelId(idElement.GetString()!));
                }
            }

            return modelIds;
        }

        private static string NormalizeModelId(string modelId)
        {
            var normalizedModelId = modelId.Trim();

            return normalizedModelId.StartsWith(ModelIdPrefixToIgnore, StringComparison.OrdinalIgnoreCase)
                ? normalizedModelId[ModelIdPrefixToIgnore.Length..]
                : normalizedModelId;
        }
    }
}
