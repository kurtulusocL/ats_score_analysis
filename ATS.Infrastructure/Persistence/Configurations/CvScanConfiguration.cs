using ATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATS.Infrastructure.Persistence.Configurations
{
    public class CvScanConfiguration : IEntityTypeConfiguration<CvScan>
    {
        public void Configure(EntityTypeBuilder<CvScan> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CandidateName).IsRequired().HasMaxLength(200);

            builder.Property(x => x.RawText).IsRequired();

            builder.Property(x => x.FileType).IsRequired().HasMaxLength(10);

            builder.Property(x => x.FilePath).HasMaxLength(500);

            builder.Property(x => x.OverallScore).IsRequired().HasDefaultValue(0);

            builder.Property(x => x.IsJobMatched).IsRequired().HasDefaultValue(false);

            builder.HasMany(x => x.SectionScores).WithOne(x => x.CvScan).HasForeignKey(x => x.CvScanId).OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.JobPosting).WithOne(x => x.CvScan).HasForeignKey<CvScan>(x => x.JobPostingId).OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.ScoreReports).WithOne(x => x.CvScan).HasForeignKey(x => x.CvScanId).OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("CvScans");
        }
    }
}
