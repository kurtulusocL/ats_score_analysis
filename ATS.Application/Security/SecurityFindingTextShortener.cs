using ATS.Core.Helpers;

namespace ATS.Application.Security
{
    public static class SecurityFindingTextShortener
    {
        public const int MaximumLength = 100;

        public static string Shorten(string? snippet, string? description)
        {
            var text = string.IsNullOrWhiteSpace(snippet) ? description : snippet;
            var singleLineText = string.Join(' ', (text ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            return TextTruncationHelper.Truncate(singleLineText, MaximumLength);
        }
    }
}
