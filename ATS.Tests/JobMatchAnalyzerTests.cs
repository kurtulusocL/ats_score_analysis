using ATS.Application.Analyzers.JobMatching;

namespace ATS.Tests
{
    public class JobMatchAnalyzerTests
    {
        [Fact]
        public void Analyze_MatchesShortTechTerms()
        {
            const string text = "SQL C# .NET AWS Git";
            var analyzer = new JobMatchAnalyzer(text);

            var result = analyzer.Analyze(text);

            Assert.Equal(14, result.Score);
            Assert.True(result.IsPassed);
        }

        [Fact]
        public void Analyze_ReportsTheMissingShortTechTerms_WhenARequirementIsPartlyCovered()
        {
            var analyzer = new JobMatchAnalyzer("SQL C# .NET AWS Git");

            var result = analyzer.Analyze("Developer with SQL and Git knowledge");

            Assert.Contains(result.Suggestions, suggestion => suggestion.Contains("Not found in the CV: c#, net, aws."));
            Assert.Empty(result.MissingItems);
        }

        [Fact]
        public void Analyze_ReportsTheWholeRequirementAsMissing_WhenNoneOfTheShortTechTermsAppear()
        {
            var analyzer = new JobMatchAnalyzer("SQL C# .NET AWS Git");

            var result = analyzer.Analyze("Developer with Excel and Word knowledge");

            Assert.Equal(0, result.Score);
            Assert.Contains("SQL C# .NET AWS Git", result.MissingItems);
        }
    }
}
