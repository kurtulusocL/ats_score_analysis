using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ATS.Application.Abstract.Services;
using ATS.Core.Extensions;
using ATS.Core.Helpers;
using ATS.Infrastructure.Constants.Language;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ATS.Infrastructure.Concrete.ServiceManagers;

public class RapidApiTranslator : ITranslatorService
{
	private const int DefaultChunkChars = 5000;

	private const int MinChunkChars = 500;

	private readonly HttpClient _httpClient;

	private readonly IConfiguration _configuration;

	private readonly ILogger<RapidApiTranslator> _logger;

	private readonly JsonSerializerOptions _jsonOptions;

	private int MaxChunkChars
	{
		get
		{
			int result;
			return int.TryParse(_configuration["RapidApi:MaxChunkChars"], out result) ? Math.Max(result, 500) : 5000;
		}
	}

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

	public async Task<string> GetAnalysisTextAsync(string rawText, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (string.IsNullOrWhiteSpace(rawText))
		{
			return string.Empty;
		}
		EnglishTextAnalysis localCheck = EnglishTextDetector.Analyze(rawText);
		_logger.LogInformation("Local language check. Words: {WordCount}, EnglishRatio: {EnglishRatio}, TurkishWordRatio: {TurkishWordRatio}, TurkishLetterRatio: {TurkishLetterRatio}, LikelyEnglish: {LikelyEnglish}", localCheck.WordCount, Math.Round(localCheck.EnglishFunctionWordRatio, 3), Math.Round(localCheck.TurkishFunctionWordRatio, 3), Math.Round(localCheck.TurkishLetterRatio, 3), localCheck.IsLikelyEnglish);
		if (localCheck.IsLikelyEnglish)
		{
			return rawText;
		}
		try
		{
			List<string> chunks = SplitIntoChunks(rawText, MaxChunkChars);
			(string Text, string? DetectedLanguage) first = await TranslateChunkAsync(chunks[0], null, "en", cancellationToken);
			string detected = first.DetectedLanguage;
			_logger.LogInformation("Detected language: {Lang}. Chunks: {Count}", detected ?? "unknown", chunks.Count);
			if (string.Equals(detected, "en", StringComparison.OrdinalIgnoreCase))
			{
				return rawText;
			}
			List<string> parts = new List<string> { first.Text };
			foreach (string chunk in chunks.Skip(1))
			{
				cancellationToken.ThrowIfCancellationRequested();
				parts.Add((await TranslateChunkAsync(chunk, detected, "en", cancellationToken)).Item1);
			}
			string translated = string.Join("\n", parts);
			_logger.LogInformation("Translation completed. Chars: {Before} -> {After}", rawText.Length, translated.Length);
			return translated;
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception ex2)
		{
			_logger.LogError(ex2, "Translation failed.");
			throw new TranslationUnavailableException("The translation service failed.", ex2);
		}
	}

	public async Task<string> DetectLanguageAsync(string text, CancellationToken cancellationToken = default(CancellationToken))
	{
		try
		{
			string sample = ((text.Length > 500) ? text.Substring(0, 500) : text);
			return (await TranslateChunkAsync(sample, null, "en", cancellationToken)).Item2 ?? "en";
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception ex2)
		{
			Exception ex3 = ex2;
			_logger.LogError(ex3, "Language detection failed. Default: 'en'");
			return "en";
		}
	}

	public async Task<string> TranslateAsync(string text, string targetLanguage = "en", CancellationToken cancellationToken = default(CancellationToken))
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return string.Empty;
		}
		try
		{
			List<string> parts = new List<string>();
			string source = null;
			foreach (string chunk in SplitIntoChunks(text, MaxChunkChars))
			{
				cancellationToken.ThrowIfCancellationRequested();
				(string Text, string? DetectedLanguage) part = await TranslateChunkAsync(chunk, source, targetLanguage, cancellationToken);
				if (source == null)
				{
					source = part.DetectedLanguage;
				}
				parts.Add(part.Text);
			}
			return string.Join("\n", parts);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Translation failed. Returning the original text.");
			return text;
		}
	}

	private async Task<(string Text, string? DetectedLanguage)> TranslateChunkAsync(string chunk, string? source, string target, CancellationToken cancellationToken)
	{
		Dictionary<string, string> payload = new Dictionary<string, string>
		{
			["format"] = "text",
			["q"] = chunk,
			["target"] = target
		};
		if (!string.IsNullOrWhiteSpace(source))
		{
			payload["source"] = source;
		}
		using HttpRequestMessage request = CreateRequest(payload);
		using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
		response.EnsureSuccessStatusCode();
		Translations first = JsonSerializer.Deserialize<TranslationResponse>(await response.Content.ReadAsStringAsync(cancellationToken), _jsonOptions)?.Data?.Translations?.FirstOrDefault() ?? throw new InvalidOperationException("Translation response contained no translations.");
		return (Text: WebUtility.HtmlDecode(first.TranslatedText), DetectedLanguage: first.DetectedSourceLanguage);
	}

	private static List<string> SplitIntoChunks(string text, int maxChars)
	{
		List<string> list = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		string[] array = text.Split('\n');
		foreach (string text2 in array)
		{
			string text3 = text2.TrimEnd('\r');
			for (int j = 0; j < Math.Max(1, text3.Length); j += maxChars)
			{
				string text4 = ((text3.Length == 0) ? string.Empty : text3.Substring(j, Math.Min(maxChars, text3.Length - j)));
				if (stringBuilder.Length > 0 && stringBuilder.Length + text4.Length + 1 > maxChars)
				{
					list.Add(stringBuilder.ToString());
					stringBuilder.Clear();
				}
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append('\n');
				}
				stringBuilder.Append(text4);
			}
		}
		if (stringBuilder.Length > 0)
		{
			list.Add(stringBuilder.ToString());
		}
		return list;
	}

	private HttpRequestMessage CreateRequest(object payload)
	{
		string text = _configuration["RapidApi:Host"];
		string value = _configuration["RapidApi:Key"];
		string text2 = _configuration["RapidApi:TranslatePath"] ?? "/v2";
		if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(value))
		{
			throw new InvalidOperationException("RapidApi:Host and RapidApi:Key must be configured.");
		}
		return new HttpRequestMessage
		{
			Method = HttpMethod.Post,
			RequestUri = new Uri("https://" + text + text2),
			Headers = 
			{
				{ "x-rapidapi-key", value },
				{ "x-rapidapi-host", text }
			},
			Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
		};
	}
}
