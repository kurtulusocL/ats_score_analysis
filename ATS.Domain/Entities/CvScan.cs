using ATS.Domain.Entities.Base;

namespace ATS.Domain.Entities
{
    public class CvScan:BaseEntity
    {
        public string CandidateName { get; set; } = string.Empty;
        public string RawText { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty; 
        public string FilePath { get; set; } = string.Empty;
        public int OverallScore { get; set; }
        public bool IsJobMatched { get; set; }

        public int? JobPostingId { get; set; }
        public virtual JobPosting? JobPosting { get; set; }

        public virtual ICollection<SectionScore> SectionScores { get; set; } = new List<SectionScore>();
        public virtual ICollection<ScoreReport> ScoreReports { get; set; } = new List<ScoreReport>();        
    }
}
