using ATS.Application.Results;
using ATS.Application.Security.Enums;
using System.Globalization;
using System.Text;

namespace ATS.Application.Security
{
    public class WordHiddenTextDetector
    {
        private readonly HiddenTextDetector _hiddenTextDetector;

        public WordHiddenTextDetector(HiddenTextDetector hiddenTextDetector)
        {
            _hiddenTextDetector = hiddenTextDetector;
        }

        public IReadOnlyList<SecurityFindingResult> Detect(string location, IEnumerable<WordRunStyleInfo> runs)
        {
            ArgumentNullException.ThrowIfNull(runs);

            var findings = new List<SecurityFindingResult>();
            var segmentText = new StringBuilder();
            HiddenTextReason? currentReason = null;

            void FlushSegment()
            {
                if (currentReason != null)
                {
                    var finding = _hiddenTextDetector.TryCreateFinding(segmentText.ToString(), currentReason.Value, location);
                    if (finding != null)
                        findings.Add(finding);
                }

                segmentText.Clear();
            }

            foreach (var run in runs)
            {
                if (string.IsNullOrWhiteSpace(run.Text))
                {
                    if (currentReason != null)
                        segmentText.Append(' ');

                    continue;
                }

                var reason = Classify(run);

                if (reason != currentReason)
                {
                    FlushSegment();
                    currentReason = reason;
                }

                if (reason != null)
                    segmentText.Append(run.Text);
            }

            FlushSegment();
            return findings;
        }

        private static HiddenTextReason? Classify(WordRunStyleInfo run)
        {
            if (run.IsVanished)
                return HiddenTextReason.HiddenFormatting;

            if (run.PointSize is > 0 and < HiddenTextThresholds.MinimumVisiblePointSize)
                return HiddenTextReason.TinyText;

            if (IsNearWhite(run.FontColorHex) && !IsDark(run.BackgroundFillHex))
                return HiddenTextReason.WhiteText;

            return null;
        }

        private static bool IsNearWhite(string? hexColor)
        {
            return TryParseRgb(hexColor, out var red, out var green, out var blue)
                   && red >= HiddenTextThresholds.NearWhiteThreshold
                   && green >= HiddenTextThresholds.NearWhiteThreshold
                   && blue >= HiddenTextThresholds.NearWhiteThreshold;
        }

        private static bool IsDark(string? hexColor)
        {
            if (!TryParseRgb(hexColor, out var red, out var green, out var blue))
                return false;

            var luminance = 0.299 * red + 0.587 * green + 0.114 * blue;
            return luminance < HiddenTextThresholds.DarkBackgroundLuminance;
        }

        private static bool TryParseRgb(string? hexColor, out double red, out double green, out double blue)
        {
            red = green = blue = 0;

            if (hexColor is not { Length: 6 })
                return false;

            if (!int.TryParse(hexColor.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var redValue)
                || !int.TryParse(hexColor.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var greenValue)
                || !int.TryParse(hexColor.AsSpan(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var blueValue))
            {
                return false;
            }

            red = redValue / 255.0;
            green = greenValue / 255.0;
            blue = blueValue / 255.0;
            return true;
        }
    }
}
