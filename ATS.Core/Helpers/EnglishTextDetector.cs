using System.Text.RegularExpressions;

namespace ATS.Core.Helpers
{
    public static class EnglishTextDetector
    {
        private const int MinimumWordCount = 30;
        private const double MinimumEnglishFunctionWordRatio = 0.12;
        private const double MaximumTurkishFunctionWordRatio = 0.01;
        private const double MaximumTurkishLetterRatio = 0.02;
        private const string TurkishSpecificLetters = "çğıöşüÇĞİÖŞÜ";
        private static readonly Regex WordRegex = new(@"\p{L}+", RegexOptions.Compiled);

        private static readonly HashSet<string> EnglishFunctionWords = new(StringComparer.Ordinal)
        {
            "the", "and", "of", "to", "in", "is", "with", "for", "that", "this", "from",
            "are", "was", "were", "have", "has", "will", "our", "your", "you", "we",
            "they", "their", "which", "not", "can", "all"
        };

        private static readonly HashSet<string> TurkishFunctionWords = new(StringComparer.Ordinal)
        {
            "ve", "ile", "için", "icin", "bir", "bu", "olarak", "gibi", "daha",
            "çok", "cok", "veya", "ama", "olan", "kadar"
        };

        public static EnglishTextAnalysis Analyze(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new EnglishTextAnalysis(0, 0, 0, 0, false);

            var wordCount = 0;
            var englishHitCount = 0;
            var turkishHitCount = 0;

            foreach (Match match in WordRegex.Matches(text))
            {
                wordCount++;
                var word = match.Value.ToLowerInvariant();

                if (EnglishFunctionWords.Contains(word))
                    englishHitCount++;
                else if (TurkishFunctionWords.Contains(word))
                    turkishHitCount++;
            }

            var letterCount = 0;
            var turkishLetterCount = 0;

            foreach (var character in text)
            {
                if (!char.IsLetter(character))
                    continue;

                letterCount++;
                if (TurkishSpecificLetters.Contains(character))
                    turkishLetterCount++;
            }

            var englishRatio = wordCount == 0 ? 0 : (double)englishHitCount / wordCount;
            var turkishWordRatio = wordCount == 0 ? 0 : (double)turkishHitCount / wordCount;
            var turkishLetterRatio = letterCount == 0 ? 0 : (double)turkishLetterCount / letterCount;

            var isLikelyEnglish = wordCount >= MinimumWordCount
                                  && englishRatio >= MinimumEnglishFunctionWordRatio
                                  && turkishWordRatio <= MaximumTurkishFunctionWordRatio
                                  && turkishLetterRatio <= MaximumTurkishLetterRatio;

            return new EnglishTextAnalysis(wordCount, englishRatio, turkishWordRatio, turkishLetterRatio, isLikelyEnglish);
        }

        public static bool IsLikelyEnglish(string? text) => Analyze(text).IsLikelyEnglish;
    }
}
