using System.Text.RegularExpressions;
using ATS.Application.Analyzers.Base;
using ATS.Application.Analyzers.Base.Models;

namespace ATS.Application.Analyzers
{
    public class FormatAnalyzer : IAnalyzer
    {
        public string SectionName => "Format";
        public int MaxScore => 15;

        private static readonly string[] _datePatterns =
        {
            @"\b(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[a-z]*\.?\s+\d{4}\b",
            @"\b\d{2}/\d{4}\b",
            @"\b\d{4}\s*[-–]\s*(\d{4}|present|current|now)\b"
        };

        private static readonly Regex _bulletRegex = new(@"^[\s]*[•\-\*\u2022\u25AA\u25CF]", RegexOptions.Multiline);
        private static readonly Regex _allCapsLineRegex = new(@"^[A-Z\s]{4,}$", RegexOptions.Multiline);
        private static readonly Regex _emailRegex = new(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
        private static readonly Regex _phoneRegex = new(@"(\+?\d[\d\s\-\(\)]{7,}\d)");
        private static readonly Regex _urlRegex = new(@"(linkedin\.com|github\.com|portfolio|website)", RegexOptions.IgnoreCase);

        public AnalyzerResult Analyze(string cvText, string? jobDescription = null)
        {
            var result = new AnalyzerResult
            {
                SectionName = SectionName,
                MaxScore = MaxScore
            };

            var lines = cvText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var wordCount = cvText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

            int careerYears = CalculateCareerYears(cvText);
            int wordLimit = 900;

            if (careerYears >= 4 && careerYears < 7) wordLimit = 1200;
            else if (careerYears >= 7) wordLimit = 1800;
           
            var dateMatchCount = _datePatterns.Sum(p => Regex.Matches(cvText, p, RegexOptions.IgnoreCase).Count);
            if (dateMatchCount >= 2)
            {
                result.Score += 3;
                result.Feedbacks.Add("✓ Consistent date formatting detected.");
            }
            else
            {
                result.MissingItems.Add("Date Format");
                result.Suggestions.Add("Use consistent date formats throughout (e.g. Jan 2020 – Mar 2023).");
            }
           
            var bulletCount = _bulletRegex.Matches(cvText).Count;
            if (bulletCount >= 3)
            {
                result.Score += 2;
                result.Feedbacks.Add("✓ Bullet points used for experience descriptions.");
            }
            else
            {
                result.Suggestions.Add("Use bullet points to describe your experience — improves ATS readability.");
            }

            var capsLines = _allCapsLineRegex.Matches(cvText).Count;
            if (capsLines >= 2)
            {
                result.Score += 2;
                result.Feedbacks.Add("✓ Section headers are clearly formatted.");
            }
            else
            {
                result.Suggestions.Add("Use clear section headers (e.g. EXPERIENCE, EDUCATION) to improve ATS parsing.");
            }

            var hasEmail = _emailRegex.IsMatch(cvText);
            var hasPhone = _phoneRegex.IsMatch(cvText);
            var hasUrl = _urlRegex.IsMatch(cvText);

            if (hasEmail && hasPhone)
            {
                result.Score += 3;
                result.Feedbacks.Add("✓ Contact information (email and phone) is present.");
            }
            else
            {
                if (!hasEmail) result.Suggestions.Add("Add your email address to the CV.");
                if (!hasPhone) result.Suggestions.Add("Add your phone number to the CV.");
            }

            if (hasUrl)
            {
                result.Score += 1;
                result.Feedbacks.Add("✓ Online profile (LinkedIn/GitHub) detected.");
            }

            if (wordCount >= 300 && wordCount <= wordLimit)
            {
                result.Score += 8;
                result.Feedbacks.Add($"✓ CV length ({wordCount} words) is appropriate for your {careerYears}+ years of experience (Limit: {wordLimit}).");
            }
            else if (wordCount < 300)
            {
                result.Score += 2;
                result.Suggestions.Add($"CV is very short ({wordCount} words). A professional CV should be at least 300 words.");
            }
            else
            {
                result.Score += 4;
                result.Suggestions.Add($"CV is long ({wordCount} words). For your seniority level, consider trimming to under {wordLimit} words.");
            }

            result.Score = Math.Min(result.Score, MaxScore);
            result.IsPassed = result.Score >= (MaxScore / 2);
            return result;
        }

        private int CalculateCareerYears(string text)
        {
            var yearMatches = Regex.Matches(text, @"\b(19|20)\d{2}\b");
            if (yearMatches.Count == 0) return 1;

            var years = yearMatches.Select(m => int.Parse(m.Value)).OrderBy(y => y).ToList();
            int firstYear = years.First();
            int currentYear = DateTime.Now.Year;

            int diff = currentYear - firstYear;
            return diff <= 0 ? 1 : diff;
        }
    }
}
