using ATS.Application.Results;
using ATS.Application.Validation;
using ATS.Domain.Enums;

namespace ATS.Tests
{
    public class EvidenceGroundingCheckerTests
    {
        private const string CvText = "Developed REST APIs\nwith   C# and SQL Server.\nManaged a team of five engineers.";

        private static RequirementInterpretationResult Interpretation(MatchStatus status, string? evidenceQuote, string? suggestion = "Add more detail.") =>
            new("Requirement", status, evidenceQuote, "Model explanation.", suggestion);

        private static RequirementInterpretationResult CheckSingle(RequirementInterpretationResult interpretation, string? cvText = CvText) =>
            Assert.Single(EvidenceGroundingChecker.Check(new[] { interpretation }, cvText).Interpretations);

        [Fact]
        public void Check_KeepsAnInterpretationWhoseQuoteAppearsInTheCv()
        {
            var interpretation = Interpretation(MatchStatus.Met, "Managed a team of five engineers");

            var result = EvidenceGroundingChecker.Check(new[] { interpretation }, CvText);

            Assert.Equal(interpretation, Assert.Single(result.Interpretations));
            Assert.Equal(0, result.DowngradedCount);
        }

        [Fact]
        public void Check_IgnoresCaseAndDifferencesInWhitespaceAndLineBreaks()
        {
            var checkedInterpretation = CheckSingle(Interpretation(MatchStatus.Met, "developed rest apis with c# and sql server"));

            Assert.Equal(MatchStatus.Met, checkedInterpretation.Status);
        }

        [Fact]
        public void Check_IgnoresQuotationMarksAndEllipsesAroundTheQuote()
        {
            var checkedInterpretation = CheckSingle(Interpretation(MatchStatus.Partial, "\"…Managed a team of five engineers…\""));

            Assert.Equal(MatchStatus.Partial, checkedInterpretation.Status);
        }

        [Theory]
        [InlineData(MatchStatus.Met)]
        [InlineData(MatchStatus.Partial)]
        public void Check_TreatsAnInterpretationAsMissing_WhenTheQuoteIsNotInTheCv(MatchStatus status)
        {
            var result = EvidenceGroundingChecker.Check(new[] { Interpretation(status, "Led a team of fifty engineers") }, CvText);

            var checkedInterpretation = Assert.Single(result.Interpretations);
            Assert.Equal(MatchStatus.Missing, checkedInterpretation.Status);
            Assert.Null(checkedInterpretation.EvidenceQuote);
            Assert.Equal(EvidenceGroundingChecker.UngroundedExplanation, checkedInterpretation.Explanation);
            Assert.Equal("Add more detail.", checkedInterpretation.Suggestion);
            Assert.Equal("Requirement", checkedInterpretation.RequirementName);
            Assert.Equal(1, result.DowngradedCount);
        }

        [Fact]
        public void Check_LeavesAMissingInterpretationUntouched_AndDoesNotCountIt()
        {
            var interpretation = Interpretation(MatchStatus.Missing, null);

            var result = EvidenceGroundingChecker.Check(new[] { interpretation }, CvText);

            Assert.Equal(interpretation, Assert.Single(result.Interpretations));
            Assert.Equal(0, result.DowngradedCount);
        }

        [Fact]
        public void Check_DowngradesAQuoteShorterThanTheMinimum_EvenWhenTheCvContainsIt()
        {
            var tooShort = new string('s', EvidenceGroundingChecker.MinimumQuoteCharacters - 1);
            var checkedInterpretation = CheckSingle(Interpretation(MatchStatus.Met, tooShort), cvText: "has " + tooShort + " inside");

            Assert.Equal(MatchStatus.Missing, checkedInterpretation.Status);
        }

        [Fact]
        public void Check_KeepsAQuoteOfExactlyTheMinimumLength()
        {
            var shortestQuote = new string('s', EvidenceGroundingChecker.MinimumQuoteCharacters);
            var checkedInterpretation = CheckSingle(Interpretation(MatchStatus.Met, shortestQuote), cvText: "has " + shortestQuote + " inside");

            Assert.Equal(MatchStatus.Met, checkedInterpretation.Status);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        public void Check_DowngradesEveryClaim_WhenTheCvTextIsBlank(string? cvText)
        {
            var checkedInterpretation = CheckSingle(Interpretation(MatchStatus.Met, "Managed a team of five engineers"), cvText);

            Assert.Equal(MatchStatus.Missing, checkedInterpretation.Status);
        }

        [Fact]
        public void Check_KeepsTheOrderAndCountsOnlyTheDowngradedInterpretations()
        {
            var interpretations = new[]
            {
                Interpretation(MatchStatus.Met, "Managed a team of five engineers"),
                Interpretation(MatchStatus.Met, "an invented achievement"),
                Interpretation(MatchStatus.Missing, null),
                Interpretation(MatchStatus.Partial, "Developed REST APIs")
            };

            var result = EvidenceGroundingChecker.Check(interpretations, CvText);

            Assert.Equal(
                new[] { MatchStatus.Met, MatchStatus.Missing, MatchStatus.Missing, MatchStatus.Partial },
                result.Interpretations.Select(interpretation => interpretation.Status));
            Assert.Equal(1, result.DowngradedCount);
        }
    }
}
