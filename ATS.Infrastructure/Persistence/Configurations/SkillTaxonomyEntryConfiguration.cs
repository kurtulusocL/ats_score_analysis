using ATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATS.Infrastructure.Persistence.Configurations
{
    public class SkillTaxonomyEntryConfiguration : IEntityTypeConfiguration<SkillTaxonomyEntry>
    {
        public void Configure(EntityTypeBuilder<SkillTaxonomyEntry> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Category).IsRequired().HasMaxLength(50).HasDefaultValue(string.Empty);
            builder.Property(x => x.Description).IsRequired().HasMaxLength(1000).HasDefaultValue(string.Empty);
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

            builder.HasIndex(x => x.Name).IsUnique();
            builder.HasIndex(x => x.Category);

            builder.ToTable("SkillTaxonomyEntries");
        }
    }
}
