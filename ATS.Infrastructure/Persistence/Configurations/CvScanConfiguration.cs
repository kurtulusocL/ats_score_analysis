using ATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATS.Infrastructure.Persistence.Configurations;

public class CvScanConfiguration : IEntityTypeConfiguration<CvScan>
{
	public void Configure(EntityTypeBuilder<CvScan> builder)
	{
		builder.HasKey((CvScan x) => x.Id);
		builder.Property((CvScan x) => x.CandidateName).IsRequired().HasMaxLength(200);
		builder.Property((CvScan x) => x.RawText).IsRequired();
		builder.Property((CvScan x) => x.FileType).IsRequired().HasMaxLength(10);
		builder.Property((CvScan x) => x.FilePath).HasMaxLength(500);
		builder.Property((CvScan x) => x.OverallScore).IsRequired().HasDefaultValue(0);
		builder.Property((CvScan x) => x.IsJobMatched).IsRequired().HasDefaultValue(false);
		builder.HasMany((CvScan x) => x.SectionScores).WithOne((SectionScore x) => x.CvScan).HasForeignKey((SectionScore x) => x.CvScanId)
			.OnDelete(DeleteBehavior.Cascade);
		builder.HasOne((CvScan x) => x.JobPosting).WithMany((JobPosting x) => x.CvScans).HasForeignKey((CvScan x) => x.JobPostingId)
			.OnDelete(DeleteBehavior.SetNull);
		builder.HasMany((CvScan x) => x.ScoreReports).WithOne((ScoreReport x) => x.CvScan).HasForeignKey((ScoreReport x) => x.CvScanId)
			.OnDelete(DeleteBehavior.Cascade);
		builder.ToTable("CvScans");
	}
}
