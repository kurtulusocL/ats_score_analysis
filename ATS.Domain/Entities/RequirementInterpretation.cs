using ATS.Domain.Entities.Base;
using ATS.Domain.Enums;

namespace ATS.Domain.Entities
{
    public class RequirementInterpretation:BaseEntity
    {
        public MatchStatus Status { get; set; }
        public string? EvidenceQuote { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public string? Suggestion { get; set; }
        public string ModelIdentity { get; set; } = string.Empty;

        public int RequirementMatchId { get; set; }
        public virtual RequirementMatch RequirementMatch { get; set; } = null!;
    }
}
