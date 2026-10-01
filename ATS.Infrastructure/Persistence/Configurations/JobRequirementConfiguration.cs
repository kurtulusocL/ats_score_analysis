using ATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATS.Infrastructure.Persistence.Configurations
{
    public class JobRequirementConfiguration : IEntityTypeConfiguration<JobRequirement>
    {
        public void Configure(EntityTypeBuilder<JobRequirement> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Category).IsRequired().HasMaxLength(50).HasDefaultValue(string.Empty);
            builder.Property(x => x.IsMandatory).IsRequired().HasDefaultValue(true);
            builder.HasOne(x => x.JobPosting).WithMany(x => x.JobRequirements).HasForeignKey(x => x.JobPostingId).OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("JobRequirements");
        }
    }
}
