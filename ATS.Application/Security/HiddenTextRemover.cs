using System.Text;

namespace ATS.Application.Security
{
    public static class HiddenTextRemover
    {
        public static HiddenTextRemovalResult Remove(string? text, IEnumerable<string?> hiddenTexts)
        {
            ArgumentNullException.ThrowIfNull(hiddenTexts);

            var currentText = text ?? string.Empty;
            var removedCount = 0;
            var notFoundCount = 0;

            var compactHiddenTexts = hiddenTexts
                .Select(RemoveWhitespace)
                .Where(compactHiddenText => compactHiddenText.Length > 0)
                .OrderByDescending(compactHiddenText => compactHiddenText.Length)
                .ToList();

            foreach (var compactHiddenText in compactHiddenTexts)
            {
                if (TryRemoveFirstOccurrence(currentText, compactHiddenText, out var remainingText))
                {
                    currentText = remainingText;
                    removedCount++;
                }
                else
                {
                    notFoundCount++;
                }
            }

            return new HiddenTextRemovalResult(currentText, removedCount, notFoundCount);
        }

        private static bool TryRemoveFirstOccurrence(string text, string compactHiddenText, out string remainingText)
        {
            var compactTextBuilder = new StringBuilder(text.Length);
            var originalIndexes = new List<int>(text.Length);

            for (var index = 0; index < text.Length; index++)
            {
                if (char.IsWhiteSpace(text[index]))
                    continue;

                compactTextBuilder.Append(text[index]);
                originalIndexes.Add(index);
            }

            var compactText = compactTextBuilder.ToString();
            var searchStart = 0;

            while (searchStart <= compactText.Length - compactHiddenText.Length)
            {
                var matchIndex = compactText.IndexOf(compactHiddenText, searchStart, StringComparison.Ordinal);
                if (matchIndex < 0)
                    break;

                var firstOriginalIndex = originalIndexes[matchIndex];
                var lastOriginalIndex = originalIndexes[matchIndex + compactHiddenText.Length - 1];

                if (IsWordBoundary(text, firstOriginalIndex - 1) && IsWordBoundary(text, lastOriginalIndex + 1))
                {
                    remainingText = text.Remove(firstOriginalIndex, lastOriginalIndex - firstOriginalIndex + 1);
                    return true;
                }

                searchStart = matchIndex + 1;
            }

            remainingText = text;
            return false;
        }

        private static bool IsWordBoundary(string text, int index) =>
            index < 0 || index >= text.Length || !char.IsLetterOrDigit(text[index]);

        private static string RemoveWhitespace(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var builder = new StringBuilder(text.Length);
            foreach (var character in text)
            {
                if (!char.IsWhiteSpace(character))
                    builder.Append(character);
            }

            return builder.ToString();
        }
    }
}
