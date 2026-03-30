using ATS.Domain.Entities;
using ATS.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace ATS.Infrastructure.Persistence.Context.Mssql
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<AnalyzerKeyword> AnalyzerKeywords { get; set; }
        public DbSet<CvScan> CvScans { get; set; }
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<SectionScore> SectionScores { get; set; }       
        public DbSet<ScoreReport> ScoreReports { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            AnalyzerKeywordSeed.Seed(modelBuilder);
        }
    }
}
