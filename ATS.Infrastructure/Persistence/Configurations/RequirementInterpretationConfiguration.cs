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
    public class RequirementInterpretationConfiguration : IEntityTypeConfiguration<RequirementInterpretation>
    {
        public void Configure(EntityTypeBuilder<RequirementInterpretation> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.EvidenceQuote).HasMaxLength(2000);
            builder.Property(x => x.Explanation).IsRequired().HasMaxLength(1000);
            builder.Property(x => x.Suggestion).HasMaxLength(1000);
            builder.Property(x => x.ModelIdentity).IsRequired().HasMaxLength(100);
            builder.HasOne(x => x.RequirementMatch).WithOne(x => x.RequirementInterpretation).HasForeignKey<RequirementInterpretation>(x => x.RequirementMatchId).OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("RequirementInterpretations");
        }
    }
}
