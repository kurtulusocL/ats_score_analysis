using System.Text.RegularExpressions;

namespace ATS.Core.Extensions
{
    public static class CvTextExtensions
    {
        private static readonly Regex _emailRegex = new(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
        private static readonly Regex _phoneRegex = new(@"(\+?\d[\d\s\-\(\)]{7,}\d)");
        private static readonly Regex _urlRegex = new(@"(http|www|linkedin|github)", RegexOptions.IgnoreCase);
        private static readonly Regex _namePattern = new(@"^[A-ZÇĞİÖŞÜa-zçğışöüÇĞİÖŞÜ]+(\s[A-ZÇĞİÖŞÜa-zçğışöüÇĞİÖŞÜ]+){1,3}$");

        public static string ExtractCandidateName(this string cvText)
        {
            var lines = cvText.SplitLines().Select(l => l.Trim()).Where(l => !l.IsNullOrWhiteSpace()).Take(10).ToList();
            foreach (var line in lines)
            {
                if (_emailRegex.IsMatch(line)) continue;
                if (_phoneRegex.IsMatch(line)) continue;
                if (_urlRegex.IsMatch(line)) continue;
                if (line.Length < 3 || line.Length > 60) continue;

                if (_namePattern.IsMatch(line))
                    return line;
            }
            return "Unknown";
        }
    }
}
