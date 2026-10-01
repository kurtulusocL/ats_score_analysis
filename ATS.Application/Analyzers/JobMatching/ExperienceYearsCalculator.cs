using System.Text.RegularExpressions;

namespace ATS.Application.Analyzers.JobMatching
{
    public static class ExperienceYearsCalculator
    {
        private static readonly Regex RangeRegex = new(
            @"(?:(?<startMonth>[A-Za-z]{3,9})\.?\s+)?(?<startYear>(?:19|20)\d{2})\s*[–—\-]\s*(?:(?<endMonth>[A-Za-z]{3,9})\.?\s+)?(?<endYear>(?:19|20)\d{2}|present|current|now|ongoing|today)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex NonWorkLineRegex = new(
            @"\b(?:break|gap|sabbatical|universit\w*|bachelor\w*|master'?s|master of|degree|diploma|academy|school|college|course|certificate|bootcamp|training|education)\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex StatedYearsRegex = new(@"(\d{1,2})\s*\+?\s*years", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly string[] MonthPrefixes =
            { "jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec" };

        private const int FirstMonth = 1;
        private const int LastMonth = 12;

        public static double? Calculate(string? cvText, DateTime today)
        {
            if (string.IsNullOrWhiteSpace(cvText))
                return null;

            var ranges = FindWorkRanges(cvText, today);
            if (ranges.Count > 0)
                return CountUnionMonths(ranges) / 12.0;

            var statedYears = StatedYearsRegex.Matches(cvText)
                .Select(match => int.Parse(match.Groups[1].Value))
                .DefaultIfEmpty(0)
                .Max();

            return statedYears > 0 ? statedYears : null;
        }

        private static List<(int Start, int End)> FindWorkRanges(string cvText, DateTime today)
        {
            var ranges = new List<(int Start, int End)>();

            foreach (Match match in RangeRegex.Matches(cvText))
            {
                if (NonWorkLineRegex.IsMatch(GetLineContaining(cvText, match.Index)))
                    continue;

                var start = ToMonthIndex(int.Parse(match.Groups["startYear"].Value), match.Groups["startMonth"].Value, FirstMonth);
                var endYearText = match.Groups["endYear"].Value;
                var end = char.IsDigit(endYearText[0])
                    ? ToMonthIndex(int.Parse(endYearText), match.Groups["endMonth"].Value, LastMonth)
                    : today.Year * 12 + today.Month;

                if (end >= start)
                    ranges.Add((start, end));
            }

            return ranges;
        }

        private static int CountUnionMonths(List<(int Start, int End)> ranges)
        {
            var totalMonths = 0;
            var currentStart = -1;
            var currentEnd = -1;

            foreach (var (start, end) in ranges.OrderBy(range => range.Start))
            {
                if (currentStart < 0 || start > currentEnd + 1)
                {
                    if (currentStart >= 0)
                        totalMonths += currentEnd - currentStart + 1;

                    currentStart = start;
                    currentEnd = end;
                }
                else
                {
                    currentEnd = Math.Max(currentEnd, end);
                }
            }

            return totalMonths + currentEnd - currentStart + 1;
        }

        private static int ToMonthIndex(int year, string monthText, int defaultMonth)
        {
            var month = monthText.Length >= 3
                ? Array.IndexOf(MonthPrefixes, monthText[..3].ToLowerInvariant()) + 1
                : 0;

            return year * 12 + (month > 0 ? month : defaultMonth);
        }

        private static string GetLineContaining(string text, int index)
        {
            var lineStart = text.LastIndexOf('\n', Math.Max(index - 1, 0)) + 1;
            var lineEnd = text.IndexOf('\n', index);
            return lineEnd < 0 ? text[lineStart..] : text[lineStart..lineEnd];
        }
    }
}
