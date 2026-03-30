using ATS.Application.Analyzers.Base;
using ATS.Application.Analyzers.Base.Models;
using ATS.Core.Constants;
using ATS.Core.Extensions;

namespace ATS.Application.Analyzers
{
    public class JobMatchAnalyzer : IAnalyzer
    {
        public string SectionName => "Job Match";
        public int MaxScore => 20;

        private readonly string _jobPostingText;
        private readonly string _jobTitle;

        public JobMatchAnalyzer(string jobPostingText, string jobTitle = "")
        {
            _jobPostingText = jobPostingText;
            _jobTitle = jobTitle;
        }

        public AnalyzerResult Analyze(string cvText, string? jobDescription = null)
        {
            var result = new AnalyzerResult
            {
                SectionName = SectionName,
                MaxScore = MaxScore
            };

            if (_jobPostingText.IsNullOrWhiteSpace())
            {
                result.Suggestions.Add("No job posting provided. Skipping job match analysis.");
                result.IsPassed = false;
                return result;
            }
            var cvTokens = cvText.Tokenize(StopWords.Default).Select(t => t.ToLower()).Where(t => t.Length > 2).ToHashSet();
            var jobTokens = _jobPostingText.Tokenize(StopWords.Default).Select(t => t.ToLower()).Where(t => t.Length > 3).ToHashSet();

            if (!jobTokens.Any())
            {
                result.Suggestions.Add("Job posting text could not be parsed.");
                result.IsPassed = false;
                return result;
            }

            var commonTokens = cvTokens.Intersect(jobTokens).ToList();
            var overlapRatio = (double)commonTokens.Count / jobTokens.Count;

            if (overlapRatio >= 0.60)
            {
                result.Score += 8;
                result.Feedbacks.Add($"✓ Strong keyword match with job posting ({Math.Round(overlapRatio * 100)}%).");
            }
            else if (overlapRatio >= 0.40)
            {
                result.Score += 5;
                result.Feedbacks.Add($"✓ Moderate keyword match with job posting ({Math.Round(overlapRatio * 100)}%).");
                result.Suggestions.Add("Tailor your CV to include more keywords from the job posting.");
            }
            else if (overlapRatio >= 0.20)
            {
                result.Score += 2;
                result.Suggestions.Add($"Low keyword match ({Math.Round(overlapRatio * 100)}%). Review the job posting and align your CV language.");
            }
            else
            {
                result.MissingItems.Add("Job Keyword Match");
                result.Suggestions.Add("Very low match with job posting. Consider rewriting key sections to reflect the job requirements.");
            }

            var missingFromCv = jobTokens.Except(cvTokens).Where(t => t.Length > 4).ToList();
            var missingRatio = (double)missingFromCv.Count / jobTokens.Count;

            if (missingRatio <= 0.20)
            {
                result.Score += 6;
                result.Feedbacks.Add("✓ Most job posting keywords are present in the CV.");
            }
            else if (missingRatio <= 0.40)
            {
                result.Score += 3;
                result.Suggestions.Add($"Some job keywords missing from CV: {string.Join(", ", missingFromCv.Take(8))}.");
            }
            else
            {
                result.MissingItems.Add("Critical Keywords");
                result.Suggestions.Add($"Many job keywords are missing from CV. Consider adding: {string.Join(", ", missingFromCv.Take(8))}.");
            }
           
            string titleToCompare = !string.IsNullOrWhiteSpace(_jobTitle) ? _jobTitle : (_jobPostingText.Length > 100 ? _jobPostingText.Substring(0, 100) : _jobPostingText);
            var titleTokens = titleToCompare.Tokenize(StopWords.Default);
            var titleMatches = titleTokens.Intersect(cvTokens).ToList();
            var titleMatchRatio = titleTokens.Count > 0
                ? (double)titleMatches.Count / titleTokens.Count
                : 0;

            if (titleMatchRatio >= 0.50)
            {
                result.Score += 6;
                result.Feedbacks.Add($"✓ CV aligns well with the job title: '{titleToCompare}'.");
            }
            else if (titleMatchRatio >= 0.25)
            {
                result.Score += 3;
                result.Suggestions.Add($"Partially matches the job role '{titleToCompare}'. Strengthen role-specific language.");
            }
            else
            {
                result.Suggestions.Add($"CV does not clearly reflect the '{titleToCompare}' role. Align your summary.");
            }
            result.Score = Math.Max(0, Math.Min(result.Score, MaxScore));
            result.IsPassed = result.Score >= (MaxScore / 2);
            return result;
        }
    }
}
