

namespace ATS.Application.Security
{
    internal static class HiddenTextThresholds
    {
        public const double MinimumVisiblePointSize = 3.0;
        public const double NearWhiteThreshold = 0.95;
        public const double DarkBackgroundLuminance = 0.5;
        public const double OffPageMargin = 5.0;
        public const int MinimumSegmentLength = 3;
        public const int MaximumSnippetLength = 100;
        public const int MaximumFindings = 100;
    }
}
