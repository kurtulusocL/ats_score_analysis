using ATS.Domain.Entities;

namespace ATS.Application.Abstract.Services
{
    public interface ICvScanService
    {
        Task<CvScan> AnalyzeAsync(string filePath, string fileType);
        Task<CvScan> AnalyzeWithJobPostingAsync(string filePath, string fileType, string jobPostingText, string jobTitle);
        Task<IEnumerable<CvScan>> GetAllScansAsync();
        Task<CvScan?> GetScanByIdAsync(int id);
        Task<CvScan?> GetScanWithDetailsAsync(int id);
        Task DeleteScanAsync(int id);
        Task SaveReportAsync(CvScan cvScan, string reportType);
    }
}
