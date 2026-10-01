using ATS.Application.Abstract.Services;

namespace ATS.Tests.TestSupport
{
    public sealed class RecordingTranslatorService : ITranslatorService
    {
        private readonly List<string> _analysisTexts = new();

        public IReadOnlyList<string> AnalysisTexts => _analysisTexts;

        public Task<string> DetectLanguageAsync(string text, CancellationToken cancellationToken = default) => Task.FromResult("en");

        public Task<string> TranslateAsync(string text, string targetLanguage = "en", CancellationToken cancellationToken = default) => Task.FromResult(text);

        public Task<string> GetAnalysisTextAsync(string rawText, CancellationToken cancellationToken = default)
        {
            _analysisTexts.Add(rawText);
            return Task.FromResult(rawText);
        }
    }
}
