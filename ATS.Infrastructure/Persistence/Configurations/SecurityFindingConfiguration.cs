using ATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATS.Infrastructure.Persistence.Configurations
{
    public class SecurityFindingConfiguration : IEntityTypeConfiguration<SecurityFinding>
    {
        public void Configure(EntityTypeBuilder<SecurityFinding> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type).IsRequired().HasConversion<string>();
            builder.Property(x => x.Severity).IsRequired().HasConversion<string>();
            builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
            builder.Property(x => x.Snippet).IsRequired();

            builder.HasOne(x => x.CvScan)
                   .WithMany(x => x.SecurityFindings)
                   .HasForeignKey(x => x.CvScanId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("SecurityFindings");
        }
    }
}
