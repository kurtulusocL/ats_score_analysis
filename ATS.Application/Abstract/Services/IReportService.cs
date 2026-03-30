using ATS.Domain.Entities;

namespace ATS.Application.Abstract.Services
{
    public interface IReportService
    {
        string GenerateGeneralReport(CvScan cvScan);
        string GenerateJobMatchReport(CvScan cvScan);
    }
}
