using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ATS.Application.Analyzers.JobMatching
{
    public static class RequirementTermExtractor
    {
        private const int MinimumStemLength = 4;
        private static readonly Regex TokenRegex = new(@"[\p{L}\p{N}#+]+(?:[./][\p{L}\p{N}#+]+)*", RegexOptions.Compiled);
        private static readonly Regex NumberOnlyRegex = new(@"^\d+\+?$", RegexOptions.Compiled);
        private static readonly string[] DerivationalSuffixes = { "ability", "ation", "ment", "ing", "able", "er", "ed", "ly" };
        private static readonly string[] EsPluralEndings = { "sses", "xes", "ches", "shes", "zes" };
        private static readonly string[] KeepSingularEndings = { "ss", "us", "sis" };
        private static readonly HashSet<string> IgnoredWords = new(StringComparer.Ordinal)
        {
            "a", "an", "and", "or", "of", "to", "in", "on", "for", "with", "the", "is", "are", "be", "as", "at", "by",
            "from", "that", "this", "these", "those", "it", "its", "their", "our", "your", "you", "we", "they", "into",
            "using", "use", "within", "across", "more", "one", "any", "all", "not", "can", "will", "may", "able", "also",
            "well", "new", "via", "per", "such", "like", "etc",
            "strong", "ability", "experience", "familiarity", "understanding", "exposure", "willingness", "effective",
            "continuous", "common", "established", "existing", "related", "similar", "equivalent", "including",
            "ensure", "utilize"
        };

        public static IReadOnlyList<RequirementTerm> ExtractRequirementTerms(string? text)
        {
            var terms = new List<RequirementTerm>();
            var seenStems = new HashSet<string>(StringComparer.Ordinal);

            foreach (var token in SplitTokens(text))
            {
                var stem = Stem(token);
                if (seenStems.Add(stem))
                    terms.Add(new RequirementTerm(stem, token));
            }

            return terms;
        }

        public static IReadOnlySet<string> ExtractCvStems(string? text)
        {
            var stems = new HashSet<string>(StringComparer.Ordinal);

            foreach (var token in SplitTokens(text))
            {
                stems.Add(Stem(token));

                if (!token.Contains('.'))
                    continue;

                foreach (var part in token.Split('.', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (IsMeaningful(part))
                        stems.Add(Stem(part));
                }
            }

            return stems;
        }

        public static string Stem(string word)
        {
            var stem = RemovePlural(word);

            foreach (var suffix in DerivationalSuffixes)
            {
                if (stem.EndsWith(suffix, StringComparison.Ordinal) && stem.Length - suffix.Length >= MinimumStemLength)
                {
                    stem = stem[..^suffix.Length];
                    break;
                }
            }
            return stem.Length > MinimumStemLength && stem.EndsWith('e') ? stem[..^1] : stem;
        }

        private static string RemovePlural(string word)
        {
            if (word.Length <= 3 || !word.All(char.IsLetter))
                return word;

            if (word.Length > 4 && word.EndsWith("ies", StringComparison.Ordinal))
                return word[..^3] + "y";

            if (EsPluralEndings.Any(ending => word.EndsWith(ending, StringComparison.Ordinal)))
                return word[..^2];

            if (word.EndsWith('s') && !KeepSingularEndings.Any(ending => word.EndsWith(ending, StringComparison.Ordinal)))
                return word[..^1];

            return word;
        }

        private static IEnumerable<string> SplitTokens(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                yield break;

            foreach (Match match in TokenRegex.Matches(text.ToLowerInvariant()))
            {
                foreach (var token in SplitSlashes(match.Value))
                {
                    if (IsMeaningful(token))
                        yield return token;
                }
            }
        }

        private static IEnumerable<string> SplitSlashes(string token)
        {
            if (!token.Contains('/'))
                return new[] { token };

            var parts = token.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 && parts.All(IsShortAbbreviation)
                ? new[] { string.Join('/', parts) }
                : parts;
        }

        private static bool IsShortAbbreviation(string part) => part.Length <= 3 && part.All(char.IsLetter);

        private static bool IsMeaningful(string token) =>
            token.Any(char.IsLetterOrDigit)
            && !NumberOnlyRegex.IsMatch(token)
            && !IgnoredWords.Contains(token);
    }
}
