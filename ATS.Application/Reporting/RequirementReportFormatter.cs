

namespace ATS.Application.Reporting
{
    public static class RequirementReportFormatter
    {
        public static IReadOnlyList<string> FormatLines(RequirementReport requirementReport)
        {
            ArgumentNullException.ThrowIfNull(requirementReport);

            if (!requirementReport.HasRows)
                return Array.Empty<string>();

            var lines = new List<string> { $"📋 {RequirementReportTexts.SectionTitle} ({requirementReport.Rows.Count})" };

            if (requirementReport.ScoreComponentsText != null)
                lines.Add("   " + requirementReport.ScoreComponentsText);

            if (requirementReport.HasInterpretations)
                lines.Add("   " + RequirementReportTexts.BuildModelNote(requirementReport.ModelIdentity));

            foreach (var row in requirementReport.Rows)
                lines.AddRange(FormatRow(row));

            return lines;
        }

        private static IEnumerable<string> FormatRow(RequirementReportRow row)
        {
            yield return $"   • {row.RequirementName} ({row.PriorityText})";
            yield return $"       {RequirementReportTexts.SemanticLabel}: {row.SemanticStatus} (similarity {row.SimilarityText})";

            if (row.ModelStatus.HasValue)
                yield return $"       {RequirementReportTexts.ModelLabel}: {row.ModelStatus.Value}";

            if (!string.IsNullOrWhiteSpace(row.Explanation))
                yield return $"       {RequirementReportTexts.ExplanationLabel}: {row.Explanation}";

            if (!string.IsNullOrWhiteSpace(row.EvidenceQuote))
                yield return $"       {RequirementReportTexts.EvidenceLabel}: \"{row.EvidenceQuote}\"";

            if (!string.IsNullOrWhiteSpace(row.Suggestion))
                yield return $"       {RequirementReportTexts.SuggestionLabel}: {row.Suggestion}";

            if (row.HasDisagreement)
                yield return $"       ⚠ {RequirementReportTexts.DisagreementNote}";
        }
    }
}
