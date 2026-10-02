using ATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATS.Infrastructure.Persistence.Configurations;

public class SectionScoreConfiguration : IEntityTypeConfiguration<SectionScore>
{
	public void Configure(EntityTypeBuilder<SectionScore> builder)
	{
		builder.HasKey((SectionScore x) => x.Id);
		builder.Property((SectionScore x) => x.SectionName).IsRequired().HasMaxLength(100);
		builder.Property((SectionScore x) => x.Score).IsRequired().HasDefaultValue(0);
		builder.Property((SectionScore x) => x.MaxScore).IsRequired().HasDefaultValue(0);
		builder.Property((SectionScore x) => x.Feedback).IsRequired().HasDefaultValue(string.Empty);
		builder.Property((SectionScore x) => x.IsPassed).IsRequired().HasDefaultValue(false);
		builder.ToTable("SectionScores");
	}
}
