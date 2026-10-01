using ATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATS.Infrastructure.Persistence.Configurations
{
    public class EmbeddingCacheEntryConfiguration : IEntityTypeConfiguration<EmbeddingCacheEntry>
    {
        public void Configure(EntityTypeBuilder<EmbeddingCacheEntry> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TextHash).IsRequired().HasMaxLength(64).IsFixedLength();
            builder.Property(x => x.Model).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Dimensions).IsRequired();
            builder.Property(x => x.Vector).IsRequired();
            builder.HasIndex(x => new { x.TextHash, x.Model }).IsUnique();

            builder.ToTable("EmbeddingCacheEntries");
        }
    }
}
