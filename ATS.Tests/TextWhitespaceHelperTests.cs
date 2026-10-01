using ATS.Core.Helpers;

namespace ATS.Tests
{
    public class TextWhitespaceHelperTests
    {
        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("   \n\t ", "")]
        [InlineData("  one   two  ", "one two")]
        [InlineData("line one\r\nline two\tthree", "line one line two three")]
        public void Collapse_TurnsEveryWhitespaceRunIntoASingleSpace_AndTrimsTheEdges(string? text, string expected)
        {
            Assert.Equal(expected, TextWhitespaceHelper.Collapse(text));
        }
    }
}
