using System.Net;
using System.Text;
using System.Text.Json;
using ATS.Application.Abstract.Services;
using ATS.Infrastructure.Constants.Language;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ATS.Infrastructure.Concrete.ServiceManagers
{
    public class RapidApiTranslator : ITranslatorService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RapidApiTranslator> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public RapidApiTranslator(HttpClient httpClient, IConfiguration configuration, ILogger<RapidApiTranslator> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<string> DetectLanguageAsync(string text)
        {
            try
            {
                var truncatedText = text.Length > 500 ? text.Substring(0, 500) : text;
                var request = CreateRequest("language/translate/v2/detect", new { q = truncatedText });
                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<DetectResponse>(body, _jsonOptions);
                return result?.Data?.Detections?.FirstOrDefault()?.FirstOrDefault()?.Language ?? "en";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dil tespiti sırasında hata oluştu. Varsayılan: 'en'");
                return "en";
            }
        }

        public async Task<string> GetAnalysisTextAsync(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText)) return string.Empty;
            var detectedLang = await DetectLanguageAsync(rawText);
            if (detectedLang == "en") return rawText;

            _logger.LogInformation("CV dili {Lang} olarak tespit edildi. Analiz için çevriliyor...", detectedLang);
            return await TranslateAsync(rawText, "en");
        }

        public async Task<string> TranslateAsync(string text, string targetLanguage = "en")
        {
            try
            {
                var request = CreateRequest("language/translate/v2", new { q = text, target = targetLanguage });

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TranslationResponse>(body, _jsonOptions);
                var translatedText = result?.Data?.Translations?.FirstOrDefault()?.TranslatedText ?? text;
                return WebUtility.HtmlDecode(translatedText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Çeviri sırasında hata oluştu. Orijinal metin dönülüyor.");
                return text;
            }
        }

        private HttpRequestMessage CreateRequest(string endpoint, object payload)
        {
            var host = _configuration["RapidApi:Host"];
            var key = _configuration["RapidApi:Key"];

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://{host}/{endpoint}"),
                Headers =
                {
                    { "x-rapidapi-key", key },
                    { "x-rapidapi-host", host },
                },
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            return request;
        }
    }
}
