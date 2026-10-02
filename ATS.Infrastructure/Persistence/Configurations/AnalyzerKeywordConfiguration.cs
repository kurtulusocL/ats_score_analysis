using ATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATS.Infrastructure.Persistence.Configurations;

public class AnalyzerKeywordConfiguration : IEntityTypeConfiguration<AnalyzerKeyword>
{
	public void Configure(EntityTypeBuilder<AnalyzerKeyword> builder)
	{
		builder.HasKey((AnalyzerKeyword x) => x.Id);
		builder.Property((AnalyzerKeyword x) => x.Word).IsRequired().HasMaxLength(100);
		builder.Property((AnalyzerKeyword x) => x.Category).IsRequired().HasMaxLength(50);
		builder.Property((AnalyzerKeyword x) => x.IsActive).IsRequired().HasDefaultValue(true);
		builder.HasIndex((AnalyzerKeyword x) => x.Category);
		builder.HasIndex((AnalyzerKeyword x) => new { x.Word, x.Category }).IsUnique();
		builder.Property((AnalyzerKeyword x) => x.SubCategory).HasMaxLength(50).HasDefaultValue(string.Empty);
		builder.ToTable("AnalyzerKeywords");
	}
}
