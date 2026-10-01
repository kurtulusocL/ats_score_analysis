using ATS.Application.Prompts;

namespace ATS.Tests
{
    public class RandomPromptDelimiterProviderTests
    {
        [Fact]
        public void CreateToken_ReturnsThirtyTwoHexadecimalCharacters()
        {
            var token = new RandomPromptDelimiterProvider().CreateToken();

            Assert.Matches("^[0-9A-F]{32}$", token);
        }

        [Fact]
        public void CreateToken_ReturnsADifferentValueOnEveryCall()
        {
            var provider = new RandomPromptDelimiterProvider();

            var tokens = Enumerable.Range(0, 100).Select(_ => provider.CreateToken()).ToList();

            Assert.Equal(100, tokens.Distinct().Count());
        }
    }
}
