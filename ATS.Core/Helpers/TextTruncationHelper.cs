

namespace ATS.Core.Helpers
{
    public static class TextTruncationHelper
    {
        private const string Ellipsis = "...";

        public static string Truncate(string? text, int maximumLength)
        {
            if (maximumLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(maximumLength), "The maximum length must be greater than zero.");

            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (text.Length <= maximumLength)
                return text;

            var suffix = maximumLength > Ellipsis.Length ? Ellipsis : string.Empty;
            var cutLength = maximumLength - suffix.Length;

            if (cutLength > 0 && char.IsHighSurrogate(text[cutLength - 1]))
                cutLength--;

            return text.Substring(0, cutLength) + suffix;
        }
    }
}
