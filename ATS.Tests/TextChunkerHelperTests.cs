using ATS.Core.Helpers;

namespace ATS.Tests
{
    public class TextChunkerHelperTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("  \n \n  ")]
        public void Split_ReturnsEmpty_ForBlankText(string text)
        {
            Assert.Empty(TextChunkerHelper.Split(text));
        }

        [Fact]
        public void Split_MergesShortLinesIntoSingleChunk()
        {
            var chunks = TextChunkerHelper.Split("C#\nSQL\nAzure", 400);

            Assert.Single(chunks);
            Assert.Equal("C#\nSQL\nAzure", chunks[0]);
        }

        [Fact]
        public void Split_NeverExceedsMaxChunkCharacters()
        {
            var text = string.Join("\n", Enumerable.Range(1, 60)
                .Select(i => $"Line number {i} with some descriptive text about experience"));

            var chunks = TextChunkerHelper.Split(text, 200);

            Assert.True(chunks.Count > 1);
            Assert.All(chunks, chunk => Assert.True(chunk.Length <= 200));
        }

        [Fact]
        public void Split_BreaksLongLineAtWordBoundaries()
        {
            var longLine = string.Join(" ", Enumerable.Repeat("developer", 50));

            var chunks = TextChunkerHelper.Split(longLine, 100);

            Assert.True(chunks.Count > 1);
            Assert.All(chunks, chunk => Assert.True(chunk.Length <= 100));
            Assert.All(chunks, chunk => Assert.All(chunk.Split(' '), word => Assert.Equal("developer", word)));
        }

        [Fact]
        public void Split_PreservesAllWordsInOrder()
        {
            var text = string.Join("\n", Enumerable.Range(1, 40).Select(i => $"Item{i} alpha beta gamma"));

            var chunks = TextChunkerHelper.Split(text, 120);

            var originalWords = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            var rebuiltWords = string.Join("\n", chunks).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(originalWords, rebuiltWords);
        }
    }
}
