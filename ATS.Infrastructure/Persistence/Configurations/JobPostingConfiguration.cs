using ATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATS.Infrastructure.Persistence.Configurations;

public class JobPostingConfiguration : IEntityTypeConfiguration<JobPosting>
{
	public void Configure(EntityTypeBuilder<JobPosting> builder)
	{
		builder.HasKey((JobPosting x) => x.Id);
		builder.Property((JobPosting x) => x.Title).IsRequired().HasMaxLength(300);
		builder.Property((JobPosting x) => x.RawText).IsRequired();
		builder.ToTable("JobPostings");
	}
}
