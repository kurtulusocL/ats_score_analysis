using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ATS.Tests.TestSupport
{
    public class TestDocumentFactory
    {
        public static byte[] CreatePdf(Action<ColumnDescriptor> content)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container => container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.Content().Column(content);
            })).GeneratePdf();
        }

        public static byte[] CreateDocx(Action<DocumentFormat.OpenXml.Wordprocessing.Body> buildBody)
        {
            using var stream = new MemoryStream();

            using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
            {
                var mainPart = document.AddMainDocumentPart();
                mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new DocumentFormat.OpenXml.Wordprocessing.Body());
                buildBody(mainPart.Document.Body!);
                mainPart.Document.Save();
            }

            return stream.ToArray();
        }
    }
}
