using ATS.Application.Abstract.Repository;
using ATS.Application.Abstract.Services;
using ATS.Application.Analyzers;
using ATS.Application.Analyzers.Base;
using ATS.Application.Analyzers.Base.Models;
using ATS.Core.Constants;
using ATS.Core.Extensions;
using ATS.Domain.Entities;
using Serilog;

namespace ATS.Infrastructure.Concrete.ServiceManagers
{
    public class CvScanManager : ICvScanService
    {
        private readonly IRepositoryBase<CvScan> _cvScanRepository;
        private readonly IRepositoryBase<SectionScore> _sectionScoreRepository;
        private readonly IRepositoryBase<JobPosting> _jobPostingRepository;
        private readonly IRepositoryBase<AnalyzerKeyword> _keywordRepository;
        private readonly IRepositoryBase<ScoreReport> _scoreReportRepository;
        private readonly ICvParserService _cvParserService;
        private readonly IReportService _reportService;
        private readonly ITranslatorService _translatorService;
        private readonly ILogger _logger = Log.ForContext<CvScanManager>();

        public CvScanManager(IRepositoryBase<CvScan> cvScanRepository, IRepositoryBase<SectionScore> sectionScoreRepository, IRepositoryBase<JobPosting> jobPostingRepository,
            IRepositoryBase<AnalyzerKeyword> keywordRepository, IRepositoryBase<ScoreReport> scoreReportRepository, ITranslatorService translatorService, ICvParserService cvParserService, IReportService reportService)
        {
            _cvScanRepository = cvScanRepository;
            _sectionScoreRepository = sectionScoreRepository;
            _jobPostingRepository = jobPostingRepository;
            _keywordRepository = keywordRepository;
            _scoreReportRepository = scoreReportRepository;
            _cvParserService = cvParserService;
            _translatorService = translatorService;
            _reportService = reportService;
        }

        public async Task<CvScan> AnalyzeAsync(string filePath, string fileType)
        {
            _logger.Information("Starting CV analysis. File: {FilePath}", filePath);

            var rawText = await _cvParserService.ParseAsync(filePath, fileType);
            var analysisText = await _translatorService.GetAnalysisTextAsync(rawText);
            var analyzers = await BuildAnalyzersAsync(analysisText, null);
            var orchestrator = new ScoringOrchestrator(analyzers);
            var orchestratorResult = orchestrator.Run(analysisText, Path.GetFileName(filePath));

            var cvScan = new CvScan
            {
                FilePath = filePath,
                FileType = fileType,
                RawText = rawText,
                CandidateName = analysisText.ExtractCandidateName(),
                OverallScore = orchestratorResult.TotalScore,
                IsJobMatched = false
            };
            await _cvScanRepository.AddAsync(cvScan);
            await _cvScanRepository.SaveChangesAsync();
            await SaveSectionScoresAsync(cvScan.Id, orchestratorResult.AnalyzerResults);
            _logger.Information("CV analysis completed. Score: {Score}/{Max}", orchestratorResult.TotalScore, orchestratorResult.TotalMaxScore);
            return cvScan;
        }

        public async Task<CvScan> AnalyzeWithJobPostingAsync(string filePath, string fileType, string jobPostingText, string jobTitle)
        {
            _logger.Information("Starting CV analysis with job posting. File: {FilePath}, Job: {JobTitle}", filePath, jobTitle);

            var rawText = await _cvParserService.ParseAsync(filePath, fileType);
            var analysisCvText = await _translatorService.GetAnalysisTextAsync(rawText);
            var analysisJobText = await _translatorService.GetAnalysisTextAsync(jobPostingText);
            var analyzers = await BuildAnalyzersAsync(analysisCvText, analysisJobText);
            var orchestrator = new ScoringOrchestrator(analyzers);
            var orchestratorResult = orchestrator.Run(analysisCvText, Path.GetFileName(filePath), analysisJobText);

            var cvScan = new CvScan
            {
                FilePath = filePath,
                FileType = fileType,
                RawText = rawText,
                CandidateName = analysisCvText.ExtractCandidateName(),
                OverallScore = orchestratorResult.TotalScore,
                IsJobMatched = true
            };
            await _cvScanRepository.AddAsync(cvScan);
            await _cvScanRepository.SaveChangesAsync();

            var jobPosting = new JobPosting
            {
                CvScanId = cvScan.Id,
                Title = jobTitle,
                RawText = jobPostingText,
                MatchScore = orchestratorResult.AnalyzerResults.FirstOrDefault(r => r.SectionName == "Job Match")?.Score ?? 0
            };
            await _jobPostingRepository.AddAsync(jobPosting);
            await _jobPostingRepository.SaveChangesAsync();
            await SaveSectionScoresAsync(cvScan.Id, orchestratorResult.AnalyzerResults);
            _logger.Information("CV analysis with job posting completed. Score: {Score}/{Max}", orchestratorResult.TotalScore, orchestratorResult.TotalMaxScore);
            return cvScan;
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
            return await _cvScanRepository.GetWithIncludesAsync(x => x.Id == id, x => x.SectionScores, x => x.JobPosting, x => x.ScoreReports);
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

        private async Task<List<IAnalyzer>> BuildAnalyzersAsync(string cvText, string? jobPostingText)
        {
            var allKeywords = await _keywordRepository.GetAllAsync();
            var activeKeywords = allKeywords.Where(k => k.IsActive).ToList();

            var impactVerbs = activeKeywords.Where(k => k.Category == KeywordCategories.ImpactVerb).Select(k => k.Word);
            var passiveIndicators = activeKeywords.Where(k => k.Category == KeywordCategories.PassiveIndicator).Select(k => k.Word);
            var sectionHeaders = activeKeywords.Where(k => k.Category == KeywordCategories.SectionHeader).ToList();
            var summaryHeaders = sectionHeaders.Where(k => k.SubCategory == "Summary").Select(k => k.Word);
            var experienceHeaders = sectionHeaders.Where(k => k.SubCategory == "Experience").Select(k => k.Word);
            var skillsHeaders = sectionHeaders.Where(k => k.SubCategory == "Skills").Select(k => k.Word);
            var allSectionWords = sectionHeaders.Select(k => k.Word);

            var analyzers = new List<IAnalyzer>
            {
                new SectionPresenceAnalyzer(sectionHeaders.Select(k => new { k.Word, k.SubCategory }).GroupBy(k => k.SubCategory)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Word).ToList())),
                new FormatAnalyzer(),
                new KeywordAnalyzer(impactVerbs, passiveIndicators),
                new ConsistencyAnalyzer(summaryHeaders, experienceHeaders, skillsHeaders)
            };
            if (!string.IsNullOrWhiteSpace(jobPostingText))
                analyzers.Add(new JobMatchAnalyzer(jobPostingText));

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
    }
}
