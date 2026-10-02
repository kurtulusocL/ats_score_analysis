using System.Threading;
using System.Threading.Tasks;

namespace ATS.Application.Abstract.Services;

public interface ITranslatorService
{
	Task<string> DetectLanguageAsync(string text, CancellationToken cancellationToken = default(CancellationToken));

	Task<string> TranslateAsync(string text, string targetLanguage = "en", CancellationToken cancellationToken = default(CancellationToken));

	Task<string> GetAnalysisTextAsync(string rawText, CancellationToken cancellationToken = default(CancellationToken));
}
