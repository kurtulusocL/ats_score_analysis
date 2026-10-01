

namespace ATS.Core.Helpers
{
    public static class TextWhitespaceHelper
    {
        public static string Collapse(string? text) =>
           string.Join(' ', (text ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
