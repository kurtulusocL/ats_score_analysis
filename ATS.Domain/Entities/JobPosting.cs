using System.Collections.Generic;
using ATS.Domain.Entities.Base;

namespace ATS.Domain.Entities;

public class JobPosting : BaseEntity
{
	public string Title { get; set; } = string.Empty;

	public string RawText { get; set; } = string.Empty;

	public virtual ICollection<CvScan> CvScans { get; set; } = new List<CvScan>();

	public virtual ICollection<JobRequirement> JobRequirements { get; set; } = new List<JobRequirement>();
}
