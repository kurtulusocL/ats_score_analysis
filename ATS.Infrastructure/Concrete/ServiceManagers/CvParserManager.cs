using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ATS.Application.Abstract.Services;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Serilog;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ATS.Infrastructure.Concrete.ServiceManagers;

public class CvParserManager : ICvParserService
{
	private readonly ILogger _logger = Log.ForContext<CvParserManager>();

	public async Task<string> ParseAsync(string filePath, string fileType, CancellationToken cancellationToken = default(CancellationToken))
	{
		_logger.Information("Parsing CV file. Path: {FilePath}, Type: {FileType}", filePath, fileType);
		try
		{
			string text = fileType.ToLowerInvariant();
			if (1 == 0)
			{
			}
			string text2 = text switch
			{
				"pdf" => await ParsePdfAsync(filePath, cancellationToken), 
				"docx" => await ParseDocxAsync(filePath, cancellationToken), 
				"txt" => await File.ReadAllTextAsync(filePath, cancellationToken), 
				_ => throw new NotSupportedException("File type '" + fileType + "' is not supported."), 
			};
			if (1 == 0)
			{
			}
			string text3 = text2;
			_logger.Information("CV parsed successfully. Characters extracted: {CharCount}", text3.Length);
			return text3;
		}
		catch (OperationCanceledException)
		{
			_logger.Warning("CV parsing cancelled. Path: {FilePath}", filePath);
			throw;
		}
		catch (Exception ex2)
		{
			Exception ex3 = ex2;
			_logger.Error(ex3, "Failed to parse CV file. Path: {FilePath}", filePath);
			throw;
		}
	}

	private Task<string> ParsePdfAsync(string filePath, CancellationToken cancellationToken)
	{
		StringBuilder sb = new StringBuilder();
		using PdfDocument pdfDocument = PdfDocument.Open(filePath);
		foreach (Page page in pdfDocument.GetPages())
		{
			cancellationToken.ThrowIfCancellationRequested();
			List<Word> list = (from w in page.GetWords()
				orderby w.BoundingBox.Bottom descending, w.BoundingBox.Left
				select w).ToList();
			List<Word> line = new List<Word>();
			double num = 0.0;
			foreach (Word item in list)
			{
				double num2 = Math.Max(2.0, item.BoundingBox.Height * 0.5);
				if (line.Count > 0 && Math.Abs(item.BoundingBox.Bottom - num) > num2)
				{
					FlushLine();
				}
				if (line.Count == 0)
				{
					num = item.BoundingBox.Bottom;
				}
				line.Add(item);
			}
			FlushLine();
			void FlushLine()
			{
				if (line.Count != 0)
				{
					sb.AppendLine(string.Join(" ", from w in line
						orderby w.BoundingBox.Left
						select w.Text));
					line.Clear();
				}
			}
		}
		return Task.FromResult(sb.ToString());
	}

	private Task<string> ParseDocxAsync(string filePath, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		StringBuilder stringBuilder = new StringBuilder();
		using WordprocessingDocument wordprocessingDocument = WordprocessingDocument.Open(filePath, isEditable: false);
		Body body = wordprocessingDocument.MainDocumentPart?.Document?.Body;
		if (body == null)
		{
			return Task.FromResult(string.Empty);
		}
		foreach (Paragraph item in body.Elements<Paragraph>())
		{
			stringBuilder.AppendLine(item.InnerText);
		}
		return Task.FromResult(stringBuilder.ToString());
	}
}
