using ATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATS.Infrastructure.Persistence.Configurations
{
    public class AnalysisWarningConfiguration : IEntityTypeConfiguration<AnalysisWarning>
    {
        public void Configure(EntityTypeBuilder<AnalysisWarning> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Message).IsRequired().HasMaxLength(1000);
            builder.HasOne(x => x.CvScan).WithMany(x => x.AnalysisWarnings).HasForeignKey(x => x.CvScanId).OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("AnalysisWarnings");
        }
    }
}
