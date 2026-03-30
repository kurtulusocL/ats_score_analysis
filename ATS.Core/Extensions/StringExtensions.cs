using System.Text.RegularExpressions;

namespace ATS.Core.Extensions
{
    public static class StringExtensions
    {
        public static string ToLower(this string text)
            => text.ToLowerInvariant();

        public static bool ContainsIgnoreCase(this string text, string value)
            => text.Contains(value, StringComparison.OrdinalIgnoreCase);

        public static bool IsNullOrWhiteSpace(this string? text)
            => string.IsNullOrWhiteSpace(text);

        public static string[] SplitWords(this string text)
            => text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        public static string[] SplitLines(this string text)
            => text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        public static HashSet<string> Tokenize(this string text, HashSet<string> stopWords)
        {
            return new HashSet<string>(Regex.Split(text.ToLowerInvariant(), @"\W+").Where(w => w.Length > 3).Except(stopWords));
        }

        public static string ExtractSection(this string text, IEnumerable<string> startHeaders, IEnumerable<string>? endHeaders = null)
        {
            var startIndex = startHeaders.Select(h => text.IndexOf(h, StringComparison.OrdinalIgnoreCase)).Where(i => i >= 0).DefaultIfEmpty(-1).Min();
            if (startIndex < 0) return string.Empty;
            var endIndex = text.Length;

            if (endHeaders != null)
            {
                var end = endHeaders.Select(h => text.IndexOf(h, startIndex + 1, StringComparison.OrdinalIgnoreCase)).Where(i => i > startIndex).DefaultIfEmpty(text.Length).Min();
                endIndex = end;
            }
            return text.Substring(startIndex, endIndex - startIndex);
        }

        public static string TruncateAt(this string text, int maxLength)
            => text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";

        public static int WordCount(this string text)
            => text.SplitWords().Length;

        public static bool HasMinimumWordCount(this string text, int min)
            => text.WordCount() >= min;
    }
}
