using ATS.Core.Helpers;

namespace ATS.Tests
{
    public class FuzzyMatcherHelperTests
    {
        [Theory]
        [InlineData("Experience", "experience")]
        [InlineData("Work Experience", "experience")]
        [InlineData("experiance", "experience")]
        public void IsMatch_ReturnsTrue_ForContainedOrSimilarText(string source, string target)
        {
            Assert.True(FuzzyMatcherHelper.IsMatch(source, target));
        }

        [Fact]
        public void IsMatch_ReturnsFalse_ForUnrelatedText()
        {
            Assert.False(FuzzyMatcherHelper.IsMatch("education", "skills"));
        }

        [Theory]
        [InlineData("", "skills")]
        [InlineData("skills", "")]
        public void IsMatch_ReturnsFalse_ForEmptyInput(string source, string target)
        {
            Assert.False(FuzzyMatcherHelper.IsMatch(source, target));
        }
    }
}
