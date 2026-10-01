using ATS.Application.Reporting;
using ATS.Domain.Entities;
using ATS.Domain.Enums;
using ATS.Infrastructure.Concrete.Repository;
using ATS.Infrastructure.Concrete.ServiceManagers;
using ATS.Infrastructure.Persistence.Context.Mssql;
using Microsoft.EntityFrameworkCore;

namespace ATS.Tests
{
    public class ScanDetailIncludeTests
    {
        [Fact]
        public async Task GetWithIncludePathsAsync_WithTheScanDetailPaths_LoadsEverythingTheReportsReadAfterTheContextIsDisposed()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            int cvScanId;

            await using (var seedContext = new ApplicationDbContext(options))
            {
                var jobPosting = new JobPosting { Title = "Backend Developer", RawText = "posting" };
                var jobRequirement = new JobRequirement { Name = "SQL Server", IsMandatory = true, Category = "Database", JobPosting = jobPosting };
                jobPosting.JobRequirements.Add(jobRequirement);

                var cvScan = new CvScan
                {
                    CandidateName = "Test",
                    RawText = "cv",
                    FileType = "pdf",
                    FilePath = "cv.pdf",
                    IsJobMatched = true,
                    JobPosting = jobPosting
                };

                var requirementMatch = new RequirementMatch
                {
                    Similarity = 0.9,
                    Status = MatchStatus.Met,
                    Evidence = "evidence",
                    CvScan = cvScan,
                    JobRequirement = jobRequirement
                };
                requirementMatch.RequirementInterpretation = new RequirementInterpretation
                {
                    Status = MatchStatus.Partial,
                    EvidenceQuote = "quote",
                    Explanation = "Because.",
                    Suggestion = "Do more.",
                    ModelIdentity = "Ollama:test-model",
                    RequirementMatch = requirementMatch
                };
                cvScan.RequirementMatches.Add(requirementMatch);

                cvScan.SectionScores.Add(new SectionScore { SectionName = "Job Match", Score = 10, MaxScore = 20 });
                cvScan.ScoreReports.Add(new ScoreReport { ReportPath = "report.pdf", ReportType = "jobmatch", IsGenerated = true });
                cvScan.SecurityFindings.Add(new SecurityFinding
                {
                    Type = SecurityFindingType.HiddenText,
                    Severity = SecurityFindingSeverity.Low,
                    Description = "description",
                    Snippet = "snippet"
                });
                cvScan.AnalysisWarnings.Add(new AnalysisWarning { Message = "warning" });
                cvScan.AnalysisAudit = new AnalysisAudit
                {
                    AiStatus = "Available",
                    DeterministicJobMatchScore = 10,
                    SemanticJobMatchScore = 14.0,
                    HybridJobMatchScore = 12
                };

                seedContext.CvScans.Add(cvScan);
                await seedContext.SaveChangesAsync();
                cvScanId = cvScan.Id;
            }

            CvScan? loadedScan;
            await using (var readContext = new ApplicationDbContext(options))
            {
                loadedScan = await new RepositoryBase<CvScan>(readContext)
                    .GetWithIncludePathsAsync(cvScan => cvScan.Id == cvScanId, CvScanManager.ScanDetailIncludePaths.ToArray());
            }

            Assert.NotNull(loadedScan);
            Assert.Equal("Backend Developer", loadedScan!.JobPosting!.Title);
            Assert.Single(loadedScan.SectionScores);
            Assert.Single(loadedScan.ScoreReports);
            Assert.Single(loadedScan.SecurityFindings);
            Assert.Single(loadedScan.AnalysisWarnings);
            Assert.Equal("Available", loadedScan.AnalysisAudit!.AiStatus);

            var report = RequirementReportBuilder.Build(loadedScan);
            var row = Assert.Single(report.Rows);
            Assert.Equal("SQL Server", row.RequirementName);
            Assert.Equal(MatchStatus.Partial, row.ModelStatus);
            Assert.Equal("Because.", row.Explanation);
            Assert.Equal("Keyword score 10/20 | Semantic score 14.0/20 | Hybrid score 12/20", report.ScoreComponentsText);
        }
    }
}
