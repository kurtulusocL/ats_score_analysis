using ATS.Domain.Entities;
using ATS.Domain.Enums;
using ATS.Infrastructure.Persistence.Context.Mssql;
using Microsoft.EntityFrameworkCore;

namespace ATS.Tests
{
    public class SecurityFindingPersistenceTests
    {
        [Fact]
        public async Task SecurityFindings_AreSavedWithTheCvScan_AndReadBackWithTheirEnumValues()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            int cvScanId;

            await using (var context = new ApplicationDbContext(options))
            {
                var cvScan = new CvScan
                {
                    CandidateName = "Test Candidate",
                    RawText = "cv text",
                    FileType = "pdf",
                    FilePath = "cv.pdf"
                };

                cvScan.SecurityFindings.Add(new SecurityFinding
                {
                    Type = SecurityFindingType.HiddenText,
                    Severity = SecurityFindingSeverity.Low,
                    Description = "Hidden text (white or near-white text) on page 1",
                    Snippet = "python kubernetes"
                });

                cvScan.SecurityFindings.Add(new SecurityFinding
                {
                    Type = SecurityFindingType.InstructionPattern,
                    Severity = SecurityFindingSeverity.High,
                    Description = "Attempt to override earlier instructions",
                    Snippet = "ignore all previous instructions"
                });

                context.CvScans.Add(cvScan);
                await context.SaveChangesAsync();
                cvScanId = cvScan.Id;
            }

            await using (var context = new ApplicationDbContext(options))
            {
                var findings = await context.SecurityFindings
                    .Where(securityFinding => securityFinding.CvScanId == cvScanId)
                    .OrderBy(securityFinding => securityFinding.Id)
                    .ToListAsync();

                Assert.Equal(2, findings.Count);
                Assert.Equal(SecurityFindingType.HiddenText, findings[0].Type);
                Assert.Equal(SecurityFindingSeverity.Low, findings[0].Severity);
                Assert.Equal(SecurityFindingType.InstructionPattern, findings[1].Type);
                Assert.Equal(SecurityFindingSeverity.High, findings[1].Severity);
                Assert.Equal("ignore all previous instructions", findings[1].Snippet);
            }
        }
    }
}
