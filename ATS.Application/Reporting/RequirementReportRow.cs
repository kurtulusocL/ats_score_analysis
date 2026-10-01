using ATS.Domain.Enums;
using System.Globalization;

namespace ATS.Application.Reporting
{
    public sealed record RequirementReportRow(
        string RequirementName,
        bool IsMandatory,
        MatchStatus SemanticStatus,
        double Similarity,
        MatchStatus? ModelStatus,
        string? EvidenceQuote,
        string? Explanation,
        string? Suggestion,
        bool HasDisagreement)
        {
            public string SimilarityText => Similarity.ToString("0.00", CultureInfo.InvariantCulture);
            public string PriorityText => IsMandatory ? "mandatory" : "optional";
        }
}
