using ATS.Core.Helpers;

namespace ATS.Tests
{
    public class TextTruncationHelperTests
    {
        [Fact]
        public void Truncate_ReturnsEmptyString_WhenTextIsNull()
        {
            Assert.Equal(string.Empty, TextTruncationHelper.Truncate(null, 10));
        }

        [Fact]
        public void Truncate_ReturnsSameText_WhenTextLengthEqualsMaximumLength()
        {
            var text = new string('a', 10);

            Assert.Equal(text, TextTruncationHelper.Truncate(text, 10));
        }

        [Fact]
        public void Truncate_ReturnsExactlyMaximumLengthEndingWithEllipsis_WhenTextIsTooLong()
        {
            var result = TextTruncationHelper.Truncate(new string('a', 50), 10);

            Assert.Equal(10, result.Length);
            Assert.Equal("aaaaaaa...", result);
        }

        [Fact]
        public void Truncate_DoesNotSplitSurrogatePair()
        {
            var result = TextTruncationHelper.Truncate("abcdef😀ghijkl", 10);

            Assert.Equal("abcdef...", result);
            Assert.DoesNotContain(result, character => char.IsSurrogate(character));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Truncate_Throws_WhenMaximumLengthIsNotPositive(int maximumLength)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TextTruncationHelper.Truncate("text", maximumLength));
        }

        [Fact]
        public void Truncate_OmitsEllipsis_WhenMaximumLengthIsTooSmallForIt()
        {
            Assert.Equal("ab", TextTruncationHelper.Truncate("abcdef", 2));
        }
    }
}
