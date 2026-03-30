using ATS.Application.Analyzers.Base;
using ATS.Application.Analyzers.Base.Models;
using ATS.Core.Extensions;
using ATS.Core.Helpers;

namespace ATS.Application.Analyzers
{
    public class SectionPresenceAnalyzer : IAnalyzer
    {
        public string SectionName => "Section Presence";
        public int MaxScore => 20;

        private readonly Dictionary<string, List<string>> _sectionHeaders;

        private static readonly Dictionary<string, (bool IsRequired, int Point)> _sectionMeta = new()
        {
            ["ContactInfo"] = (true, 4),
            ["Summary"] = (true, 3),
            ["Experience"] = (true, 4),
            ["Education"] = (true, 3),
            ["Skills"] = (true, 3),
            ["Languages"] = (false, 2),
            ["Certifications"] = (false, 1),
        };

        public SectionPresenceAnalyzer(Dictionary<string, List<string>> sectionHeaders)
        {
            _sectionHeaders = sectionHeaders;
        }

        public AnalyzerResult Analyze(string cvText, string? jobDescription = null)
        {
            var result = new AnalyzerResult
            {
                SectionName = SectionName,
                MaxScore = MaxScore
            };

            var lowerText = cvText.ToLowerInvariant();
            var lines = cvText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var section in _sectionMeta)
            {
                var hasHeaders = _sectionHeaders.TryGetValue(section.Key, out var headers) && headers.Any();
                var found = hasHeaders && headers!.Any(h =>
                {
                    var normalizedHeader = h.ToLowerInvariant().Trim();

                    if (lowerText.ContainsIgnoreCase(normalizedHeader) ||
                        (normalizedHeader.Length > 10 && lowerText.ContainsIgnoreCase(normalizedHeader.Substring(0, 10))))
                        return true;

                    return lines.Any(line => FuzzyMatcherHelper.IsMatch(line, normalizedHeader));
                });

                if (found)
                {
                    result.Score += section.Value.Point;
                    result.Feedbacks.Add($"✓ {section.Key} section found.");
                }
                else
                {
                    if (section.Value.IsRequired)
                    {
                        result.MissingItems.Add(section.Key);
                        result.Suggestions.Add($"Add a {section.Key} section — this is a required CV section.");
                    }
                    else
                    {
                        result.Suggestions.Add($"{section.Key} section not found. Consider adding it.");
                    }
                }
            }
            result.Score = Math.Min(result.Score, MaxScore);
            result.IsPassed = result.Score >= (MaxScore / 2);
            return result;
        }
    }
}
