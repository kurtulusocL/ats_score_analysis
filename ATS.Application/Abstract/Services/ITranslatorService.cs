
namespace ATS.Application.Abstract.Services
{
    public interface ITranslatorService
    {
        Task<string> DetectLanguageAsync(string text);
        Task<string> TranslateAsync(string text, string targetLanguage = "en");
        Task<string> GetAnalysisTextAsync(string rawText);
    }
}
