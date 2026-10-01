using ATS.Application.Analyzers.Base.Models;
using ATS.Application.Analyzers.JobMatching;
using ATS.Tests.TestSupport;

namespace ATS.Tests
{
    public class JobMatchAnalyzerScoringTests
    {
        private static readonly DateTime Today = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        private const string FiveTermPosting = "* Design kubernetes postgresql terraform angular";

        private static AnalyzerResult Analyze(string jobPosting, string cvText, string jobTitle = "") =>
            new JobMatchAnalyzer(jobPosting, jobTitle, new FixedTimeProvider(Today)).Analyze(cvText);

        private static string Lines(params string[] lines) => string.Join("\n", lines);

        [Fact]
        public void Analyze_LimitsTheScoreTo70Percent_EvenWhenEverythingInThePostingAppearsInTheCv()
        {
            var result = Analyze(
                Lines("* Develop REST APIs with kubernetes", "* Write unit tests with postgresql"),
                "I develop REST APIs with kubernetes and write unit tests with postgresql");

            Assert.Equal(14, result.Score);
            Assert.True(result.IsPassed);
            Assert.Contains("2 of 2 requirements met, 0 partially met, 0 not met.", result.Feedbacks[0]);
            Assert.Contains(result.Feedbacks, feedback => feedback.Contains("limited to 70% of its maximum"));
        }

        [Fact]
        public void Analyze_ScoresZeroAndListsTheMissingRequirements_WhenNothingMatches()
        {
            var result = Analyze(
                Lines("* Develop REST APIs with kubernetes", "* Write unit tests with postgresql"),
                "Completely unrelated baking recipes");

            Assert.Equal(0, result.Score);
            Assert.False(result.IsPassed);
            Assert.Contains("0 of 2 requirements met, 0 partially met, 2 not met.", result.Feedbacks[0]);
            Assert.Contains("Develop REST APIs with kubernetes", result.MissingItems);
            Assert.Contains("Write unit tests with postgresql", result.MissingItems);
        }

        [Fact]
        public void Analyze_GivesNoPointsForCvContentThatThePostingDoesNotAskFor()
        {
            var cvText = "design kubernetes postgresql";

            var plain = Analyze(FiveTermPosting, cvText);
            var withExtras = Analyze(FiveTermPosting, cvText + " rabbitmq mongodb graphql kafka docker");

            Assert.Equal(10, plain.Score);
            Assert.Equal(plain.Score, withExtras.Score);
        }

        [Fact]
        public void Analyze_GivesHalfCreditAndNamesTheMissingTerms_WhenARequirementIsPartlyCovered()
        {
            var result = Analyze(FiveTermPosting, "design kubernetes postgresql");

            Assert.Equal(10, result.Score);
            Assert.True(result.IsPassed);
            Assert.Contains("0 of 1 requirements met, 1 partially met, 0 not met.", result.Feedbacks[0]);
            Assert.Contains(result.Suggestions, suggestion => suggestion.Contains("Not found in the CV: terraform, angular."));
            Assert.Empty(result.MissingItems);
        }

        [Fact]
        public void Analyze_GivesNothing_WhenALowShareOfARequirementIsCovered()
        {
            var result = Analyze(FiveTermPosting, "design");

            Assert.Equal(0, result.Score);
            Assert.False(result.IsPassed);
            Assert.Contains("Design kubernetes postgresql terraform angular", result.MissingItems);
        }

        [Fact]
        public void Analyze_TreatsExactlyFourFifthsAsMet()
        {
            Assert.Equal(14, Analyze(FiveTermPosting, "design kubernetes postgresql terraform").Score);
        }

        [Fact]
        public void Analyze_TreatsExactlyTwoFifthsAsPartial()
        {
            Assert.Equal(10, Analyze(FiveTermPosting, "design kubernetes").Score);
        }

        [Fact]
        public void Analyze_WeighsAnOptionalRequirementHalfAsMuchAsAMandatoryOne()
        {
            var posting = Lines("Requirements:", "* Use kubernetes postgresql", "Preferred:", "* Use terraform angular");

            var mandatoryOnly = Analyze(posting, "kubernetes postgresql");
            var optionalOnly = Analyze(posting, "terraform angular");

            Assert.Equal(13, mandatoryOnly.Score);
            Assert.True(mandatoryOnly.IsPassed);
            Assert.Equal(7, optionalOnly.Score);
            Assert.False(optionalOnly.IsPassed);
            Assert.DoesNotContain("limited to", string.Join(" ", mandatoryOnly.Feedbacks));
        }

        [Fact]
        public void Analyze_DoesNotListAMissingOptionalRequirementAsMissing()
        {
            var posting = Lines("Requirements:", "* Use kubernetes postgresql", "Preferred:", "* Use terraform angular");

            Assert.Empty(Analyze(posting, "kubernetes postgresql").MissingItems);
        }

        [Fact]
        public void Analyze_MatchesDifferentFormsOfTheSameWord()
        {
            var result = Analyze("* Develop maintainable software", "We developed and maintained software");

            Assert.Equal(14, result.Score);
        }

        [Fact]
        public void Analyze_CountsTheJobTitleAsAMandatoryRequirement()
        {
            Assert.Equal(10, Analyze("* Develop kubernetes", "develop kubernetes", "Quantum Astronaut").Score);
            Assert.Equal(14, Analyze("* Develop kubernetes", "develop kubernetes quantum astronaut", "Quantum Astronaut").Score);
        }

        [Fact]
        public void Analyze_ReportsHowManyRequirementsWereMetPartiallyMetAndMissed()
        {
            var result = Analyze(Lines("* Develop kubernetes", "* Write terraform"), "develop kubernetes");

            Assert.Equal(10, result.Score);
            Assert.Contains("1 of 2 requirements met, 0 partially met, 1 not met.", result.Feedbacks[0]);
        }

        [Fact]
        public void Analyze_GivesFullCreditForYears_WhenTheCvCoversTheRequiredYears()
        {
            var posting = "* 8+ years of professional experience";

            Assert.Equal(14, Analyze(posting, "Developer | January 2016 – December 2023").Score);
            Assert.Equal(14, Analyze(posting, "Developer | January 2012 – December 2021").Score);
        }

        [Fact]
        public void Analyze_GivesHalfCreditForYears_WhenTheCvCoversAtLeastHalfOfThem()
        {
            Assert.Equal(10, Analyze("* 8+ years of professional experience", "Developer | January 2020 – December 2024").Score);
        }

        [Fact]
        public void Analyze_GivesNothingForYears_WhenTheCvCoversLessThanHalfOrStatesNothing()
        {
            var posting = "* 8+ years of professional experience";

            var tooFew = Analyze(posting, "Developer | January 2023 – December 2024");
            var unknown = Analyze(posting, "Developer without any dates");

            Assert.Equal(0, tooFew.Score);
            Assert.Contains("8+ years of professional experience", tooFew.MissingItems);
            Assert.Equal(0, unknown.Score);
        }

        [Fact]
        public void Analyze_ReturnsASuggestionAndFailsWithoutAJobPosting()
        {
            var result = Analyze("   ", "anything");

            Assert.Equal(0, result.Score);
            Assert.False(result.IsPassed);
            Assert.Contains(result.Suggestions, suggestion => suggestion.Contains("No job posting provided"));
        }

        [Fact]
        public void Analyze_ReturnsASuggestionAndFails_WhenThePostingHasNothingToMatch()
        {
            var result = Analyze("and the of", "anything");

            Assert.Equal(0, result.Score);
            Assert.False(result.IsPassed);
            Assert.Contains(result.Suggestions, suggestion => suggestion.Contains("could not be parsed"));
        }
    }
}
