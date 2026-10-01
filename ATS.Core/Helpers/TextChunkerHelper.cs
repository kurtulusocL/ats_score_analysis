using System.Text;

namespace ATS.Core.Helpers
{
    public static class TextChunkerHelper
    {
        public static IReadOnlyList<string> Split(string text, int maxChunkCharacters = 400)
        {
            if (maxChunkCharacters <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxChunkCharacters));

            var chunks = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return chunks;

            var currentChunk = new StringBuilder();

            foreach (var rawLine in text.Split('\n'))
            {
                var line = rawLine.Trim();
                if (line.Length == 0) continue;

                foreach (var piece in SplitLongLine(line, maxChunkCharacters))
                {
                    if (currentChunk.Length > 0 && currentChunk.Length + 1 + piece.Length > maxChunkCharacters)
                    {
                        chunks.Add(currentChunk.ToString());
                        currentChunk.Clear();
                    }

                    if (currentChunk.Length > 0) currentChunk.Append('\n');
                    currentChunk.Append(piece);
                }
            }

            if (currentChunk.Length > 0) chunks.Add(currentChunk.ToString());
            return chunks;
        }

        private static IEnumerable<string> SplitLongLine(string line, int maxCharacters)
        {
            if (line.Length <= maxCharacters)
            {
                yield return line;
                yield break;
            }

            var piece = new StringBuilder();

            foreach (var word in line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var remaining = word;

                while (remaining.Length > maxCharacters)
                {
                    if (piece.Length > 0)
                    {
                        yield return piece.ToString();
                        piece.Clear();
                    }
                    yield return remaining[..maxCharacters];
                    remaining = remaining[maxCharacters..];
                }

                if (piece.Length > 0 && piece.Length + 1 + remaining.Length > maxCharacters)
                {
                    yield return piece.ToString();
                    piece.Clear();
                }

                if (piece.Length > 0) piece.Append(' ');
                piece.Append(remaining);
            }

            if (piece.Length > 0) yield return piece.ToString();
        }
    }
}
