using ATS.Core.Constants;
using ATS.Core.Extensions;

namespace ATS.Tests
{
    public class CoreExtensionsTests
    {
        [Fact]
        public void ExtractCandidateName_SkipsContactLines_ReturnsName()
        {
            var cv = "John Smith\njohn@mail.com\n+90 555 123 4567";

            Assert.Equal("John Smith", cv.ExtractCandidateName());
        }

        [Fact]
        public void ExtractCandidateName_ReturnsUnknown_WhenNoNameLine()
        {
            var cv = "john@mail.com\nsoftware developer with 5 years of experience";

            Assert.Equal("Unknown", cv.ExtractCandidateName());
        }
       
        [Fact]
        public void Tokenize_DropsShortTechTerms_CurrentBehavior()
        {
            var tokens = "SQL C# AWS Git".Tokenize(StopWords.Default);

            Assert.Empty(tokens);
        }

        [Fact]
        public void TokenizeTerms_KeepsShortTechTerms()
        {
            var tokens = "SQL C# AWS Git".TokenizeTerms(StopWords.Terms);

            Assert.Contains("sql", tokens);
            Assert.Contains("c#", tokens);
            Assert.Contains("aws", tokens);
            Assert.Contains("git", tokens);
        }

        [Fact]
        public void TokenizeTerms_KeepsCompoundTerms()
        {
            var tokens = ".NET ASP.NET C++ CI/CD Node.js".TokenizeTerms(StopWords.Terms);

            Assert.Contains(".net", tokens);
            Assert.Contains("asp.net", tokens);
            Assert.Contains("c++", tokens);
            Assert.Contains("ci/cd", tokens);
            Assert.Contains("node.js", tokens);
        }

        [Fact]
        public void TokenizeTerms_DropsFunctionWordsNumbersAndTrailingPunctuation()
        {
            var tokens = "We are hiring for the SQL role. 5+ 2020".TokenizeTerms(StopWords.Terms);

            Assert.Contains("sql", tokens);
            Assert.DoesNotContain("the", tokens);
            Assert.DoesNotContain("for", tokens);
            Assert.DoesNotContain("sql.", tokens);
            Assert.DoesNotContain("5+", tokens);
            Assert.DoesNotContain("2020", tokens);
        }
    }
}
