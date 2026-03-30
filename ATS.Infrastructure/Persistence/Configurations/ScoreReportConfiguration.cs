using ATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATS.Infrastructure.Persistence.Configurations
{
    public class ScoreReportConfiguration : IEntityTypeConfiguration<ScoreReport>
    {
        public void Configure(EntityTypeBuilder<ScoreReport> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReportPath).IsRequired().HasMaxLength(500);

            builder.Property(x => x.ReportType).IsRequired().HasMaxLength(20);

            builder.Property(x => x.IsGenerated).IsRequired().HasDefaultValue(false);

            builder.ToTable("ScoreReports");
        }
    }
}
