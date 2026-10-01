using ATS.Application.Abstract.Services;
using ATS.Application.Results;
using ATS.Application.Security;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ATS.Infrastructure.Concrete.Security
{
    public class PdfHiddenTextScanner : IHiddenTextFileScanner
    {
        public string FileType => "pdf";

        private readonly HiddenTextDetector _hiddenTextDetector;

        public PdfHiddenTextScanner(HiddenTextDetector hiddenTextDetector)
        {
            _hiddenTextDetector = hiddenTextDetector;
        }

        public IReadOnlyList<SecurityFindingResult> ScanFile(string filePath)
        {
            using var document = PdfDocument.Open(filePath);
            return Scan(document);
        }

        public IReadOnlyList<SecurityFindingResult> ScanBytes(byte[] pdfBytes)
        {
            using var document = PdfDocument.Open(pdfBytes);
            return Scan(document);
        }

        public IReadOnlyList<SecurityFindingResult> Scan(PdfDocument document)
        {
            var findings = new List<SecurityFindingResult>();

            foreach (var page in document.GetPages())
            {
                var letters = page.Letters.Select(ToLetterStyleInfo).ToList();

                findings.AddRange(_hiddenTextDetector.Detect(
                    page.Number,
                    Convert.ToDouble(page.Width),
                    Convert.ToDouble(page.Height),
                    letters));
            }

            return findings;
        }

        private static LetterStyleInfo ToLetterStyleInfo(Letter letter)
        {
            var hasColor = false;
            double red = 0, green = 0, blue = 0;

            if (letter.Color != null)
            {
                var (redValue, greenValue, blueValue) = letter.Color.ToRGBValues();
                red = Convert.ToDouble(redValue);
                green = Convert.ToDouble(greenValue);
                blue = Convert.ToDouble(blueValue);
                hasColor = true;
            }

            var glyphRectangle = letter.GlyphRectangle;

            return new LetterStyleInfo
            (
                letter.Value,
                Convert.ToDouble(letter.PointSize),
                red,
                green,
                blue,
                Convert.ToDouble(glyphRectangle.Left),
                Convert.ToDouble(glyphRectangle.Right),
                Convert.ToDouble(glyphRectangle.Bottom),
                Convert.ToDouble(glyphRectangle.Top),
                hasColor
            );
        }
    }
}
