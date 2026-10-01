using ATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATS.Infrastructure.Persistence.Configurations
{
    public class AnalysisAuditConfiguration : IEntityTypeConfiguration<AnalysisAudit>
    {
        public void Configure(EntityTypeBuilder<AnalysisAudit> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.AiStatus).IsRequired().HasMaxLength(30);
            builder.Property(x => x.AiProviderName).HasMaxLength(50);
            builder.HasOne(x => x.CvScan).WithOne(x => x.AnalysisAudit).HasForeignKey<AnalysisAudit>(x => x.CvScanId).OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("AnalysisAudits");
        }
    }
}
