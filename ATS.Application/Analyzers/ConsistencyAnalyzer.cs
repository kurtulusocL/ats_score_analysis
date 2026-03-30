using System.Text.RegularExpressions;
using ATS.Application.Analyzers.Base;
using ATS.Application.Analyzers.Base.Models;
using ATS.Core.Constants;
using ATS.Core.Extensions;

namespace ATS.Application.Analyzers
{
    public class ConsistencyAnalyzer : IAnalyzer
    {
        public string SectionName => "Consistency";
        public int MaxScore => 25;

        private readonly IEnumerable<string> _summaryHeaders;
        private readonly IEnumerable<string> _experienceHeaders;
        private readonly IEnumerable<string> _skillsHeaders;

        private static readonly Regex _chronologyRegex = new(@"\b(19|20)\d{2}\b", RegexOptions.IgnoreCase);

        private static readonly string[] _explainedGapKeywords =
        { "research", "independent", "project", "self-employed", "education", "course", "health problem", "study", "freelance", "reposition" };

        public ConsistencyAnalyzer(IEnumerable<string> summaryHeaders, IEnumerable<string> experienceHeaders, IEnumerable<string> skillsHeaders)
        {
            _summaryHeaders = summaryHeaders;
            _experienceHeaders = experienceHeaders;
            _skillsHeaders = skillsHeaders;
        }

        public AnalyzerResult Analyze(string cvText, string? jobDescription = null)
        {
            var result = new AnalyzerResult
            {
                SectionName = SectionName,
                MaxScore = MaxScore
            };

            var lowerText = cvText.ToLowerInvariant();

            string summaryText = ExtractSectionAnywhere(lowerText, _summaryHeaders);
            string experienceText = ExtractSectionAnywhere(lowerText, _experienceHeaders);
            string skillsText = ExtractSectionAnywhere(lowerText, _skillsHeaders);

            bool orderPenalty = CheckSectionOrderPenalty(lowerText);

            AnalyzeSummaryExperienceConsistency(result, summaryText, experienceText);
            AnalyzeTechConsistency(result, skillsText, experienceText);
            AnalyzeChronology(result, cvText); // Boşluk analizi burada yapılıyor

            if (orderPenalty)
            {
                int penalty = 3;
                result.Score -= penalty;
                result.Suggestions.Add("Section order is non-standard. Recommended order: Summary → Experience → Skills.");
                result.Feedbacks.Add($"⚠️ Non-standard section order detected (-{penalty} pts).");
            }

            result.Score = Math.Max(0, Math.Min(result.Score, MaxScore));
            result.IsPassed = result.Score >= (MaxScore / 2);
            return result;
        }

        private static string ExtractSectionAnywhere(string lowerText, IEnumerable<string> targetHeaders)
        {
            // Tüm bilinen section başlıkları — bitiş sınırı için
            var allKnownHeaders = new[]
            {
                "summary", "professional summary", "career summary", "about me", "profile",
                "experience", "professional experience", "work experience", "employment history",
                "skills", "technical skills", "core technical skills", "core skills",
                "education", "academic background","bachelor's degree","graduate",
                "certifications", "certificates",
                "languages",
                "projects", "personal projects",
                "references",
                "awards", "publications", "volunteer", "interests"
            };

            int startIndex = targetHeaders
                .Select(h => lowerText.IndexOf(h, StringComparison.OrdinalIgnoreCase))
                .Where(i => i >= 0)
                .DefaultIfEmpty(-1)
                .Min();

            if (startIndex < 0) return string.Empty;

            int endIndex = allKnownHeaders
                .Where(h => !targetHeaders.Contains(h, StringComparer.OrdinalIgnoreCase))
                .Select(h => lowerText.IndexOf(h, startIndex + 1, StringComparison.OrdinalIgnoreCase))
                .Where(i => i > startIndex).DefaultIfEmpty(lowerText.Length).Min();

            return lowerText.Substring(startIndex, endIndex - startIndex);
        }

        private bool CheckSectionOrderPenalty(string lowerText)
        {
            int summaryPos = _summaryHeaders
                .Select(h => lowerText.IndexOf(h, StringComparison.OrdinalIgnoreCase))
                .Where(i => i >= 0).DefaultIfEmpty(int.MaxValue).Min();

            int experiencePos = _experienceHeaders
                .Select(h => lowerText.IndexOf(h, StringComparison.OrdinalIgnoreCase))
                .Where(i => i >= 0).DefaultIfEmpty(int.MaxValue).Min();

            int skillsPos = _skillsHeaders
                .Select(h => lowerText.IndexOf(h, StringComparison.OrdinalIgnoreCase))
                .Where(i => i >= 0).DefaultIfEmpty(int.MaxValue).Min();

            if (summaryPos == int.MaxValue ||
                experiencePos == int.MaxValue ||
                skillsPos == int.MaxValue)
                return false;

            return summaryPos > experiencePos;
        }

        private void AnalyzeSummaryExperienceConsistency(AnalyzerResult result, string summaryText, string experienceText)
        {
            if (summaryText.IsNullOrWhiteSpace())
            {
                result.MissingItems.Add("Career Summary");
                result.Suggestions.Add("No career summary found. Add a professional summary that reflects your experience.");
                return;
            }

            if (experienceText.IsNullOrWhiteSpace())
            {
                result.MissingItems.Add("Experience Section");
                result.Suggestions.Add("No experience section found. Add your work history.");
                return;
            }

            var summaryTokens = summaryText.Tokenize(StopWords.Default);
            var experienceTokens = experienceText.Tokenize(StopWords.Default);

            var overlapRatio = summaryTokens.Count > 0
                ? (double)summaryTokens.Intersect(experienceTokens).Count() / summaryTokens.Count
                : 0;

            if (overlapRatio >= 0.45)
            {
                result.Score += 10;
                result.Feedbacks.Add("✓ Career summary strongly reflects the experience section.");
            }
            else if (overlapRatio >= 0.25)
            {
                result.Score += 6;
                result.Feedbacks.Add("✓ Career summary partially matches experience section.");
                result.Suggestions.Add("Align your summary more closely with your actual experience.");
            }
            else if (overlapRatio >= 0.10)
            {
                result.Score += 3;
                result.Suggestions.Add("Your career summary and experience section have low overlap.");
            }
            else
            {
                result.MissingItems.Add("Summary-Experience Alignment");
                result.Suggestions.Add("Career summary does not reflect the experience section.");
            }
        }

        private void AnalyzeTechConsistency(AnalyzerResult result, string skillsText, string experienceText)
        {
            if (skillsText.IsNullOrWhiteSpace())
            {
                result.MissingItems.Add("Skills Section");
                result.Suggestions.Add("No skills section found. List your technical skills and tools.");
                return;
            }

            var skillTokens = skillsText.Tokenize(StopWords.Default);

            if (!skillTokens.Any())
            {
                result.Suggestions.Add("No recognizable skills found in the skills section.");
                return;
            }

            if (experienceText.IsNullOrWhiteSpace())
            {
                result.Score += 3;
                result.Suggestions.Add("Skills listed but no experience section found to verify usage.");
                return;
            }

            var experienceTokens = experienceText.Tokenize(StopWords.Default);
            var mentionedInExperience = skillTokens.Intersect(experienceTokens).ToList();
            var notMentioned = skillTokens.Except(experienceTokens).ToList();

            var mentionRatio = skillTokens.Count > 0
                ? (double)mentionedInExperience.Count / skillTokens.Count
                : 0;

            if (mentionRatio >= 0.70)
            {
                result.Score += 10;
                result.Feedbacks.Add($"✓ Most listed skills are backed by experience ({mentionedInExperience.Count}/{skillTokens.Count}).");
            }
            else if (mentionRatio >= 0.40)
            {
                result.Score += 6;
                result.Feedbacks.Add($"✓ Some skills are mentioned in experience ({mentionedInExperience.Count}/{skillTokens.Count}).");
                if (notMentioned.Any())
                    result.Suggestions.Add($"These skills are listed but not mentioned in experience: {string.Join(", ", notMentioned.Take(5))}.");
            }
            else
            {
                result.Score += 2;
                result.MissingItems.Add("Skills-Experience Alignment");
                result.Suggestions.Add($"Most listed skills are not mentioned in your experience. Describe how you used: {string.Join(", ", notMentioned.Take(5))}.");
            }
        }

        private void AnalyzeChronology(AnalyzerResult result, string cvText)
        {
            var lowerText = cvText.ToLowerInvariant();
            var years = _chronologyRegex
                .Matches(cvText).Select(m => int.Parse(m.Value)).Where(y => y >= 1980 && y <= DateTime.Now.Year).OrderBy(y => y).ToList();

            if (years.Count < 2)
            {
                result.Suggestions.Add("Add employment dates to your experience entries.");
                return;
            }

            var hasCurrentOrRecent = years.Any(y => y >= DateTime.Now.Year - 1);
            var largeGap = false;

            for (int i = 1; i < years.Count; i++)
            {
                if (years[i] - years[i - 1] > 2)
                {
                    largeGap = true;
                    break;
                }
            }

            if (!largeGap && hasCurrentOrRecent)
            {
                result.Score += 11;
                result.Feedbacks.Add("✓ Employment history appears chronologically consistent.");
            }
            else if (largeGap)
            {
                bool isExplained = _explainedGapKeywords.Any(kw => lowerText.Contains(kw));

                if (isExplained)
                {
                    result.Score += 8;
                    result.Feedbacks.Add("✓ Employment gap detected but a valid explanation (Research/Project) was found.");
                }
                else
                {
                    result.Score += 1;
                    result.Suggestions.Add("Large employment gap detected without explanation. Consider explaining the gap if you were doing research or personal projects.");
                }
            }
            else
            {
                result.Score += 5;
            }
        }
    }
}
