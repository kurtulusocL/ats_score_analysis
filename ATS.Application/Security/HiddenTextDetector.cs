using ATS.Application.Results;
using ATS.Application.Security.Enums;
using ATS.Domain.Enums;
using System.Text;
using System.Text.RegularExpressions;

namespace ATS.Application.Security
{
    public class HiddenTextDetector
    {
        private readonly InstructionPatternDetector _instructionPatternDetector;

        public HiddenTextDetector(InstructionPatternDetector instructionPatternDetector)
        {
            _instructionPatternDetector = instructionPatternDetector;
        }

        public IReadOnlyList<SecurityFindingResult> Detect(
            int pageNumber,
            double pageWidth,
            double pageHeight,
            IEnumerable<LetterStyleInfo> letters)
        {
            ArgumentNullException.ThrowIfNull(letters);

            var findings = new List<SecurityFindingResult>();
            var segmentText = new StringBuilder();
            var location = $"on page {pageNumber}";
            HiddenTextReason? currentReason = null;

            void FlushSegment()
            {
                if (currentReason != null && findings.Count < HiddenTextThresholds.MaximumFindings)
                {
                    var finding = TryCreateFinding(segmentText.ToString(), currentReason.Value, location);
                    if (finding != null)
                        findings.Add(finding);
                }

                segmentText.Clear();
            }

            foreach (var letter in letters)
            {
                if (string.IsNullOrWhiteSpace(letter.Text))
                {
                    if (currentReason != null)
                        segmentText.Append(' ');

                    continue;
                }

                var reason = Classify(letter, pageWidth, pageHeight);

                if (reason != currentReason)
                {
                    FlushSegment();
                    currentReason = reason;
                }

                if (reason != null)
                    segmentText.Append(letter.Text);
            }

            FlushSegment();
            return findings;
        }

        public SecurityFindingResult? TryCreateFinding(string hiddenText, HiddenTextReason reason, string location)
        {
            var text = Regex.Replace(hiddenText ?? string.Empty, @"\s+", " ").Trim();
            var visibleCharacterCount = text.Count(character => !char.IsWhiteSpace(character));

            if (visibleCharacterCount < HiddenTextThresholds.MinimumSegmentLength)
                return null;

            var instructionFindings = _instructionPatternDetector.Detect(text);

            if (instructionFindings.Count > 0)
            {
                return new SecurityFindingResult(
                SecurityFindingType.HiddenText,
                SecurityFindingSeverity.High,
                $"Hidden text ({Describe(reason)}) contains instruction-like phrasing {location}",
                text);
            }

            return new SecurityFindingResult(
                SecurityFindingType.HiddenText,
                SecurityFindingSeverity.Low,
                $"Hidden text ({Describe(reason)}) {location}",
                text);
        }

        private static HiddenTextReason? Classify(LetterStyleInfo letter, double pageWidth, double pageHeight)
        {
            if (letter.Right < -HiddenTextThresholds.OffPageMargin
                || letter.Left > pageWidth + HiddenTextThresholds.OffPageMargin
                || letter.Top < -HiddenTextThresholds.OffPageMargin
                || letter.Bottom > pageHeight + HiddenTextThresholds.OffPageMargin)
            {
                return HiddenTextReason.OutsidePage;
            }

            if (letter.PointSize > 0 && letter.PointSize < HiddenTextThresholds.MinimumVisiblePointSize)
                return HiddenTextReason.TinyText;

            if (letter.HasColor
                && letter.Red >= HiddenTextThresholds.NearWhiteThreshold
                && letter.Green >= HiddenTextThresholds.NearWhiteThreshold
                && letter.Blue >= HiddenTextThresholds.NearWhiteThreshold)
            {
                return HiddenTextReason.WhiteText;
            }

            return null;
        }

        private static string Describe(HiddenTextReason reason) => reason switch
        {
            HiddenTextReason.WhiteText => "white or near-white text",
            HiddenTextReason.TinyText => "text smaller than 3 pt",
            HiddenTextReason.HiddenFormatting => "text formatted as hidden",
            _ => "text outside the page area"
        };
    }
}
