

namespace ATS.Application.Reporting
{
    public sealed record RequirementReport(IReadOnlyList<RequirementReportRow> Rows, string? ScoreComponentsText, string? ModelIdentity)
    {
        public static RequirementReport Empty { get; } = new(Array.Empty<RequirementReportRow>(), null, null);

        public bool HasRows => Rows.Count > 0;

        public bool HasInterpretations => Rows.Any(row => row.ModelStatus.HasValue);
    }
}
