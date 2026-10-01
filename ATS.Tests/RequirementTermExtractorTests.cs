using ATS.Application.Analyzers.JobMatching;

namespace ATS.Tests
{
    public class RequirementTermExtractorTests
    {
        private static List<string> Displays(string text) =>
            RequirementTermExtractor.ExtractRequirementTerms(text).Select(term => term.Display).ToList();

        [Fact]
        public void ExtractRequirementTerms_KeepsTechnicalTokensWhole()
        {
            Assert.Equal(new[] { "c#", "net", "c++", "ci/cd", "asp.net" }, Displays("C#, .NET, C++, CI/CD and ASP.NET"));
        }

        [Fact]
        public void ExtractRequirementTerms_SplitsHyphenatedAndSlashSeparatedWords()
        {
            Assert.Equal(new[] { "problem", "solving", "skills" }, Displays("problem-solving skills"));
            Assert.Equal(new[] { "azure", "devops", "tfs" }, Displays("Azure DevOps/TFS"));
        }

        [Fact]
        public void ExtractRequirementTerms_DropsFillerWordsAndNumbers()
        {
            Assert.Empty(Displays("Strong experience with the"));
            Assert.Equal(new[] { "years", "since" }, Displays("8+ years since 2019"));
        }

        [Fact]
        public void ExtractRequirementTerms_ReturnsEachStemOnce_WithTheFirstSpelling()
        {
            var terms = RequirementTermExtractor.ExtractRequirementTerms("develop developer developed");

            var term = Assert.Single(terms);
            Assert.Equal("develop", term.Display);
            Assert.Equal("develop", term.Stem);
        }

        [Theory]
        [InlineData("develops", "develop")]
        [InlineData("developer", "develop")]
        [InlineData("developed", "develop")]
        [InlineData("developing", "develop")]
        [InlineData("development", "develop")]
        [InlineData("technologies", "technology")]
        [InlineData("services", "service")]
        [InlineData("maintainable", "maintainability")]
        [InlineData("manage", "management")]
        [InlineData("manager", "managed")]
        [InlineData("processes", "process")]
        [InlineData("queries", "query")]
        [InlineData("apis", "api")]
        [InlineData("reusable", "reuse")]
        public void Stem_MapsWordFormsOfTheSameWordToTheSameStem(string first, string second)
        {
            Assert.Equal(RequirementTermExtractor.Stem(second), RequirementTermExtractor.Stem(first));
        }

        [Theory]
        [InlineData("java", "javascript")]
        [InlineData("sales", "salesforce")]
        [InlineData("analysis", "analyst")]
        public void Stem_KeepsDifferentWordsApart(string first, string second)
        {
            Assert.False(RequirementTermExtractor.Stem(first) == RequirementTermExtractor.Stem(second));
        }

        [Fact]
        public void ExtractCvStems_AlsoIndexesThePartsOfDottedWords()
        {
            var stems = RequirementTermExtractor.ExtractCvStems("Built with ASP.NET and github.com/kurtulusocl");

            Assert.Contains("asp.net", stems);
            Assert.Contains("asp", stems);
            Assert.Contains("net", stems);
            Assert.Contains("github", stems);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Extract_ReturnsNothing_ForBlankText(string? text)
        {
            Assert.Empty(RequirementTermExtractor.ExtractRequirementTerms(text));
            Assert.Empty(RequirementTermExtractor.ExtractCvStems(text));
        }
    }
}
