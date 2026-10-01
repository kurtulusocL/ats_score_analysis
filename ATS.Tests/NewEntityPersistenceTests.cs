using ATS.Domain.Entities;
using ATS.Domain.Enums;
using ATS.Infrastructure.Persistence.Context.Mssql;
using Microsoft.EntityFrameworkCore;

namespace ATS.Tests
{
    public class NewEntityPersistenceTests
    {
        private static DbContextOptions<ApplicationDbContext> CreateOptions() =>
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        private static CvScan CreateCvScan() => new()
        {
            CandidateName = "Test Candidate",
            RawText = "cv text",
            FileType = "pdf",
            FilePath = "cv.pdf"
        };

        [Fact]
        public async Task AnalysisAudit_IsSavedWithTheCvScan_IncludingNullableMetrics()
        {
            var options = CreateOptions();
            int cvScanId;

            await using (var context = new ApplicationDbContext(options))
            {
                var cvScan = CreateCvScan();
                cvScan.AnalysisAudit = new AnalysisAudit
                {
                    AiStatus = "Available",
                    AiProviderName = "Ollama",
                    DeterministicJobMatchScore = 11,
                    SemanticJobMatchScore = 14.5,
                    HybridJobMatchScore = 13,
                    DurationMilliseconds = 2400
                };

                context.CvScans.Add(cvScan);
                await context.SaveChangesAsync();
                cvScanId = cvScan.Id;
            }

            await using (var context = new ApplicationDbContext(options))
            {
                var analysisAudit = await context.AnalysisAudits.SingleAsync(audit => audit.CvScanId == cvScanId);

                Assert.Equal("Available", analysisAudit.AiStatus);
                Assert.Equal("Ollama", analysisAudit.AiProviderName);
                Assert.Equal(11, analysisAudit.DeterministicJobMatchScore);
                Assert.Equal(14.5, analysisAudit.SemanticJobMatchScore);
                Assert.Equal(13, analysisAudit.HybridJobMatchScore);
                Assert.Equal(2400, analysisAudit.DurationMilliseconds);
                Assert.Null(analysisAudit.InputTokenCount);
                Assert.Null(analysisAudit.EmbeddingCacheHitCount);
            }
        }

        [Fact]
        public async Task AnalysisWarnings_AreSavedWithTheCvScan()
        {
            var options = CreateOptions();
            int cvScanId;

            await using (var context = new ApplicationDbContext(options))
            {
                var cvScan = CreateCvScan();
                cvScan.AnalysisWarnings.Add(new AnalysisWarning { Message = "First warning" });
                cvScan.AnalysisWarnings.Add(new AnalysisWarning { Message = "Second warning" });

                context.CvScans.Add(cvScan);
                await context.SaveChangesAsync();
                cvScanId = cvScan.Id;
            }

            await using (var context = new ApplicationDbContext(options))
            {
                var messages = await context.AnalysisWarnings
                    .Where(analysisWarning => analysisWarning.CvScanId == cvScanId)
                    .OrderBy(analysisWarning => analysisWarning.Id)
                    .Select(analysisWarning => analysisWarning.Message)
                    .ToListAsync();

                Assert.Equal(new[] { "First warning", "Second warning" }, messages);
            }
        }

        [Fact]
        public async Task RequirementInterpretation_IsLinkedToItsRequirementMatch()
        {
            var options = CreateOptions();
            int requirementMatchId;

            await using (var context = new ApplicationDbContext(options))
            {
                var jobPosting = new JobPosting { Title = "Backend Developer", RawText = "posting text" };
                var jobRequirement = new JobRequirement
                {
                    Name = "SQL Server",
                    IsMandatory = true,
                    Category = "Database",
                    JobPosting = jobPosting
                };
                jobPosting.JobRequirements.Add(jobRequirement);

                var cvScan = CreateCvScan();
                cvScan.JobPosting = jobPosting;

                var requirementMatch = new RequirementMatch
                {
                    Similarity = 0.9,
                    Status = MatchStatus.Met,
                    Evidence = "Developed APIs with SQL Server",
                    CvScan = cvScan,
                    JobRequirement = jobRequirement
                };
                requirementMatch.RequirementInterpretation = new RequirementInterpretation
                {
                    Status = MatchStatus.Partial,
                    EvidenceQuote = "Developed APIs with SQL Server",
                    Explanation = "Only basic usage is shown.",
                    Suggestion = "Describe query optimization work.",
                    ModelIdentity = "Ollama:test-model",
                    RequirementMatch = requirementMatch
                };
                cvScan.RequirementMatches.Add(requirementMatch);

                context.CvScans.Add(cvScan);
                await context.SaveChangesAsync();
                requirementMatchId = requirementMatch.Id;
            }

            await using (var context = new ApplicationDbContext(options))
            {
                var requirementInterpretation = await context.RequirementInterpretations.SingleAsync();

                Assert.Equal(requirementMatchId, requirementInterpretation.RequirementMatchId);
                Assert.Equal(MatchStatus.Partial, requirementInterpretation.Status);
                Assert.Equal("Only basic usage is shown.", requirementInterpretation.Explanation);
                Assert.Equal("Ollama:test-model", requirementInterpretation.ModelIdentity);
            }
        }

        [Fact]
        public async Task SkillTaxonomyEntry_IsSavedWithItsAliases()
        {
            var options = CreateOptions();
            int entryId;

            await using (var context = new ApplicationDbContext(options))
            {
                var entry = new SkillTaxonomyEntry
                {
                    Name = "SQL Server",
                    Category = "Database",
                    Description = "Microsoft relational database."
                };
                entry.SkillTaxonomyAliases.Add(new SkillTaxonomyAlias { Alias = "MSSQL" });
                entry.SkillTaxonomyAliases.Add(new SkillTaxonomyAlias { Alias = "T-SQL" });

                context.SkillTaxonomyEntries.Add(entry);
                await context.SaveChangesAsync();
                entryId = entry.Id;
            }

            await using (var context = new ApplicationDbContext(options))
            {
                var aliases = await context.SkillTaxonomyAliases
                    .Where(skillTaxonomyAlias => skillTaxonomyAlias.SkillTaxonomyEntryId == entryId)
                    .OrderBy(skillTaxonomyAlias => skillTaxonomyAlias.Alias)
                    .Select(skillTaxonomyAlias => skillTaxonomyAlias.Alias)
                    .ToListAsync();

                Assert.Equal(new[] { "MSSQL", "T-SQL" }, aliases);

                var entry = await context.SkillTaxonomyEntries.SingleAsync(skillTaxonomyEntry => skillTaxonomyEntry.Id == entryId);
                Assert.True(entry.IsActive);
            }
        }
    }
}
