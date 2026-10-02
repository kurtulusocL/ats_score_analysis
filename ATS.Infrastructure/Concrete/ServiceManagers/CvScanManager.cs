using ATS.Application.Abstract.AI;
using ATS.Application.Abstract.Repository;
using ATS.Application.Abstract.Services;
using ATS.Application.Analyzers;
using ATS.Application.Analyzers.Base;
using ATS.Application.Analyzers.Base.Models;
using ATS.Application.Analyzers.JobMatching;
using ATS.Application.Results;
using ATS.Application.Security;
using ATS.Application.Semantic;
using ATS.Core.Constants;
using ATS.Core.Extensions;
using ATS.Core.Helpers;
using ATS.Domain.Entities;
using ATS.Domain.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ATS.Infrastructure.Concrete.ServiceManagers;

public class CvScanManager : ICvScanService
{
    private const string NotApplicableAiStatus = "NotApplicable";

    // Raporlar tarama kaydı yüklendikten sonra, bağlam kapanmışken üretilir; okudukları her şey burada yüklenmelidir.
    public static IReadOnlyList<string> ScanDetailIncludePaths { get; } = new[]
    {
            nameof(CvScan.SectionScores),
            nameof(CvScan.JobPosting),
            nameof(CvScan.ScoreReports),
            nameof(CvScan.SecurityFindings),
            nameof(CvScan.AnalysisWarnings),
            nameof(CvScan.AnalysisAudit),
            $"{nameof(CvScan.RequirementMatches)}.{nameof(RequirementMatch.JobRequirement)}",
            $"{nameof(CvScan.RequirementMatches)}.{nameof(RequirementMatch.RequirementInterpretation)}"
        };

    private readonly IRepositoryBase<CvScan> _cvScanRepository;
    private readonly IRepositoryBase<SectionScore> _sectionScoreRepository;
    private readonly IRepositoryBase<AnalyzerKeyword> _keywordRepository;
    private readonly IRepositoryBase<ScoreReport> _scoreReportRepository;
    private readonly ICvParserService _cvParserService;
    private readonly IReportService _reportService;
    private readonly ITranslatorService _translatorService;
    private readonly IAiAvailabilityService _aiAvailabilityService;
    private readonly Lazy<IRequirementExtractor> _requirementExtractor;
    private readonly Lazy<ISemanticMatcher> _semanticMatcher;
    private readonly Lazy<HybridScoreCalculator> _hybridScoreCalculator;
    private readonly ISecurityScanService _securityScanService;
    private readonly Lazy<IRequirementInterpreter> _requirementInterpreter;
    private readonly AiUsageTracker _aiUsageTracker;
    private readonly ILogger _logger = Log.ForContext<CvScanManager>();

    public CvScanManager(IRepositoryBase<CvScan> cvScanRepository, IRepositoryBase<SectionScore> sectionScoreRepository,
        IRepositoryBase<AnalyzerKeyword> keywordRepository, IRepositoryBase<ScoreReport> scoreReportRepository,
        ITranslatorService translatorService, ICvParserService cvParserService, IReportService reportService,
        IAiAvailabilityService aiAvailabilityService, Lazy<IRequirementExtractor> requirementExtractor,
        Lazy<ISemanticMatcher> semanticMatcher, Lazy<HybridScoreCalculator> hybridScoreCalculator,
        ISecurityScanService securityScanService, Lazy<IRequirementInterpreter> requirementInterpreter,
        AiUsageTracker aiUsageTracker)
    {
        _cvScanRepository = cvScanRepository;
        _sectionScoreRepository = sectionScoreRepository;
        _keywordRepository = keywordRepository;
        _scoreReportRepository = scoreReportRepository;
        _cvParserService = cvParserService;
        _translatorService = translatorService;
        _reportService = reportService;
        _aiAvailabilityService = aiAvailabilityService;
        _requirementExtractor = requirementExtractor;
        _semanticMatcher = semanticMatcher;
        _hybridScoreCalculator = hybridScoreCalculator;
        _securityScanService = securityScanService;
        _requirementInterpreter = requirementInterpreter;
        _aiUsageTracker = aiUsageTracker;
    }

    public async Task<AnalysisResult> AnalyzeAsync(string filePath, string fileType, CancellationToken cancellationToken = default)
    {
        _logger.Information("Starting CV analysis. File: {FilePath}", filePath);
        var stopwatch = Stopwatch.StartNew();
        var warnings = new List<string>();

        var rawText = await _cvParserService.ParseAsync(filePath, fileType, cancellationToken);
        var securityFindings = await RunSecurityScanAsync(filePath, fileType, rawText, warnings, cancellationToken);
        var scoringText = RemoveHiddenTextFromAnalysisText(rawText, securityFindings, warnings);
        var analysisText = await GetAnalysisTextOrFallbackAsync(scoringText, "CV", warnings, cancellationToken);
        var analyzers = await BuildAnalyzersAsync(analysisText, null, null);
        var orchestrator = new ScoringOrchestrator(analyzers);
        var orchestratorResult = orchestrator.Run(analysisText, Path.GetFileName(filePath));

        // Kayıt aşamasına geçtikten sonra iptal edilmez; yarım kayıt kalmasın.
        cancellationToken.ThrowIfCancellationRequested();

        var cvScan = new CvScan
        {
            FilePath = filePath,
            FileType = fileType,
            RawText = rawText,
            CandidateName = analysisText.ExtractCandidateName(),
            OverallScore = orchestratorResult.TotalScore,
            IsJobMatched = false
        };

        // İlansız analizde yapay zekâ katmanı çalışmaz; skor ve kullanım alanları boştur.
        AttachSecurityFindingsWarningsAndAudit(cvScan, securityFindings, warnings, new AnalysisAuditDetails(
            AiStatus: NotApplicableAiStatus,
            AiProviderName: null,
            DeterministicJobMatchScore: null,
            HybridJobMatchScore: null,
            SemanticJobMatchScore: null,
            DurationMilliseconds: (int)stopwatch.ElapsedMilliseconds,
            Usage: AiUsageSnapshot.Empty));

        await _cvScanRepository.AddAsync(cvScan);
        await _cvScanRepository.SaveChangesAsync();
        await SaveSectionScoresAsync(cvScan.Id, orchestratorResult.AnalyzerResults);
        _logger.Information("CV analysis completed. Score: {Score}/{Max}", orchestratorResult.TotalScore, orchestratorResult.TotalMaxScore);
        return new AnalysisResult(cvScan, warnings, null, securityFindings);
    }

    public async Task<AnalysisResult> AnalyzeWithJobPostingAsync(string filePath, string fileType, string jobPostingText, string jobTitle, CancellationToken cancellationToken = default)
    {
        _logger.Information("Starting CV analysis with job posting. File: {FilePath}, Job: {JobTitle}", filePath, jobTitle);
        var stopwatch = Stopwatch.StartNew();
        var warnings = new List<string>();

        // Sayaçlar yalnızca bu analizin çağrılarını saysın.
        _aiUsageTracker.Reset();

        var ai = await _aiAvailabilityService.CheckAsync(cancellationToken);
        _logger.Information("AI availability: {Status}, Provider: {Provider}", ai.Status, ai.ProviderName);
        if (!ai.IsAvailable && ai.Status != AiStatus.Disabled)
            warnings.Add(ai.Message);

        var rawText = await _cvParserService.ParseAsync(filePath, fileType, cancellationToken);
        var cvSecurityFindings = await RunSecurityScanAsync(filePath, fileType, rawText, warnings, cancellationToken);
        var jobPostingSecurityFindings = await RunJobPostingSecurityScanAsync(jobPostingText, warnings, cancellationToken);
        var securityFindings = cvSecurityFindings.Concat(jobPostingSecurityFindings).ToList();
        var scoringCvText = RemoveHiddenTextFromAnalysisText(rawText, cvSecurityFindings, warnings);
        var analysisCvText = await GetAnalysisTextOrFallbackAsync(scoringCvText, "CV", warnings, cancellationToken);
        var analysisJobText = await GetAnalysisTextOrFallbackAsync(jobPostingText, "job posting", warnings, cancellationToken);
        var analyzers = await BuildAnalyzersAsync(analysisCvText, analysisJobText, jobTitle);
        var orchestrator = new ScoringOrchestrator(analyzers);
        var orchestratorResult = orchestrator.Run(analysisCvText, Path.GetFileName(filePath), analysisJobText);

        // Hibrit skor uygulanmadan önceki (deterministik) Job Match puanı.
        var deterministicJobMatchScore = orchestratorResult.AnalyzerResults
            .FirstOrDefault(analyzerResult => analyzerResult.SectionName == "Job Match")?.Score;

        IReadOnlyList<RequirementMatchResult> requirementMatchResults = Array.Empty<RequirementMatchResult>();
        HybridScoreResult? appliedHybridScoreResult = null;
        var interpretationOutcome = RequirementInterpretationOutcome.None;
        if (ai.IsAvailable)
        {
            requirementMatchResults = await RunSemanticLayerAsync(analysisCvText, analysisJobText, warnings, cancellationToken);

            try
            {
                appliedHybridScoreResult = ApplyHybridJobMatchScore(orchestratorResult, requirementMatchResults);
            }
            catch (Exception exception)
            {
                _logger.Warning(exception, "Hybrid score could not be applied. Falling back to the deterministic Job Match score.");
                warnings.Add("The hybrid score could not be calculated. Only the deterministic Job Match score was used.");
                requirementMatchResults = Array.Empty<RequirementMatchResult>();
                appliedHybridScoreResult = null;
            }

            // Yorumlar skoru etkilemez; yalnızca gerekçe ve öneri üretir.
            if (requirementMatchResults.Count > 0)
                interpretationOutcome = await RunInterpretationAsync(requirementMatchResults, analysisCvText, warnings, cancellationToken);
        }

        // Kayıt aşamasına geçtikten sonra iptal edilmez; yarım kayıt kalmasın.
        cancellationToken.ThrowIfCancellationRequested();

        var jobPosting = new JobPosting
        {
            Title = jobTitle,
            RawText = jobPostingText
        };

        var cvScan = new CvScan
        {
            FilePath = filePath,
            FileType = fileType,
            RawText = rawText,
            CandidateName = analysisCvText.ExtractCandidateName(),
            OverallScore = orchestratorResult.TotalScore,
            IsJobMatched = true,
            JobPosting = jobPosting
        };

        AddRequirementMatches(cvScan, jobPosting, requirementMatchResults, interpretationOutcome);

        AttachSecurityFindingsWarningsAndAudit(cvScan, securityFindings, warnings, new AnalysisAuditDetails(
            AiStatus: ai.Status.ToString(),
            AiProviderName: ai.ProviderName,
            DeterministicJobMatchScore: deterministicJobMatchScore,
            HybridJobMatchScore: appliedHybridScoreResult?.HybridScore,
            SemanticJobMatchScore: appliedHybridScoreResult?.SemanticScore,
            DurationMilliseconds: (int)stopwatch.ElapsedMilliseconds,
            Usage: _aiUsageTracker.GetSnapshot()));

        await _cvScanRepository.AddAsync(cvScan);
        await _cvScanRepository.SaveChangesAsync();
        await SaveSectionScoresAsync(cvScan.Id, orchestratorResult.AnalyzerResults);

        _logger.Information("CV analysis with job posting completed. Score: {Score}/{Max}", orchestratorResult.TotalScore, orchestratorResult.TotalMaxScore);
        return new AnalysisResult(cvScan, warnings, ai, securityFindings);
    }

    public async Task<IEnumerable<CvScan>> GetAllScansAsync()
    {
        _logger.Information("Fetching all CV scans.");
        return await _cvScanRepository.GetAllAsync();
    }

    public async Task<CvScan?> GetScanByIdAsync(int id)
    {
        _logger.Information("Fetching CV scan. Id: {Id}", id);
        return await _cvScanRepository.GetByIdAsync(id);
    }

    public async Task<CvScan?> GetScanWithDetailsAsync(int id)
    {
        _logger.Information("Fetching CV scan with details. Id: {Id}", id);
        return await _cvScanRepository.GetWithIncludePathsAsync(scan => scan.Id == id, ScanDetailIncludePaths.ToArray());
    }

    public async Task DeleteScanAsync(int id)
    {
        _logger.Information("Deleting CV scan. Id: {Id}", id);
        var scan = await _cvScanRepository.GetByIdAsync(id);
        if (scan == null)
        {
            _logger.Warning("CV scan not found for deletion. Id: {Id}", id);
            return;
        }
        await _cvScanRepository.DeleteAsync(scan);
        await _cvScanRepository.SaveChangesAsync();
        _logger.Information("CV scan deleted. Id: {Id}", id);
    }

    public async Task SaveReportAsync(CvScan cvScan, string reportType)
    {
        try
        {
            var reportPath = reportType == "general"
                ? _reportService.GenerateGeneralReport(cvScan)
                : _reportService.GenerateJobMatchReport(cvScan);

            var scoreReport = new ScoreReport
            {
                CvScanId = cvScan.Id,
                ReportPath = reportPath,
                ReportType = reportType,
                IsGenerated = true
            };
            await _scoreReportRepository.AddAsync(scoreReport);
            await _scoreReportRepository.SaveChangesAsync();
            _logger.Information("Score report saved. CvScanId: {Id}, Path: {Path}", cvScan.Id, reportPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save score report. CvScanId: {Id}", cvScan.Id);
            throw;
        }
    }

    private string RemoveHiddenTextFromAnalysisText(string rawText, IReadOnlyList<SecurityFindingResult> cvSecurityFindings, List<string> warnings)
    {
        var hiddenTexts = cvSecurityFindings
            .Where(securityFinding => securityFinding.Type == SecurityFindingType.HiddenText)
            .Select(securityFinding => securityFinding.Snippet)
            .ToList();

        if (hiddenTexts.Count == 0)
            return rawText;

        var removalResult = HiddenTextRemover.Remove(rawText, hiddenTexts);
        _logger.Information("Hidden text excluded from scoring. Removed: {RemovedCount}, NotFound: {NotFoundCount}", removalResult.RemovedCount, removalResult.NotFoundCount);

        if (removalResult.NotFoundCount > 0)
            warnings.Add("Some hidden text could not be located in the extracted CV text, so it may not have been excluded from scoring.");

        return removalResult.Text;
    }

    private async Task<IReadOnlyList<SecurityFindingResult>> RunSecurityScanAsync(string filePath, string fileType, string rawText, List<string> warnings, CancellationToken cancellationToken)
    {
        try
        {
            var scanResult = await _securityScanService.ScanAsync(filePath, fileType, rawText, cancellationToken);
            warnings.AddRange(scanResult.Warnings);
            return scanResult.Findings.ToList();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.Warning(exception, "Security scan failed. Continuing without security findings.");
            warnings.Add("The security scan could not be completed. No security findings were produced for this CV.");
            return Array.Empty<SecurityFindingResult>();
        }
    }

    private async Task<IReadOnlyList<SecurityFindingResult>> RunJobPostingSecurityScanAsync(string jobPostingText, List<string> warnings, CancellationToken cancellationToken)
    {
        try
        {
            var scanResult = await _securityScanService.ScanJobPostingAsync(jobPostingText, cancellationToken);
            warnings.AddRange(scanResult.Warnings);
            return scanResult.Findings.ToList();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.Warning(exception, "Job posting security scan failed. Continuing without job posting findings.");
            warnings.Add("The security scan of the job posting could not be completed. No security findings were produced for the job posting.");
            return Array.Empty<SecurityFindingResult>();
        }
    }

    private static void AttachSecurityFindingsWarningsAndAudit(
        CvScan cvScan, IReadOnlyList<SecurityFindingResult> securityFindings, IReadOnlyList<string> warnings, AnalysisAuditDetails auditDetails)
    {
        foreach (var securityFinding in securityFindings)
        {
            cvScan.SecurityFindings.Add(new SecurityFinding
            {
                Type = securityFinding.Type,
                Severity = securityFinding.Severity,
                Description = TextTruncationHelper.Truncate(securityFinding.Description, PersistenceLimits.SecurityFindingDescriptionMaximumLength),
                Snippet = securityFinding.Snippet
            });
        }

        foreach (var warning in warnings)
        {
            cvScan.AnalysisWarnings.Add(new AnalysisWarning
            {
                Message = TextTruncationHelper.Truncate(warning, PersistenceLimits.AnalysisWarningMessageMaximumLength)
            });
        }

        cvScan.AnalysisAudit = new AnalysisAudit
        {
            AiStatus = auditDetails.AiStatus,
            AiProviderName = auditDetails.AiProviderName,
            DeterministicJobMatchScore = auditDetails.DeterministicJobMatchScore,
            HybridJobMatchScore = auditDetails.HybridJobMatchScore,
            SemanticJobMatchScore = auditDetails.SemanticJobMatchScore,
            DurationMilliseconds = auditDetails.DurationMilliseconds,
            InputTokenCount = auditDetails.Usage.InputTokenCount,
            OutputTokenCount = auditDetails.Usage.OutputTokenCount,
            EmbeddingCacheHitCount = auditDetails.Usage.EmbeddingCacheHitCount,
            EmbeddingGeneratedCount = auditDetails.Usage.EmbeddingGeneratedCount
        };
    }

    private async Task<string> GetAnalysisTextOrFallbackAsync(string text, string subject, List<string> warnings, CancellationToken cancellationToken)
    {
        try
        {
            return await _translatorService.GetAnalysisTextAsync(text, cancellationToken);
        }
        catch (TranslationUnavailableException ex)
        {
            _logger.Warning(ex, "Translation unavailable for {Subject}. Using untranslated text.", subject);
            warnings.Add($"The translation service was unavailable for the {subject}. The untranslated text was analyzed, so scores for non-English text may be unreliable.");
            return text;
        }
    }

    private async Task<IReadOnlyList<RequirementMatchResult>> RunSemanticLayerAsync(
        string analysisCvText, string analysisJobText, List<string> warnings, CancellationToken cancellationToken)
    {
        try
        {
            var extractedRequirements = await _requirementExtractor.Value.ExtractAsync(analysisJobText, cancellationToken);
            if (extractedRequirements.Count == 0)
            {
                warnings.Add("No requirements could be extracted from the job posting. The semantic layer was skipped.");
                return Array.Empty<RequirementMatchResult>();
            }

            return await _semanticMatcher.Value.MatchAsync(extractedRequirements, analysisCvText, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.Warning(exception, "Semantic layer failed. Falling back to the deterministic Job Match score.");
            warnings.Add("The semantic layer failed. Only the deterministic Job Match score was used.");
            return Array.Empty<RequirementMatchResult>();
        }
    }

    private async Task<RequirementInterpretationOutcome> RunInterpretationAsync(
        IReadOnlyList<RequirementMatchResult> requirementMatchResults, string analysisCvText, List<string> warnings, CancellationToken cancellationToken)
    {
        try
        {
            var requirements = requirementMatchResults.Select(requirementMatchResult => requirementMatchResult.ExtractedRequirement).ToList();
            var outcome = await _requirementInterpreter.Value.InterpretAsync(requirements, analysisCvText, cancellationToken);

            AddInterpretationWarnings(outcome, requirements.Count, warnings);
            return outcome;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.Warning(exception, "Requirement interpretation failed. The analysis continues without language model explanations.");
            warnings.Add("The requirement interpretation failed. The Job Match score is not affected, but there are no language model explanations.");
            return RequirementInterpretationOutcome.None;
        }
    }

    private static void AddInterpretationWarnings(RequirementInterpretationOutcome outcome, int requirementCount, List<string> warnings)
    {
        if (outcome.UninterpretedRequirementCount > 0)
        {
            warnings.Add(string.Format(CultureInfo.InvariantCulture,
                "The language model could not interpret {0} of {1} requirements, so they have no explanation.",
                outcome.UninterpretedRequirementCount, requirementCount));
        }

        if (outcome.DowngradedCount > 0)
        {
            warnings.Add(string.Format(CultureInfo.InvariantCulture,
                "{0} of the language model's decisions were treated as not met because the evidence it quoted could not be found in the CV.",
                outcome.DowngradedCount));
        }
    }

    private static void AddRequirementMatches(
        CvScan cvScan, JobPosting jobPosting, IReadOnlyList<RequirementMatchResult> requirementMatchResults, RequirementInterpretationOutcome interpretationOutcome)
    {
        var interpretationsByRequirementKey = IndexInterpretationsByRequirement(interpretationOutcome.Interpretations);

        foreach (var requirementMatchResult in requirementMatchResults)
        {
            var jobRequirement = new JobRequirement
            {
                Name = requirementMatchResult.ExtractedRequirement.Name,
                IsMandatory = requirementMatchResult.ExtractedRequirement.IsMandatory,
                Category = requirementMatchResult.ExtractedRequirement.Category,
                JobPosting = jobPosting
            };
            jobPosting.JobRequirements.Add(jobRequirement);

            var requirementMatch = new RequirementMatch
            {
                Similarity = requirementMatchResult.Similarity,
                Status = requirementMatchResult.Status,
                Evidence = requirementMatchResult.Evidence,
                CvScan = cvScan,
                JobRequirement = jobRequirement
            };

            var requirementKey = TextWhitespaceHelper.Collapse(requirementMatchResult.ExtractedRequirement.Name);
            if (interpretationsByRequirementKey.TryGetValue(requirementKey, out var interpretation))
                requirementMatch.RequirementInterpretation = CreateRequirementInterpretation(interpretation, interpretationOutcome.ModelIdentity, requirementMatch);

            cvScan.RequirementMatches.Add(requirementMatch);
        }
    }

    private static Dictionary<string, RequirementInterpretationResult> IndexInterpretationsByRequirement(
        IReadOnlyList<RequirementInterpretationResult> interpretations)
    {
        var interpretationsByRequirementKey = new Dictionary<string, RequirementInterpretationResult>(StringComparer.OrdinalIgnoreCase);

        foreach (var interpretation in interpretations)
            interpretationsByRequirementKey.TryAdd(TextWhitespaceHelper.Collapse(interpretation.RequirementName), interpretation);

        return interpretationsByRequirementKey;
    }

    // Metin sınırları doğrulayıcıda zaten kolon sınırlarına eşitlenmiştir; yalnızca model kimliği burada kırpılır.
    private static RequirementInterpretation CreateRequirementInterpretation(
        RequirementInterpretationResult interpretation, string modelIdentity, RequirementMatch requirementMatch) => new()
        {
            Status = interpretation.Status,
            EvidenceQuote = interpretation.EvidenceQuote,
            Explanation = interpretation.Explanation,
            Suggestion = interpretation.Suggestion,
            ModelIdentity = TextTruncationHelper.Truncate(modelIdentity, PersistenceLimits.RequirementInterpretationModelIdentityMaximumLength),
            RequirementMatch = requirementMatch
        };

    private HybridScoreResult? ApplyHybridJobMatchScore(OrchestratorResult orchestratorResult, IReadOnlyList<RequirementMatchResult> requirementMatchResults)
    {
        var jobMatchResult = orchestratorResult.AnalyzerResults.FirstOrDefault(analyzerResult => analyzerResult.SectionName == "Job Match");
        if (jobMatchResult == null || requirementMatchResults.Count == 0)
            return null;

        var hybridScoreResult = _hybridScoreCalculator.Value.Calculate(jobMatchResult.Score, jobMatchResult.MaxScore, requirementMatchResults);
        if (!hybridScoreResult.IsSemanticScoreApplied)
            return null;

        jobMatchResult.Feedbacks.Add(string.Format(
            CultureInfo.InvariantCulture,
            "Hybrid Job Match: {0}/{1} (keyword score {2}, semantic score {3:0.0})",
            hybridScoreResult.HybridScore, jobMatchResult.MaxScore, hybridScoreResult.DeterministicScore, hybridScoreResult.SemanticScore));

        foreach (var requirementMatchResult in requirementMatchResults)
        {
            var requirementName = requirementMatchResult.ExtractedRequirement.Name;
            var priority = requirementMatchResult.ExtractedRequirement.IsMandatory ? "mandatory" : "optional";
            var similarity = requirementMatchResult.Similarity.ToString("0.00", CultureInfo.InvariantCulture);

            switch (requirementMatchResult.Status)
            {
                case MatchStatus.Met:
                    jobMatchResult.Feedbacks.Add($"✓ Requirement met: {requirementName} ({priority}, similarity {similarity})");
                    break;
                case MatchStatus.Partial:
                    jobMatchResult.Feedbacks.Add($"Requirement partially met: {requirementName} ({priority}, similarity {similarity})");
                    break;
                default:
                    jobMatchResult.MissingItems.Add($"{requirementName} ({priority})");
                    break;
            }
        }

        jobMatchResult.Score = hybridScoreResult.HybridScore;
        jobMatchResult.IsPassed = JobMatchScoringRules.IsPassing(jobMatchResult.Score, jobMatchResult.MaxScore);
        orchestratorResult.RecalculateTotals();
        return hybridScoreResult;
    }

    private async Task<List<IAnalyzer>> BuildAnalyzersAsync(string cvText, string? jobPostingText, string? jobTitle)
    {
        var allKeywords = await _keywordRepository.GetAllAsync();
        var activeKeywords = allKeywords.Where(k => k.IsActive).ToList();

        var impactVerbs = activeKeywords.Where(k => k.Category == KeywordCategories.ImpactVerb).Select(k => k.Word);
        var passiveIndicators = activeKeywords.Where(k => k.Category == KeywordCategories.PassiveIndicator).Select(k => k.Word);
        var sectionHeaders = activeKeywords.Where(k => k.Category == KeywordCategories.SectionHeader).ToList();
        var summaryHeaders = sectionHeaders.Where(k => k.SubCategory == "Summary").Select(k => k.Word);
        var experienceHeaders = sectionHeaders.Where(k => k.SubCategory == "Experience").Select(k => k.Word);
        var skillsHeaders = sectionHeaders.Where(k => k.SubCategory == "Skills").Select(k => k.Word);

        var analyzers = new List<IAnalyzer>
            {
                new SectionPresenceAnalyzer(sectionHeaders.Select(k => new { k.Word, k.SubCategory }).GroupBy(k => k.SubCategory)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Word).ToList())),
                new FormatAnalyzer(),
                new KeywordAnalyzer(impactVerbs, passiveIndicators),
                new ConsistencyAnalyzer(summaryHeaders, experienceHeaders, skillsHeaders)
            };
        if (!string.IsNullOrWhiteSpace(jobPostingText))
            analyzers.Add(new JobMatchAnalyzer(jobPostingText, jobTitle ?? string.Empty));

        return analyzers;
    }

    private async Task SaveSectionScoresAsync(int cvScanId, List<AnalyzerResult> results)
    {
        foreach (var analyzerResult in results)
        {
            var sectionScore = new SectionScore
            {
                CvScanId = cvScanId,
                SectionName = analyzerResult.SectionName,
                Score = analyzerResult.Score,
                MaxScore = analyzerResult.MaxScore,
                IsPassed = analyzerResult.IsPassed,
                Feedback = string.Join("|", analyzerResult.Feedbacks.Concat(analyzerResult.Suggestions).Concat(analyzerResult.MissingItems.Select(m => $"MISSING: {m}")))
            };
            await _sectionScoreRepository.AddAsync(sectionScore);
        }
        await _sectionScoreRepository.SaveChangesAsync();
    }
}
