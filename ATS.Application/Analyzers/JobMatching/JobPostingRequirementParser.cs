using System.Text.RegularExpressions;

namespace ATS.Application.Analyzers.JobMatching
{
    public static class JobPostingRequirementParser
    {
        private static readonly Regex BulletRegex = new(@"^\s*(?:[*\-•●·–—]\s*|\d+[.)]\s+)", RegexOptions.Compiled);
        private static readonly Regex SentenceBoundaryRegex = new(@"(?<=[.!?;])\s+", RegexOptions.Compiled);
        private static readonly Regex NumberedItemBoundaryRegex = new(@"(?<=(?<!\d)[.!?;])\s*(?=\d{1,3}[.)]\s+\S)", RegexOptions.Compiled);
        private static readonly Regex OptionalHeadingRegex = new(@"prefer|nice to have|bonus|\bplus\b|tercih|artı", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex YearsRegex = new(@"(\d{1,2})\s*(?:\+|-\s*\d{1,2}|to\s+\d{1,2})?\s*(?:years?|yrs?)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static IReadOnlyList<JobPostingRequirement> Parse(string? jobPostingText)
        {
            var requirements = new List<JobPostingRequirement>();

            if (string.IsNullOrWhiteSpace(jobPostingText))
                return requirements;

            var isOptionalSection = false;

            foreach (var rawLine in jobPostingText.Split('\n'))
            {
                var line = rawLine.Trim();
                if (line.Length == 0)
                    continue;

                var hasBullet = BulletRegex.IsMatch(line);
                if (!hasBullet && line.EndsWith(':'))
                {
                    isOptionalSection = OptionalHeadingRegex.IsMatch(line);
                    continue;
                }

                foreach (var statement in SplitIntoStatements(line, hasBullet))
                {
                    var requirement = CreateRequirement(statement, !isOptionalSection);
                    if (requirement != null)
                        requirements.Add(requirement);
                }
            }

            return requirements;
        }

        private static IEnumerable<string> SplitIntoStatements(string line, bool hasBullet)
        {
            var items = NumberedItemBoundaryRegex.Split(line);

            for (var index = 0; index < items.Length; index++)
            {
                var isBulleted = index > 0 || hasBullet;
                var text = BulletRegex.Replace(items[index], string.Empty).Trim();
                var statements = isBulleted ? new[] { text } : SentenceBoundaryRegex.Split(text);

                foreach (var statement in statements.Select(statement => statement.Trim()).Where(statement => statement.Length > 0))
                    yield return statement;
            }
        }

        private static JobPostingRequirement? CreateRequirement(string text, bool isMandatory)
        {
            var yearsMatch = YearsRegex.Match(text);
            if (yearsMatch.Success)
                return new JobPostingRequirement(text, isMandatory, int.Parse(yearsMatch.Groups[1].Value), Array.Empty<RequirementTerm>());

            var terms = RequirementTermExtractor.ExtractRequirementTerms(text);
            return terms.Count == 0 ? null : new JobPostingRequirement(text, isMandatory, null, terms);
        }
    }
}
