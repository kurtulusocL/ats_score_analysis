using ATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATS.Infrastructure.Persistence.Configurations
{
    public class RequirementMatchConfiguration : IEntityTypeConfiguration<RequirementMatch>
    {
        public void Configure(EntityTypeBuilder<RequirementMatch> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Similarity).IsRequired();
            builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Evidence).HasMaxLength(2000);
            builder.HasIndex(x => new { x.CvScanId, x.JobRequirementId }).IsUnique();
            builder.HasOne(x => x.CvScan).WithMany(x => x.RequirementMatches).HasForeignKey(x => x.CvScanId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.JobRequirement).WithMany(x => x.RequirementMatches).HasForeignKey(x => x.JobRequirementId).OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("RequirementMatches");
        }
    }
}
