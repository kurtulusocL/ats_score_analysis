using ATS.Application.Abstract.Services;
using ATS.Application.Results;
using ATS.Application.Security;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Globalization;
using System.Text;

namespace ATS.Infrastructure.Concrete.Security
{
    public class DocxHiddenTextScanner : IHiddenTextFileScanner
    {
        private const int MaximumFindings = 100;
        private readonly WordHiddenTextDetector _wordHiddenTextDetector;
        public string FileType => "docx";

        public DocxHiddenTextScanner(WordHiddenTextDetector wordHiddenTextDetector)
        {
            _wordHiddenTextDetector = wordHiddenTextDetector;
        }

        public IReadOnlyList<SecurityFindingResult> ScanFile(string filePath)
        {
            using var document = WordprocessingDocument.Open(filePath, false);
            return Scan(document);
        }

        public IReadOnlyList<SecurityFindingResult> ScanBytes(byte[] docxBytes)
        {
            using var stream = new MemoryStream(docxBytes);
            using var document = WordprocessingDocument.Open(stream, false);
            return Scan(document);
        }

        public IReadOnlyList<SecurityFindingResult> Scan(WordprocessingDocument document)
        {
            var mainPart = document.MainDocumentPart;
            if (mainPart == null)
                return Array.Empty<SecurityFindingResult>();

            var findings = new List<SecurityFindingResult>();

            if (mainPart.Document?.Body != null)
                findings.AddRange(ScanContainer(mainPart.Document.Body, "in the document body"));

            foreach (var headerPart in mainPart.HeaderParts)
            {
                if (headerPart.Header != null)
                    findings.AddRange(ScanContainer(headerPart.Header, "in a header"));
            }

            foreach (var footerPart in mainPart.FooterParts)
            {
                if (footerPart.Footer != null)
                    findings.AddRange(ScanContainer(footerPart.Footer, "in a footer"));
            }

            return findings
                .DistinctBy(finding => (finding.Type, finding.Severity, finding.Description, finding.Snippet))
                .Take(MaximumFindings).ToList();
        }

        private IEnumerable<SecurityFindingResult> ScanContainer(OpenXmlElement container, string location)
        {
            foreach (var paragraph in container.Descendants<Paragraph>())
            {
                var paragraphFill = paragraph.ParagraphProperties?.Shading?.Fill?.Value;
                var cellFill = paragraph.Ancestors<TableCell>().FirstOrDefault()?.TableCellProperties?.Shading?.Fill?.Value;

                var runStyles = paragraph.Descendants<Run>()
                    .Where(run => ReferenceEquals(run.Ancestors<Paragraph>().FirstOrDefault(), paragraph))
                    .Select(run => ToRunStyleInfo(run, paragraphFill, cellFill)).ToList();

                foreach (var finding in _wordHiddenTextDetector.Detect(location, runStyles))
                    yield return finding;
            }
        }

        private static WordRunStyleInfo ToRunStyleInfo(Run run, string? paragraphFill, string? cellFill)
        {
            var runProperties = run.RunProperties;

            var isVanished = runProperties?.Vanish != null
                             && (runProperties.Vanish.Val == null || runProperties.Vanish.Val.Value);

            return new WordRunStyleInfo(
                ReadText(run),
                isVanished,
                TryReadPointSize(runProperties?.FontSize?.Val?.Value),
                runProperties?.Color?.Val?.Value,
                runProperties?.Shading?.Fill?.Value ?? paragraphFill ?? cellFill);
        }

        private static string ReadText(Run run)
        {
            var builder = new StringBuilder();

            foreach (var child in run.ChildElements)
            {
                switch (child)
                {
                    case Text text:
                        builder.Append(text.Text);
                        break;
                    case TabChar:
                    case Break:
                    case CarriageReturn:
                        builder.Append(' ');
                        break;
                }
            }

            return builder.ToString();
        }

        private static double? TryReadPointSize(string? halfPoints)
        {
            return double.TryParse(halfPoints, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                ? value / 2.0
                : null;
        }
    }
}
