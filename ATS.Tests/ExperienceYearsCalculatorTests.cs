using ATS.Application.Analyzers.JobMatching;

namespace ATS.Tests
{
    public class ExperienceYearsCalculatorTests
    {
        private static readonly DateTime Today = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

        private static double? Years(string cvText) => ExperienceYearsCalculator.Calculate(cvText, Today);

        [Fact]
        public void Calculate_CountsBothEndMonthsOfARange()
        {
            Assert.Equal(2.0, Years("Developer | January 2020 – December 2021")!.Value, 3);
        }

        [Fact]
        public void Calculate_CountsOverlappingRangesOnce()
        {
            var years = Years(Lines("A | January 2020 – December 2021", "B | June 2021 – December 2022"));

            Assert.Equal(3.0, years!.Value, 3);
        }

        [Fact]
        public void Calculate_UsesTodayForAnOpenEndedRange()
        {
            Assert.Equal(8 / 12.0, Years("Developer | March 2026 – Present")!.Value, 3);
        }

        [Fact]
        public void Calculate_TreatsAYearOnlyRangeAsWholeYears()
        {
            Assert.Equal(3.0, Years("Developer | 2019 – 2021")!.Value, 3);
        }

        [Fact]
        public void Calculate_IgnoresEducationRanges()
        {
            var years = Years(Lines("Developer | 2019 – 2021", "Bachelor of Science — University | 2010 – 2014"));

            Assert.Equal(3.0, years!.Value, 3);
        }

        [Fact]
        public void Calculate_IgnoresCareerBreaks()
        {
            var years = Years(Lines("Career Break — Research | March 2024 – December 2024", "Developer | 2019 – 2021"));

            Assert.Equal(3.0, years!.Value, 3);
        }

        [Fact]
        public void Calculate_DoesNotIgnoreAWorkLineBecauseAWordContainsAnExcludedWord()
        {
            Assert.Equal(2.0, Years("Developer — Singapore | January 2020 – December 2021")!.Value, 3);
        }

        [Fact]
        public void Calculate_ReadsARangeThatContinuesOnTheNextLine()
        {
            Assert.Equal(34 / 12.0, Years("Developer | 2024 –\nPresent")!.Value, 3);
        }

        [Fact]
        public void Calculate_ReturnsTheWorkedYearsOfARealisticCv()
        {
            var years = Years(Lines(
                "Senior .NET Software Developer — OGS Elektronik | July 2026 – September 2026 | Ankara",
                "Senior .NET Developer — Independent / Contract | April 2025 – June 2026 | Ankara",
                "Career Break — Research | March 2024 – December 2024 | Ankara",
                ".NET Developer — Independent / Contract | June 2019 – March 2024 | Santo Domingo",
                ".NET Developer, Independent | June 2018 – March 2019 | Ankara",
                "Microsoft Certified Software Developer Program — Bilge Adam Academy | 2017–2018",
                "Bachelor's Degree — Management Information Systems, Anadolu University | 2026 –",
                "Present"));

            Assert.Equal(86 / 12.0, years!.Value, 3);
        }

        [Fact]
        public void Calculate_FallsBackToTheYearsTheCvStates_WhenThereAreNoDates()
        {
            Assert.Equal(6.0, Years("Backend developer with 6+ years of experience")!.Value, 3);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("No dates here")]
        public void Calculate_ReturnsNull_WhenNothingCanBeRead(string? cvText)
        {
            Assert.Null(ExperienceYearsCalculator.Calculate(cvText, Today));
        }

        private static string Lines(params string[] lines) => string.Join("\n", lines);
    }
}
