using ATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATS.Infrastructure.Persistence.Configurations
{
    public class AnalyzerKeywordConfiguration : IEntityTypeConfiguration<AnalyzerKeyword>
    {
        public void Configure(EntityTypeBuilder<AnalyzerKeyword> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Word).IsRequired().HasMaxLength(100);

            builder.Property(x => x.Category).IsRequired().HasMaxLength(50);

            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

            builder.HasIndex(x => x.Category);
            builder.HasIndex(x => new { x.Word, x.Category }).IsUnique();

            builder.Property(x => x.SubCategory).HasMaxLength(50).HasDefaultValue(string.Empty);

            builder.ToTable("AnalyzerKeywords");
        }
    }
}
