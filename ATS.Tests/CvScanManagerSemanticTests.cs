using ATS.Application.Abstract.AI;
using ATS.Application.Abstract.Services;
using ATS.Application.Options;
using ATS.Application.Results;
using ATS.Application.Semantic;
using ATS.Core.Constants;
using ATS.Domain.Entities;
using ATS.Domain.Enums;
using ATS.Infrastructure.Concrete.Repository;
using ATS.Infrastructure.Concrete.ServiceManagers;
using ATS.Infrastructure.Persistence.Context.Mssql;
using ATS.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;

namespace ATS.Tests
{
    public class CvScanManagerSemanticTests
    {
        private const string CvText =
        "John Smith\njohn@mail.com\n+90 555 123 4567\n\nSUMMARY\nBackend developer.\n\nEXPERIENCE\n" +
        "Developed REST APIs with C# and SQL Server from 2018 to 2024.\n\nSKILLS\nC#, SQL Server, REST\n\nEDUCATION\nBachelor of Computer Engineering";

        private const string JobPostingText = "Experience with SQL Server, C# and Kubernetes.";

        private sealed class FakeCvParserService : ICvParserService
        {
            public Task<string> ParseAsync(string filePath, string fileType, CancellationToken cancellationToken = default) =>
                Task.FromResult(CvText);
        }

        internal sealed class PassThroughTranslatorService : ITranslatorService
        {
            public Task<string> DetectLanguageAsync(string text, CancellationToken cancellationToken = default) => Task.FromResult("en");
            public Task<string> TranslateAsync(string text, string targetLanguage = "en", CancellationToken cancellationToken = default) => Task.FromResult(text);
            public Task<string> GetAnalysisTextAsync(string rawText, CancellationToken cancellationToken = default) => Task.FromResult(rawText);
        }

        internal sealed class FakeReportService : IReportService
        {
            public string GenerateGeneralReport(CvScan cvScan) => "general.pdf";
            public string GenerateJobMatchReport(CvScan cvScan) => "jobmatch.pdf";
        }

        internal sealed class FakeAiAvailabilityService(AiAvailability availability) : IAiAvailabilityService
        {
            public Task<AiAvailability> CheckAsync(CancellationToken cancellationToken = default) => Task.FromResult(availability);
        }

        private sealed class FakeRequirementExtractor(IReadOnlyList<ExtractedRequirement> extractedRequirements) : IRequirementExtractor
        {
            public int CallCount { get; private set; }

            public Task<IReadOnlyList<ExtractedRequirement>> ExtractAsync(string jobPostingText, CancellationToken cancellationToken = default)
            {
                CallCount++;
                return Task.FromResult(extractedRequirements);
            }
        }

        private sealed class FakeSemanticMatcher(Func<IReadOnlyList<ExtractedRequirement>, IReadOnlyList<RequirementMatchResult>> respond) : ISemanticMatcher
        {
            public int CallCount { get; private set; }

            public Task<IReadOnlyList<RequirementMatchResult>> MatchAsync(
                IReadOnlyList<ExtractedRequirement> extractedRequirements, string cvText, CancellationToken cancellationToken = default)
            {
                CallCount++;
                return Task.FromResult(respond(extractedRequirements));
            }
        }

        private static readonly IReadOnlyList<ExtractedRequirement> TwoExtractedRequirements = new[]
        {
        new ExtractedRequirement("SQL Server", true, "Database"),
        new ExtractedRequirement("Kubernetes", false, "Tool")
    };

        private static IReadOnlyList<RequirementMatchResult> MetThenMissing(IReadOnlyList<ExtractedRequirement> extractedRequirements) => new[]
        {
        new RequirementMatchResult(extractedRequirements[0], 0.9, MatchStatus.Met, "Developed REST APIs with C# and SQL Server"),
        new RequirementMatchResult(extractedRequirements[1], 0.1, MatchStatus.Missing, null)
    };

        private static async Task<(AnalysisResult Result, ApplicationDbContext Context)> AnalyzeAsync(AiAvailability availability, Lazy<IRequirementExtractor> requirementExtractor,
    Lazy<ISemanticMatcher> semanticMatcher, Lazy<HybridScoreCalculator>? hybridScoreCalculator = null, ISecurityScanService? securityScanService = null, Lazy<IRequirementInterpreter>? requirementInterpreter = null, AiUsageTracker? aiUsageTracker = null)
        {
            var context = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            var manager = new CvScanManager(
                new RepositoryBase<CvScan>(context),
                new RepositoryBase<SectionScore>(context),
                new RepositoryBase<AnalyzerKeyword>(context),
                new RepositoryBase<ScoreReport>(context),
                new PassThroughTranslatorService(),
                new FakeCvParserService(),
                new FakeReportService(),
                new FakeAiAvailabilityService(availability),
                requirementExtractor,
                semanticMatcher,
                hybridScoreCalculator ?? new Lazy<HybridScoreCalculator>(() => new HybridScoreCalculator(new SemanticOptions())),
                securityScanService ?? new FakeSecurityScanService(),
                requirementInterpreter ?? new Lazy<IRequirementInterpreter>(() => FakeRequirementInterpreter.ReturningNothing()),
                aiUsageTracker ?? new AiUsageTracker());

            var result = await manager.AnalyzeWithJobPostingAsync("cv.pdf", "pdf", JobPostingText, "Backend Developer");
            return (result, context);
        }

        private static SectionScore GetJobMatchSectionScore(ApplicationDbContext context, int cvScanId) =>
            context.SectionScores.Single(sectionScore => sectionScore.CvScanId == cvScanId && sectionScore.SectionName == "Job Match");

        private static async Task<int> GetDeterministicJobMatchScoreAsync()
        {
            var (result, context) = await AnalyzeAsync(
                AiAvailability.Disabled(),
                new Lazy<IRequirementExtractor>(() => new FakeRequirementExtractor(Array.Empty<ExtractedRequirement>())),
                new Lazy<ISemanticMatcher>(() => new FakeSemanticMatcher(_ => Array.Empty<RequirementMatchResult>())));

            return GetJobMatchSectionScore(context, result.Scan.Id).Score;
        }

        [Fact]
        public async Task AnalyzeWithJobPostingAsync_WhenAiIsDisabled_NeverResolvesAiComponents()
        {
            var (result, context) = await AnalyzeAsync(
                AiAvailability.Disabled(),
                new Lazy<IRequirementExtractor>(() => throw new InvalidOperationException("extractor must not be created")),
                new Lazy<ISemanticMatcher>(() => throw new InvalidOperationException("matcher must not be created")),
                new Lazy<HybridScoreCalculator>(() => throw new InvalidOperationException("calculator must not be created")));

            Assert.Empty(result.Warnings);
            Assert.Empty(context.JobRequirements);
            Assert.Empty(context.RequirementMatches);
        }

        [Fact]
        public async Task AnalyzeWithJobPostingAsync_WhenAiIsUnreachable_AddsAvailabilityWarningAndSkipsSemanticLayer()
        {
            var extractor = new FakeRequirementExtractor(TwoExtractedRequirements);
            var matcher = new FakeSemanticMatcher(MetThenMissing);

            var (result, _) = await AnalyzeAsync(
                AiAvailability.Unreachable("Ollama", "http://localhost:11434"),
                new Lazy<IRequirementExtractor>(() => extractor),
                new Lazy<ISemanticMatcher>(() => matcher));

            Assert.Equal(0, extractor.CallCount);
            Assert.Equal(0, matcher.CallCount);
            Assert.Single(result.Warnings);
            Assert.Contains("not reachable", result.Warnings[0]);
        }

        [Fact]
        public async Task AnalyzeWithJobPostingAsync_WhenAiIsAvailable_ReplacesJobMatchScoreWithHybridScoreAndPersistsRequirementMatches()
        {
            var deterministicScore = await GetDeterministicJobMatchScoreAsync();
            var expectedHybridScore = new HybridScoreCalculator(new SemanticOptions())
                .Calculate(deterministicScore, 20, MetThenMissing(TwoExtractedRequirements)).HybridScore;

            var (result, context) = await AnalyzeAsync(
                AiAvailability.Ok(),
                new Lazy<IRequirementExtractor>(() => new FakeRequirementExtractor(TwoExtractedRequirements)),
                new Lazy<ISemanticMatcher>(() => new FakeSemanticMatcher(MetThenMissing)));

            var jobMatchSectionScore = GetJobMatchSectionScore(context, result.Scan.Id);
            Assert.Equal(expectedHybridScore, jobMatchSectionScore.Score);
            Assert.Contains("Hybrid Job Match", jobMatchSectionScore.Feedback);
            Assert.Contains("Kubernetes", jobMatchSectionScore.Feedback);

            var allSectionScores = context.SectionScores.Where(sectionScore => sectionScore.CvScanId == result.Scan.Id).ToList();
            var qualitySectionScores = allSectionScores.Where(sectionScore => sectionScore.SectionName != "Job Match").ToList();
            var qualityRatio = (double)qualitySectionScores.Sum(sectionScore => sectionScore.Score) / qualitySectionScores.Sum(sectionScore => sectionScore.MaxScore);
            var jobMatchRatio = (double)jobMatchSectionScore.Score / jobMatchSectionScore.MaxScore;
            Assert.Equal((int)Math.Round(100 * (0.3 * qualityRatio + 0.7 * jobMatchRatio), MidpointRounding.AwayFromZero), result.Scan.OverallScore);

            Assert.Equal(2, context.JobRequirements.Count());
            var requirementMatches = context.RequirementMatches.Where(requirementMatch => requirementMatch.CvScanId == result.Scan.Id).ToList();
            Assert.Equal(2, requirementMatches.Count);
            Assert.Contains(requirementMatches, requirementMatch => requirementMatch.Status == MatchStatus.Met && requirementMatch.Evidence != null);
            Assert.Contains(requirementMatches, requirementMatch => requirementMatch.Status == MatchStatus.Missing && requirementMatch.Evidence == null);
        }

        [Fact]
        public async Task AnalyzeWithJobPostingAsync_WhenSemanticLayerFails_FallsBackToDeterministicScoreWithWarning()
        {
            var deterministicScore = await GetDeterministicJobMatchScoreAsync();

            var (result, context) = await AnalyzeAsync(
                AiAvailability.Ok(),
                new Lazy<IRequirementExtractor>(() => new FakeRequirementExtractor(TwoExtractedRequirements)),
                new Lazy<ISemanticMatcher>(() => new FakeSemanticMatcher(_ => throw new InvalidOperationException("embedding model crashed"))));

            Assert.Equal(deterministicScore, GetJobMatchSectionScore(context, result.Scan.Id).Score);
            Assert.Contains(result.Warnings, warning => warning.Contains("semantic layer failed"));
            Assert.Empty(context.RequirementMatches);
        }

        [Fact]
        public async Task AnalyzeWithJobPostingAsync_WhenAiComponentCannotBeCreated_FallsBackToDeterministicScoreWithWarning()
        {
            var deterministicScore = await GetDeterministicJobMatchScoreAsync();

            var (result, context) = await AnalyzeAsync(
                AiAvailability.Ok(),
                new Lazy<IRequirementExtractor>(() => new FakeRequirementExtractor(TwoExtractedRequirements)),
                new Lazy<ISemanticMatcher>(() => throw new InvalidOperationException("provider is misconfigured")));

            Assert.Equal(deterministicScore, GetJobMatchSectionScore(context, result.Scan.Id).Score);
            Assert.Contains(result.Warnings, warning => warning.Contains("semantic layer failed"));
        }

        [Fact]
        public async Task AnalyzeWithJobPostingAsync_WhenHybridCalculatorCannotBeCreated_FallsBackToDeterministicScoreAndSavesNoRequirementMatches()
        {
            var deterministicScore = await GetDeterministicJobMatchScoreAsync();

            var (result, context) = await AnalyzeAsync(
                AiAvailability.Ok(),
                new Lazy<IRequirementExtractor>(() => new FakeRequirementExtractor(TwoExtractedRequirements)),
                new Lazy<ISemanticMatcher>(() => new FakeSemanticMatcher(MetThenMissing)),
                new Lazy<HybridScoreCalculator>(() => throw new InvalidOperationException("invalid weights")));

            Assert.Equal(deterministicScore, GetJobMatchSectionScore(context, result.Scan.Id).Score);
            Assert.Contains(result.Warnings, warning => warning.Contains("hybrid score could not be calculated"));
            Assert.Empty(context.RequirementMatches);
        }

        [Fact]
        public async Task AnalyzeWithJobPostingAsync_WhenNoRequirementsAreExtracted_KeepsDeterministicScoreWithWarning()
        {
            var deterministicScore = await GetDeterministicJobMatchScoreAsync();
            var extractor = new FakeRequirementExtractor(Array.Empty<ExtractedRequirement>());
            var matcher = new FakeSemanticMatcher(MetThenMissing);

            var (result, context) = await AnalyzeAsync(
                AiAvailability.Ok(),
                new Lazy<IRequirementExtractor>(() => extractor),
                new Lazy<ISemanticMatcher>(() => matcher));

            Assert.Equal(1, extractor.CallCount);
            Assert.Equal(0, matcher.CallCount);
            Assert.Equal(deterministicScore, GetJobMatchSectionScore(context, result.Scan.Id).Score);
            Assert.Contains(result.Warnings, warning => warning.Contains("No requirements could be extracted"));
        }

        [Fact]
        public async Task AnalyzeWithJobPostingAsync_WhenHybridScoreIsApplied_WritesDeterministicAndHybridScoresToAudit()
        {
            var deterministicScore = await GetDeterministicJobMatchScoreAsync();
            Func<IReadOnlyList<ExtractedRequirement>, IReadOnlyList<RequirementMatchResult>> allMet = extractedRequirements =>
                extractedRequirements
                    .Select(extractedRequirement => new RequirementMatchResult(extractedRequirement, 0.9, MatchStatus.Met, "evidence"))
                    .ToList();
            var expectedHybridResult = new HybridScoreCalculator(new SemanticOptions())
                .Calculate(deterministicScore, 20, allMet(TwoExtractedRequirements));

            var (result, context) = await AnalyzeAsync(
                AiAvailability.Ok("Ollama"),
                new Lazy<IRequirementExtractor>(() => new FakeRequirementExtractor(TwoExtractedRequirements)),
                new Lazy<ISemanticMatcher>(() => new FakeSemanticMatcher(allMet)));

            var audit = context.AnalysisAudits.Single(analysisAudit => analysisAudit.CvScanId == result.Scan.Id);
            Assert.Equal("Available", audit.AiStatus);
            Assert.Equal("Ollama", audit.AiProviderName);
            Assert.Equal(deterministicScore, audit.DeterministicJobMatchScore);
            Assert.Equal(expectedHybridResult.HybridScore, audit.HybridJobMatchScore);
            Assert.Equal(expectedHybridResult.SemanticScore, audit.SemanticJobMatchScore);
            Assert.Equal(expectedHybridResult.HybridScore, GetJobMatchSectionScore(context, result.Scan.Id).Score);
        }

        private static Task<(AnalysisResult Result, ApplicationDbContext Context)> AnalyzeWithInterpreterAsync(IRequirementInterpreter requirementInterpreter,
    Func<IReadOnlyList<ExtractedRequirement>, IReadOnlyList<RequirementMatchResult>> respondToMatching) => AnalyzeAsync(AiAvailability.Ok("Ollama"),
        new Lazy<IRequirementExtractor>(() => new FakeRequirementExtractor(TwoExtractedRequirements)), new Lazy<ISemanticMatcher>(() => new FakeSemanticMatcher(respondToMatching)),
        requirementInterpreter: new Lazy<IRequirementInterpreter>(() => requirementInterpreter));

        private static RequirementInterpretationOutcome OutcomeFor(
            IReadOnlyList<ExtractedRequirement> requirements, string modelIdentity = "Ollama:test-model", int uninterpretedCount = 0, int downgradedCount = 0) =>
            new(requirements.Select((requirement, index) => new RequirementInterpretationResult(
                    requirement.Name,
                    index == 0 ? MatchStatus.Partial : MatchStatus.Missing,
                    index == 0 ? "Developed REST APIs with C# and SQL Server" : null,
                    $"Explanation of {requirement.Name}.",
                    $"Suggestion for {requirement.Name}.")).ToList(),
                uninterpretedCount, downgradedCount, modelIdentity);

        [Fact]
        public async Task AnalyzeWithJobPostingAsync_PersistsTheInterpretationsLinkedToTheirRequirements_WithoutChangingTheMatches()
        {
            var interpreter = new FakeRequirementInterpreter(requirements => OutcomeFor(requirements));

            var (result, context) = await AnalyzeWithInterpreterAsync(interpreter, MetThenMissing);

            var rows = context.RequirementInterpretations
                .Where(interpretation => interpretation.RequirementMatch.CvScanId == result.Scan.Id)
                .Select(interpretation => new
                {
                    Name = interpretation.RequirementMatch.JobRequirement.Name,
                    interpretation.Status,
                    interpretation.EvidenceQuote,
                    interpretation.Explanation,
                    interpretation.Suggestion,
                    interpretation.ModelIdentity
                })
                .OrderBy(row => row.Name)
                .ToList();

            Assert.Equal(2, rows.Count);
            Assert.Equal("Kubernetes", rows[0].Name);
            Assert.Equal(MatchStatus.Missing, rows[0].Status);
            Assert.Null(rows[0].EvidenceQuote);
            Assert.Equal("Suggestion for Kubernetes.", rows[0].Suggestion);
            Assert.Equal("SQL Server", rows[1].Name);
            Assert.Equal(MatchStatus.Partial, rows[1].Status);
            Assert.Equal("Developed REST APIs with C# and SQL Server", rows[1].EvidenceQuote);
            Assert.Equal("Explanation of SQL Server.", rows[1].Explanation);
            Assert.All(rows, row => Assert.Equal("Ollama:test-model", row.ModelIdentity));

            Assert.Contains(context.RequirementMatches, requirementMatch => requirementMatch.Status == MatchStatus.Met);
            Assert.Empty(result.Warnings);
        }

        [Fact]
        public async Task AnalyzeWithJobPostingAsync_ScoresTheSame_WithAndWithoutInterpretations()
        {
            var (withoutInterpretations, withoutContext) = await AnalyzeWithInterpreterAsync(FakeRequirementInterpreter.ReturningNothing(), MetThenMissing);
            var (withInterpretations, withContext) = await AnalyzeWithInterpreterAsync(
                new FakeRequirementInterpreter(requirements => OutcomeFor(requirements)), MetThenMissing);

            Assert.Equal(withoutInterpretations.Scan.OverallScore, withInterpretations.Scan.OverallScore);
            Assert.Equal(
                GetJobMatchSectionScore(withoutContext, withoutInterpretations.Scan.Id).Score,
                GetJobMatchSectionScore(withContext, withInterpretations.Scan.Id).Score);
        }

        [Fact]
        public async Task AnalyzeWithJobPostingAsync_GivesTheInterpreterTheExtractedRequirementsAndTheAnalysisCvText()
        {
            var interpreter = new FakeRequirementInterpreter(requirements => OutcomeFor(requirements));

            await AnalyzeWithInterpreterAsync(interpreter, MetThenMissing);

            Assert.Equal(1, interpreter.CallCount);
            Assert.Equal(TwoExtractedRequirements, Assert.Single(interpreter.ReceivedRequirementLists));
            Assert.Contains("SQL Server", Assert.Single(interpreter.ReceivedCvTexts));
        }

        [Fact]
        public async Task AnalyzeWithJobPostingAsync_NeverCreatesTheInterpreter_WhenAiIsDisabled()
        {
            var (result, _) = await AnalyzeAsync(
                AiAvailability.Disabled(),
                new Lazy<IRequirementExtractor>(() => throw new InvalidOperationException("extractor must not be created")),
                new Lazy<ISemanticMatcher>(() => throw new InvalidOperationException("matcher must not be created")),
                requirementInterpreter: new Lazy<IRequirementInterpreter>(() => throw new InvalidOperationException("interpreter must not be created")));

            Assert.Empty(result.Warnings);
        }

        [Fact]
        public async Task AnalyzeWithJobPostingAsync_NeverCreatesTheInterpreter_WhenThereAreNoRequirementMatches()
        {
            var (result, _) = await AnalyzeAsync(
                AiAvailability.Ok("Ollama"),
                new Lazy<IRequirementExtractor>(() => new FakeRequirementExtractor(TwoExtractedRequirements)),
                new Lazy<ISemanticMatcher>(() => new FakeSemanticMatcher(_ => throw new InvalidOperationException("embedding model crashed"))),
                requirementInterpreter: new Lazy<IRequirementInterpreter>(() => throw new InvalidOperationException("interpreter must not be created")));

            Assert.Contains(result.Warnings, warning => warning.Contains("semantic layer failed"));
            Assert.DoesNotContain(result.Warnings, warning => warning.Contains("interpretation"));
        }

        [Fact]
        public async Task AnalyzeWithJobPostingAsync_WhenTheInterpreterFails_KeepsTheMatchesAndTheScoreAndAddsAWarning()
        {
            var (expected, expectedContext) = await AnalyzeWithInterpreterAsync(FakeRequirementInterpreter.ReturningNothing(), MetThenMissing);
            var interpreter = new FakeRequirementInterpreter(_ => throw new InvalidOperationException("model crashed"));

            var (result, context) = await AnalyzeWithInterpreterAsync(interpreter, MetThenMissing);

            Assert.Contains(result.Warnings, warning => warning.Contains("requirement interpretation failed"));
            Assert.Equal(2, context.RequirementMatches.Count(requirementMatch => requirementMatch.CvScanId == result.Scan.Id));
            Assert.Empty(context.RequirementInterpretations);
            Assert.Equal(
                GetJobMatchSectionScore(expectedContext, expected.Scan.Id).Score,
                GetJobMatchSectionScore(context, result.Scan.Id).Score);
        }

        [Fact]
        public async Task AnalyzeWithJobPostingAsync_WarnsAboutUninterpretedRequirementsAndDowngradedDecisions()
        {
            var interpreter = new FakeRequirementInterpreter(requirements => OutcomeFor(requirements, uninterpretedCount: 1, downgradedCount: 2));

            var (result, _) = await AnalyzeWithInterpreterAsync(interpreter, MetThenMissing);

            Assert.Contains(result.Warnings, warning => warning.Contains("could not interpret 1 of 2 requirements"));
            Assert.Contains(result.Warnings, warning => warning.Contains("2 of the language model's decisions were treated as not met"));
        }

        [Fact]
        public async Task AnalyzeWithJobPostingAsync_ShortensAnOverlongModelIdentityToTheColumnLimit()
        {
            var identity = new string('m', PersistenceLimits.RequirementInterpretationModelIdentityMaximumLength + 50);
            var interpreter = new FakeRequirementInterpreter(requirements => OutcomeFor(requirements, modelIdentity: identity));

            var (_, context) = await AnalyzeWithInterpreterAsync(interpreter, MetThenMissing);

            Assert.All(context.RequirementInterpretations.ToList(), interpretation =>
                Assert.Equal(PersistenceLimits.RequirementInterpretationModelIdentityMaximumLength, interpretation.ModelIdentity.Length));
        }

        [Fact]
        public async Task AnalyzeWithJobPostingAsync_GivesEveryRequirementWithTheSameNameTheSameInterpretation()
        {
            IReadOnlyList<RequirementMatchResult> TwoSpellingsOfOneRequirement(IReadOnlyList<ExtractedRequirement> _) => new[]
            {
                new RequirementMatchResult(new ExtractedRequirement("SQL Server", true, "Database"), 0.9, MatchStatus.Met, "evidence"),
                new RequirementMatchResult(new ExtractedRequirement("sql   server", true, "Database"), 0.9, MatchStatus.Met, "evidence")
            };
            var interpretation = new RequirementInterpretationResult(
                "SQL Server", MatchStatus.Met, "Developed REST APIs with C# and SQL Server", "Shown.", null);
            var interpreter = new FakeRequirementInterpreter(_ => new RequirementInterpretationOutcome(new[] { interpretation }, 0, 0, "Ollama:test-model"));

            var (_, context) = await AnalyzeWithInterpreterAsync(interpreter, TwoSpellingsOfOneRequirement);

            Assert.Equal(2, context.RequirementInterpretations.Count());
        }

        [Fact]
        public async Task AnalyzeWithJobPostingAsync_WritesTheTokenAndEmbeddingCountsOfTheAnalysisToTheAudit()
        {
            var aiUsageTracker = new AiUsageTracker();
            var interpreter = new FakeRequirementInterpreter(requirements =>
            {
                aiUsageTracker.RecordChatUsage(120, 45);
                return OutcomeFor(requirements);
            });

            var (result, context) = await AnalyzeAsync(
                AiAvailability.Ok("Ollama"),
                new Lazy<IRequirementExtractor>(() => new FakeRequirementExtractor(TwoExtractedRequirements)),
                new Lazy<ISemanticMatcher>(() => new FakeSemanticMatcher(extractedRequirements =>
                {
                    aiUsageTracker.RecordEmbeddingUsage(cacheHitCount: 3, generatedCount: 2);
                    return MetThenMissing(extractedRequirements);
                })),
                requirementInterpreter: new Lazy<IRequirementInterpreter>(() => interpreter),
                aiUsageTracker: aiUsageTracker);

            var audit = context.AnalysisAudits.Single(analysisAudit => analysisAudit.CvScanId == result.Scan.Id);
            Assert.Equal(120, audit.InputTokenCount);
            Assert.Equal(45, audit.OutputTokenCount);
            Assert.Equal(3, audit.EmbeddingCacheHitCount);
            Assert.Equal(2, audit.EmbeddingGeneratedCount);
        }

        [Fact]
        public async Task AnalyzeWithJobPostingAsync_DoesNotCountTheUsageOfEarlierAnalyses()
        {
            var aiUsageTracker = new AiUsageTracker();
            aiUsageTracker.RecordChatUsage(999, 999);
            aiUsageTracker.RecordEmbeddingUsage(9, 9);

            var (result, context) = await AnalyzeAsync(
                AiAvailability.Ok("Ollama"),
                new Lazy<IRequirementExtractor>(() => new FakeRequirementExtractor(TwoExtractedRequirements)),
                new Lazy<ISemanticMatcher>(() => new FakeSemanticMatcher(MetThenMissing)),
                aiUsageTracker: aiUsageTracker);

            var audit = context.AnalysisAudits.Single(analysisAudit => analysisAudit.CvScanId == result.Scan.Id);
            Assert.Null(audit.InputTokenCount);
            Assert.Null(audit.OutputTokenCount);
            Assert.Null(audit.EmbeddingCacheHitCount);
            Assert.Null(audit.EmbeddingGeneratedCount);
        }
    }
}
