using System.Text.RegularExpressions;
using ATS.Application.Analyzers.Base;
using ATS.Application.Analyzers.Base.Models;

namespace ATS.Application.Analyzers
{
    public class KeywordAnalyzer : IAnalyzer
    {
        public string SectionName => "Keyword";
        public int MaxScore => 20;

        private readonly IEnumerable<string> _impactVerbs;
        private readonly IEnumerable<string> _passiveIndicators;

        private static readonly Regex _numericSuccessRegex = new(
        @"\b(\d+[\%\+]|[\d\.]+[Mk]\+|\d+\s+(countries|clients|users|years|projects|records|apps))\b", RegexOptions.IgnoreCase);

        private static readonly Regex _ratioRegex = new(@"\d+\s*[xX]\s|\d+\s*times|\d+\s*fold");
        private static readonly Regex _currencyRegex = new(@"[\$€£₺]\s*\d+|\d+\s*(USD|EUR|TRY|GBP)");
        private static readonly Regex _keywordSpamRegex = new(@"(\b\w+\b)(?:\W+\1){3,}", RegexOptions.IgnoreCase);

        public KeywordAnalyzer(IEnumerable<string> impactVerbs, IEnumerable<string> passiveIndicators)
        {
            _impactVerbs = impactVerbs;
            _passiveIndicators = passiveIndicators;
        }

        public AnalyzerResult Analyze(string cvText, string? jobDescription = null)
        {
            var result = new AnalyzerResult
            {
                SectionName = SectionName,
                MaxScore = MaxScore
            };

            var lowerText = cvText.ToLowerInvariant();
            var words = cvText.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var totalWords = words.Length;

            var foundVerbs = _impactVerbs.Where(v => lowerText.Contains(v.ToLowerInvariant())).ToList();
            if (foundVerbs.Count >= 6) result.Score += 8;
            else if (foundVerbs.Count >= 3) result.Score += 4;
            else result.Suggestions.Add("Use strong action verbs (e.g. developed, improved, optimized).");

            var successMatches = _numericSuccessRegex.Matches(cvText).Count;
            var currencyMatches = _currencyRegex.Matches(cvText).Count;
            var ratioMatches = _ratioRegex.Matches(cvText).Count;

            int quantScore = 0;
            if (successMatches >= 3) quantScore += 6;
            else if (successMatches >= 1) quantScore += 3;

            if (currencyMatches >= 1 || ratioMatches >= 1) quantScore += 2;

            result.Score += Math.Min(quantScore, 8);

            if (successMatches > 0)
                result.Feedbacks.Add($"✓ Quantified achievements detected ({successMatches} found, including metrics).");
            else
                result.Suggestions.Add("Add more metrics (%, +, numbers) to quantify your impact.");

            var uniqueWords = words.Select(w => Regex.Replace(w.ToLowerInvariant(), @"[^a-z]", "")).Where(w => w.Length > 3).Distinct().Count();

            var diversityRatio = totalWords > 0 ? (double)uniqueWords / totalWords : 0;
            if (diversityRatio >= 0.45)
            {
                result.Score += 6;
                result.Feedbacks.Add("✓ Good keyword diversity for a senior profile.");
            }
            else
            {
                result.Score += 2;
                result.Suggestions.Add("Try to vary your vocabulary; avoid repeating the same technical terms too often.");
            }

            var passiveCount = _passiveIndicators.Count(p => lowerText.Contains(p.ToLowerInvariant()));
            if (passiveCount <= 1)
            {
                result.Score += 3;
                result.Feedbacks.Add("✓ Active language used throughout.");
            }
            else
            {
                result.Score += 1;
                result.Suggestions.Add("Replace passive phrases (e.g. 'was responsible for') with action verbs.");
            }

            if (_keywordSpamRegex.IsMatch(cvText))
            {
                result.Score -= 3;
                result.Suggestions.Add("⚠️ Keyword stuffing detected. Avoid repetitive word clusters.");
            }
            result.Score = Math.Max(0, Math.Min(result.Score, MaxScore));
            result.IsPassed = result.Score >= (MaxScore / 2);
            return result;
        }
    }
}
