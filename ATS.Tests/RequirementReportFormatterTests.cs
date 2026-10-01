using ATS.Application.Reporting;
using ATS.Domain.Enums;
using System.Globalization;

namespace ATS.Tests
{
    public class RequirementReportFormatterTests
    {
        private static RequirementReportRow FullRow() => new(
            "SQL Server", true, MatchStatus.Met, 0.9, MatchStatus.Partial,
            "Developed APIs with SQL Server", "Basic usage only.", "Describe query tuning.", false);

        private static RequirementReportRow RowWithoutInterpretation() => new(
            "Kubernetes", false, MatchStatus.Missing, 0.1, null, null, null, null, false);

        private static RequirementReport Report(params RequirementReportRow[] rows) =>
            new(rows, "Keyword score 10/20 | Hybrid score 12/20", "Ollama:test-model");

        [Fact]
        public void FormatLines_ReturnsNothing_WhenThereAreNoRows()
        {
            Assert.Empty(RequirementReportFormatter.FormatLines(RequirementReport.Empty));
        }

        [Fact]
        public void FormatLines_WritesTheTitleTheComponentsTheModelNoteAndTheRow()
        {
            var lines = RequirementReportFormatter.FormatLines(Report(FullRow()));

            Assert.Equal(new[]
            {
                "📋 Requirement analysis (1)",
                "   Keyword score 10/20 | Hybrid score 12/20",
                "   The explanations were written by a language model (Ollama:test-model). They do not change the score.",
                "   • SQL Server (mandatory)",
                "       Embedding match: Met (similarity 0.90)",
                "       Language model: Partial",
                "       Explanation: Basic usage only.",
                "       Evidence: \"Developed APIs with SQL Server\"",
                "       Suggestion: Describe query tuning."
            }, lines);
        }

        [Fact]
        public void FormatLines_LeavesOutTheModelLinesAndTheModelNote_WhenNoRequirementWasInterpreted()
        {
            var report = new RequirementReport(new[] { RowWithoutInterpretation() }, null, null);

            var lines = RequirementReportFormatter.FormatLines(report);

            Assert.Equal(new[]
            {
                "📋 Requirement analysis (1)",
                "   • Kubernetes (optional)",
                "       Embedding match: Missing (similarity 0.10)"
            }, lines);
        }

        [Fact]
        public void FormatLines_LeavesOutBlankExplanationEvidenceAndSuggestion()
        {
            var row = new RequirementReportRow("SQL Server", true, MatchStatus.Met, 0.9, MatchStatus.Missing, "   ", "  ", null, false);

            var lines = RequirementReportFormatter.FormatLines(Report(row));

            Assert.DoesNotContain(lines, line => line.Contains("Explanation:") || line.Contains("Evidence:") || line.Contains("Suggestion:"));
        }

        [Fact]
        public void FormatLines_AddsTheDisagreementWarning_OnlyToTheFlaggedRow()
        {
            var flaggedRow = FullRow() with { HasDisagreement = true };

            var lines = RequirementReportFormatter.FormatLines(Report(flaggedRow, RowWithoutInterpretation()));

            Assert.Single(lines, line => line.Contains(RequirementReportTexts.DisagreementNote));
        }

        [Fact]
        public void FormatLines_WritesTheSimilarityWithADecimalPoint_WhateverTheCulture()
        {
            var previousCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

                var lines = RequirementReportFormatter.FormatLines(Report(FullRow()));

                Assert.Contains(lines, line => line.Contains("(similarity 0.90)"));
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
            }
        }
    }
}
