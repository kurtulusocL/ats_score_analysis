using ATS.Application.Results;
using ATS.Core.Helpers;
using ATS.Domain.Enums;

namespace ATS.Application.Validation
{
    public static class EvidenceGroundingChecker
    {
        public const int MinimumQuoteCharacters = 3;

        public const string UngroundedExplanation =
            "The evidence quoted by the language model could not be found in the CV, so this requirement is treated as not met.";

        private static readonly char[] QuoteEdgeCharacters = { '"', '\'', '“', '”', '‘', '’', '.', '…', ' ' };

        public static EvidenceGroundingResult Check(IReadOnlyList<RequirementInterpretationResult> interpretations, string? cvText)
        {
            ArgumentNullException.ThrowIfNull(interpretations);

            var comparableCvText = ToComparableText(cvText);
            var checkedInterpretations = new List<RequirementInterpretationResult>(interpretations.Count);
            var downgradedCount = 0;

            foreach (var interpretation in interpretations)
            {
                if (IsSupported(interpretation, comparableCvText))
                {
                    checkedInterpretations.Add(interpretation);
                    continue;
                }

                checkedInterpretations.Add(interpretation with
                {
                    Status = MatchStatus.Missing,
                    EvidenceQuote = null,
                    Explanation = UngroundedExplanation
                });
                downgradedCount++;
            }

            return new EvidenceGroundingResult(checkedInterpretations, downgradedCount);
        }

        private static bool IsSupported(RequirementInterpretationResult interpretation, string comparableCvText)
        {
            if (interpretation.Status == MatchStatus.Missing)
                return true;

            var comparableQuote = ToComparableText(interpretation.EvidenceQuote).Trim(QuoteEdgeCharacters);

            return comparableQuote.Length >= MinimumQuoteCharacters
                   && comparableCvText.Contains(comparableQuote, StringComparison.Ordinal);
        }
        private static string ToComparableText(string? text) => TextWhitespaceHelper.Collapse(text).ToLowerInvariant();
    }
}
