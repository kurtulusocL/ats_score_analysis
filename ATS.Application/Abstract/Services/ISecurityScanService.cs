using ATS.Application.Results;

namespace ATS.Application.Abstract.Services
{
    public interface ISecurityScanService
    {
        Task<SecurityScanResult> ScanAsync(string filePath, string fileType, string extractedText, CancellationToken cancellationToken = default);
        Task<SecurityScanResult> ScanJobPostingAsync(string jobPostingText, CancellationToken cancellationToken = default);
    }
}
