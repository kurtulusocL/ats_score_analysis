using ATS.Domain.Entities;
using ATS.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace ATS.Infrastructure.Persistence.Context.Mssql;

public class ApplicationDbContext : DbContext
{
	public DbSet<AnalyzerKeyword> AnalyzerKeywords { get; set; }

	public DbSet<CvScan> CvScans { get; set; }

	public DbSet<JobPosting> JobPostings { get; set; }

	public DbSet<SectionScore> SectionScores { get; set; }

	public DbSet<ScoreReport> ScoreReports { get; set; }

	public DbSet<JobRequirement> JobRequirements { get; set; }

	public DbSet<RequirementMatch> RequirementMatches { get; set; }

	public DbSet<EmbeddingCacheEntry> EmbeddingCacheEntries { get; set; }

	public DbSet<SecurityFinding> SecurityFindings { get; set; }

	public DbSet<AnalysisAudit> AnalysisAudits { get; set; }

	public DbSet<AnalysisWarning> AnalysisWarnings { get; set; }

	public DbSet<RequirementInterpretation> RequirementInterpretations { get; set; }

	public DbSet<SkillTaxonomyEntry> SkillTaxonomyEntries { get; set; }

	public DbSet<SkillTaxonomyAlias> SkillTaxonomyAliases { get; set; }

	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseLazyLoadingProxies();
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
		AnalyzerKeywordSeed.Seed(modelBuilder);
		SkillTaxonomySeed.Seed(modelBuilder);
	}
}
