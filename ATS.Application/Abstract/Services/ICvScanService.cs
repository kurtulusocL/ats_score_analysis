using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ATS.Application.Results;
using ATS.Domain.Entities;

namespace ATS.Application.Abstract.Services;

public interface ICvScanService
{
	Task<AnalysisResult> AnalyzeAsync(string filePath, string fileType, CancellationToken cancellationToken = default(CancellationToken));

	Task<AnalysisResult> AnalyzeWithJobPostingAsync(string filePath, string fileType, string jobPostingText, string jobTitle, CancellationToken cancellationToken = default(CancellationToken));

	Task<IEnumerable<CvScan>> GetAllScansAsync();

	Task<CvScan?> GetScanByIdAsync(int id);

	Task<CvScan?> GetScanWithDetailsAsync(int id);

	Task DeleteScanAsync(int id);

	Task SaveReportAsync(CvScan cvScan, string reportType);
}
