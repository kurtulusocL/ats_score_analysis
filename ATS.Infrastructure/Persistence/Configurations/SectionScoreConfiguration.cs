using ATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATS.Infrastructure.Persistence.Configurations
{
    public class SectionScoreConfiguration : IEntityTypeConfiguration<SectionScore>
    {
        public void Configure(EntityTypeBuilder<SectionScore> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SectionName).IsRequired().HasMaxLength(100);

            builder.Property(x => x.Score).IsRequired().HasDefaultValue(0);

            builder.Property(x => x.MaxScore).IsRequired().HasDefaultValue(0);

            builder.Property(x => x.Feedback).HasMaxLength(1000);

            builder.Property(x => x.IsPassed).IsRequired().HasDefaultValue(false);

            builder.ToTable("SectionScores");
        }
    }
}
