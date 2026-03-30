
namespace ATS.Core.Helpers
{
    public static class FuzzyMatcherHelper
    {
        public static bool IsMatch(string source, string target, double threshold = 0.80)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)) return false;
            source = source.ToLower().Trim();
            target = target.ToLower().Trim();

            if (source.Contains(target) || target.Contains(source)) return true;

            int steps = LevenshteinDistance(source, target);
            double similarity = 1.0 - ((double)steps / Math.Max(source.Length, target.Length));
            return similarity >= threshold;
        }

        private static int LevenshteinDistance(string s, string t)
        {
            int n = s.Length, m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;
            for (int i = 1; i <= n; i++)
                for (int j = 1; j <= m; j++)
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + (t[j - 1] == s[i - 1] ? 0 : 1));
            return d[n, m];
        }
    }
}
