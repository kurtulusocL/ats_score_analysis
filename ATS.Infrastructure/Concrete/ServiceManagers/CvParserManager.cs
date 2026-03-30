using System.Text;
using ATS.Application.Abstract.Services;
using DocumentFormat.OpenXml.Packaging;
using Serilog;
using UglyToad.PdfPig;

namespace ATS.Infrastructure.Concrete.ServiceManagers
{
    public class CvParserManager : ICvParserService
    {
        private readonly ILogger _logger = Log.ForContext<CvParserManager>();

        public async Task<string> ParseAsync(string filePath, string fileType)
        {
            _logger.Information("Parsing CV file. Path: {FilePath}, Type: {FileType}", filePath, fileType);

            try
            {
                var text = fileType.ToLowerInvariant() switch
                {
                    "pdf" => await ParsePdfAsync(filePath),
                    "docx" => await ParseDocxAsync(filePath),
                    "txt" => await ParseTxtAsync(filePath),
                    _ => throw new NotSupportedException($"File type '{fileType}' is not supported.")
                };

                _logger.Information("CV parsed successfully. Characters extracted: {CharCount}", text.Length);
                return text;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to parse CV file. Path: {FilePath}", filePath);
                throw;
            }
        }

        private Task<string> ParsePdfAsync(string filePath)
        {
            var sb = new StringBuilder();

            using var document = PdfDocument.Open(filePath);
            foreach (var page in document.GetPages())
                sb.AppendLine(page.Text);

            return Task.FromResult(sb.ToString());

        }

        private Task<string> ParseDocxAsync(string filePath)
        {
            var sb = new StringBuilder();

            using var doc = WordprocessingDocument.Open(filePath, false);
            var body = doc.MainDocumentPart?.Document?.Body;

            if (body == null)
                return Task.FromResult(string.Empty);

            foreach (var paragraph in body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                sb.AppendLine(paragraph.InnerText);

            return Task.FromResult(sb.ToString());
        }

        private async Task<string> ParseTxtAsync(string filePath)
        {
            return await File.ReadAllTextAsync(filePath);
        }
    }
}
