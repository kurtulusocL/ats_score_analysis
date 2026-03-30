using ATS.Application.Abstract.Services;
using ATS.Domain.Entities;
using ATS.Infrastructure.Concrete.Reports;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Serilog;

namespace ATS.Infrastructure.Concrete.ServiceManagers
{
    public class ReportManager: IReportService
    {
        private readonly ILogger _logger = Log.ForContext<ReportManager>();

        public ReportManager()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public string GenerateGeneralReport(CvScan cvScan)
        {
            _logger.Information("Generating general report. CvScanId: {Id}", cvScan.Id);

            try
            {
                var outputPath = GetOutputPath(cvScan.Id, "general");
                var template = new CvReportTemplate(cvScan, "general");
                template.GeneratePdf(outputPath);

                _logger.Information("General report generated. Path: {Path}", outputPath);
                return outputPath;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to generate general report. CvScanId: {Id}", cvScan.Id);
                throw;
            }
        }

        public string GenerateJobMatchReport(CvScan cvScan)
        {
            _logger.Information("Generating job match report. CvScanId: {Id}", cvScan.Id);

            try
            {
                var outputPath = GetOutputPath(cvScan.Id, "jobmatch");
                var template = new CvReportTemplate(cvScan, "jobmatch");
                template.GeneratePdf(outputPath);

                _logger.Information("Job match report generated. Path: {Path}", outputPath);
                return outputPath;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to generate job match report. CvScanId: {Id}", cvScan.Id);
                throw;
            }
        }

        private string GetOutputPath(int cvScanId, string reportType)
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var directory = Path.Combine(desktopPath, "AtsReport");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            string fileName = $"report_{reportType}_{cvScanId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return Path.Combine(directory, fileName);
        }
    }
}
