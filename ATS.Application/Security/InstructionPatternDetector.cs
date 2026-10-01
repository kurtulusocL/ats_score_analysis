using ATS.Application.Results;
using ATS.Domain.Enums;
using System.Text;
using System.Text.RegularExpressions;

namespace ATS.Application.Security
{
    public class InstructionPatternDetector
    {
        private const int MaximumFindings = 100;
        private const int MaximumSnippetLength = 100;
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(250);

        private sealed record PatternDefinition(string Description, SecurityFindingSeverity Severity, Regex Regex);

        private static PatternDefinition Define(string description, SecurityFindingSeverity severity, string pattern) =>
            new(description, severity, new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, RegexTimeout));

        private static readonly IReadOnlyList<PatternDefinition> PatternDefinitions = new[]
        {
            Define("Attempt to override earlier instructions", SecurityFindingSeverity.High,
                @"\b(ignore|disregard|forget|override|bypass)\s+((all|any|the|your|every|everything)\s+){0,3}(previous|prior|above|earlier|preceding|system)\s+(instructions?|prompts?|rules?|guidelines?|context|directions?)\b"),

            Define("Attempt to override earlier instructions", SecurityFindingSeverity.High,
                @"\b(onceki|yukaridaki)\s+(tum\s+|butun\s+)?(talimat|komut|yonerge|direktif|kural)\w*"),

            Define("Fake system or instruction marker", SecurityFindingSeverity.High,
                @"\[\s*(system|inst|instruction)s?\s*\]|<\s*/?\s*(system|instruction)s?\s*>|#{3,}\s*(system|instruction)s?\b"),

            Define("Announcement of new instructions", SecurityFindingSeverity.High,
                @"\b(new|updated|revised)\s+instructions?\s*:|\byeni\s+(talimat|komut|yonerge)\w*\s*:"),

            Define("Attempt to dictate the candidate's score", SecurityFindingSeverity.High,
                @"\b(give|assign|award|rate|score|grade)\s+(this|the)\s+(candidate|applicant|resume|cv)\s+(a\s+|an\s+)?((score|rating|grade)\s+of\s+)?(100|10\s*/\s*10|full\s+marks|maximum|perfect|the\s+highest|highest)"),

            Define("Attempt to dictate the candidate's score", SecurityFindingSeverity.High,
                @"\baday\w*\s+(\w+\s+){0,3}(100|tam\s+puan|en\s+yuksek\s+puan)\s+(puan\w*\s+)?(ver|verin|verilsin|verilmeli)\b"),

            Define("Attempt to dictate the candidate's score", SecurityFindingSeverity.High,
                @"\b(100|tam\s+puan|en\s+yuksek\s+puan)\s+(puan\w*\s+)?(ver|verin|verilsin|verilmeli)\b"),

            Define("Phrasing that dictates the hiring decision", SecurityFindingSeverity.Low,
                @"\b(hire|select|shortlist)\s+(this|the)\s+(candidate|applicant)\b|\b(this|the)\s+(candidate|applicant)\s+(\w+\s+){0,3}(should|must)\s+(be\s+)?(hired|selected|shortlisted|ranked\s+first)\b|\b(this|the)\s+candidate\s+is\s+(the\s+)?(perfect|ideal|best|top)\b"),

            Define("Phrasing that dictates the hiring decision", SecurityFindingSeverity.Low,
                @"\bbu\s+aday\w*\s+(\w+\s+){0,2}(ise\s+al|sec|tercih\s+et|onerin)\w*|\bbu\s+aday\w*\s+(\w+\s+){0,2}(mukemmel|ideal|en\s+iyi)\b")
        };

        public IReadOnlyList<SecurityFindingResult> Detect(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<SecurityFindingResult>();

            var normalizedText = Normalize(text);
            var candidates = new List<(int Start, int Length, PatternDefinition Definition)>();

            foreach (var patternDefinition in PatternDefinitions)
            {
                try
                {
                    foreach (Match match in patternDefinition.Regex.Matches(normalizedText))
                        candidates.Add((match.Index, match.Length, patternDefinition));
                }
                catch (RegexMatchTimeoutException)
                {
                    
                }
            }

            var findings = new List<SecurityFindingResult>();
            var lastCoveredEnd = 0;

            foreach (var candidate in candidates.OrderBy(item => item.Start).ThenByDescending(item => item.Length))
            {
                if (candidate.Start < lastCoveredEnd)
                    continue;

                findings.Add(new SecurityFindingResult(
                    SecurityFindingType.InstructionPattern,
                    candidate.Definition.Severity,
                    candidate.Definition.Description,
                    CreateSnippet(text.Substring(candidate.Start, candidate.Length))));

                lastCoveredEnd = candidate.Start + candidate.Length;

                if (findings.Count >= MaximumFindings)
                    break;
            }

            return findings;
        }

        private static string CreateSnippet(string matchedText)
        {
            var singleLineText = Regex.Replace(matchedText, @"\s+", " ").Trim();

            return singleLineText.Length <= MaximumSnippetLength
                ? singleLineText
                : singleLineText[..(MaximumSnippetLength - 3)] + "...";
        }

        private static string Normalize(string text)
        {
            var builder = new StringBuilder(text.Length);

            foreach (var character in text)
            {
                builder.Append(character switch
                {
                    'ı' or 'İ' or 'I' => 'i',
                    'ğ' or 'Ğ' => 'g',
                    'ü' or 'Ü' => 'u',
                    'ş' or 'Ş' => 's',
                    'ö' or 'Ö' => 'o',
                    'ç' or 'Ç' => 'c',
                    _ => char.ToLowerInvariant(character)
                });
            }

            return builder.ToString();
        }
    }
}
