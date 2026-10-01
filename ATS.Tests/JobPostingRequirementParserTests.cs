using ATS.Application.Analyzers.JobMatching;

namespace ATS.Tests
{
    public class JobPostingRequirementParserTests
    {
        private static string Lines(params string[] lines) => string.Join("\n", lines);

        [Fact]
        public void Parse_StripsBulletMarkers()
        {
            var requirements = JobPostingRequirementParser.Parse(Lines("* Develop APIs", "- Write tests", "1. Design systems", "• Ship features"));

            Assert.Equal(new[] { "Develop APIs", "Write tests", "Design systems", "Ship features" }, requirements.Select(requirement => requirement.Text));
        }

        [Fact]
        public void Parse_SkipsHeadingsThatEndWithAColon()
        {
            var requirements = JobPostingRequirementParser.Parse(Lines("Requirements:", "* Develop APIs"));

            Assert.Equal("Develop APIs", Assert.Single(requirements).Text);
        }

        [Fact]
        public void Parse_MarksItemsUnderAPreferredHeadingAsOptional_UntilTheNextHeading()
        {
            var requirements = JobPostingRequirementParser.Parse(Lines(
                "Qualifications:", "* Develop APIs", "Preferred:", "* Write tests", "Responsibilities:", "* Ship features"));

            Assert.Equal(new[] { true, false, true }, requirements.Select(requirement => requirement.IsMandatory));
        }

        [Fact]
        public void Parse_ReadsTheRequiredYears_AndDoesNotMangleALeadingNumber()
        {
            var requirements = JobPostingRequirementParser.Parse(Lines(
                "* 8+ years of professional experience", "* 5-7 years in sales", "3 years of support"));

            Assert.Equal(new int?[] { 8, 5, 3 }, requirements.Select(requirement => requirement.RequiredYears));
            Assert.Equal("8+ years of professional experience", requirements[0].Text);
            Assert.All(requirements, requirement => Assert.Empty(requirement.Terms));
        }

        [Fact]
        public void Parse_SplitsAPlainParagraphIntoSentences()
        {
            var requirements = JobPostingRequirementParser.Parse("Build APIs. Write tests.");

            Assert.Equal(new[] { "Build APIs.", "Write tests." }, requirements.Select(requirement => requirement.Text));
        }

        [Fact]
        public void Parse_KeepsABulletAsASingleRequirement()
        {
            var requirements = JobPostingRequirementParser.Parse("* Use Git. Use Docker.");

            Assert.Equal("Use Git. Use Docker.", Assert.Single(requirements).Text);
        }

        [Fact]
        public void Parse_HandlesWindowsLineEndings()
        {
            var requirements = JobPostingRequirementParser.Parse("* Develop APIs\r\n* Write tests");

            Assert.Equal(new[] { "Develop APIs", "Write tests" }, requirements.Select(requirement => requirement.Text));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   \n  ")]
        [InlineData("* and the of")]
        public void Parse_ReturnsNothing_WhenThereIsNothingToMatch(string? jobPostingText)
        {
            Assert.Empty(JobPostingRequirementParser.Parse(jobPostingText));
        }

        [Fact]
        public void Parse_SplitsNumberedItemsThatShareALine_WithAndWithoutASpaceAfterThePeriod()
        {
            var requirements = JobPostingRequirementParser.Parse(Lines("9. Build APIs with C#.10. Write unit tests. 11. Review code"));

            Assert.Equal(new[] { "Build APIs with C#.", "Write unit tests.", "Review code" }, requirements.Select(requirement => requirement.Text));
        }

        [Fact]
        public void Parse_KeepsEachSplitItemAsASingleRequirement()
        {
            var requirements = JobPostingRequirementParser.Parse("* Use Git. Use Docker.2. Use Linux. Use Bash.");

            Assert.Equal(new[] { "Use Git. Use Docker.", "Use Linux. Use Bash." }, requirements.Select(requirement => requirement.Text));
        }

        [Fact]
        public void Parse_MarksEverySplitItemAsOptional_WhenTheyAreUnderAPreferredHeading()
        {
            var requirements = JobPostingRequirementParser.Parse(Lines("Preferred:", "17. Know Python.18. Know Rust."));

            Assert.Equal(2, requirements.Count);
            Assert.All(requirements, requirement => Assert.False(requirement.IsMandatory));
        }

        [Fact]
        public void Parse_ReadsTheRequiredYearsOfEachSplitItemSeparately()
        {
            var requirements = JobPostingRequirementParser.Parse("5. Needs 8+ years of C#.6. Know SQL");

            Assert.Equal(new int?[] { 8, null }, requirements.Select(requirement => requirement.RequiredYears));
        }

        [Fact]
        public void Parse_DoesNotSplitVersionNumbersInsideAnItem()
        {
            var requirements = JobPostingRequirementParser.Parse("* Experience with OAuth 2.0 and Python 3.10 is required");

            Assert.Equal("Experience with OAuth 2.0 and Python 3.10 is required", Assert.Single(requirements).Text);
        }

        [Fact]
        public void Parse_DoesNotTreatAVersionNumberAtTheEndOfASentenceAsTheStartOfANumberedItem()
        {
            var requirements = JobPostingRequirementParser.Parse("Experience with Python 3.10. Knowledge of SQL.");

            Assert.Equal(new[] { "Experience with Python 3.10.", "Knowledge of SQL." }, requirements.Select(requirement => requirement.Text));
        }
    }
}
