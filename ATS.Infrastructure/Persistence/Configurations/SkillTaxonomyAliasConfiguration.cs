using ATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATS.Infrastructure.Persistence.Configurations
{
    public class SkillTaxonomyAliasConfiguration : IEntityTypeConfiguration<SkillTaxonomyAlias>
    {
        public void Configure(EntityTypeBuilder<SkillTaxonomyAlias> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Alias).IsRequired().HasMaxLength(100);

            builder.HasIndex(x => new { x.SkillTaxonomyEntryId, x.Alias }).IsUnique();

            builder.HasOne(x => x.SkillTaxonomyEntry)
                   .WithMany(x => x.SkillTaxonomyAliases)
                   .HasForeignKey(x => x.SkillTaxonomyEntryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("SkillTaxonomyAliases");
        }
    }
}
