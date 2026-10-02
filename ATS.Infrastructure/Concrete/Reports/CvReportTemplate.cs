using System;
using System.Collections.Generic;
using System.Linq;
using ATS.Application.Reporting;
using ATS.Application.Results;
using ATS.Application.Security;
using ATS.Domain.Entities;
using ATS.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ATS.Infrastructure.Concrete.Reports;

public class CvReportTemplate : IDocument
{
	private readonly CvScan _cvScan;

	private readonly string _reportType;

	private static readonly string _primaryColor = "#2C3E50";

	private static readonly string _accentColor = "#2980B9";

	private static readonly string _successColor = "#27AE60";

	private static readonly string _warningColor = "#E67E22";

	private static readonly string _dangerColor = "#E74C3C";

	private static readonly string _lightGray = "#F5F5F5";

	private static readonly string _borderColor = "#DCDCDC";

	public CvReportTemplate(CvScan cvScan, string reportType)
	{
		_cvScan = cvScan;
		_reportType = reportType;
	}

	public DocumentMetadata GetMetadata()
	{
		return new DocumentMetadata
		{
			Title = "ATS Score Report — " + _cvScan.CandidateName,
			Author = "ATS Analyzer",
			CreationDate = DateTimeOffset.UtcNow
		};
	}

	public void Compose(IDocumentContainer container)
	{
		container.Page((PageDescriptor page) =>
		{
			page.Size(PageSizes.A4);
			page.Margin(36f);
			page.DefaultTextStyle((TextStyle t) => t.FontSize(10f).FontFamily("Arial"));
			page.Header().Element((Action<IContainer>)ComposeHeader, "ComposeHeader", "Compose", "C:\\Users\\kurtu\\Videos\\Code Archive\\C#\\C# Projects\\ATS\\ATS.Infrastructure\\Concrete\\Reports\\CvReportTemplate.cs", 46);
			page.Content().Element((Action<IContainer>)ComposeContent, "ComposeContent", "Compose", "C:\\Users\\kurtu\\Videos\\Code Archive\\C#\\C# Projects\\ATS\\ATS.Infrastructure\\Concrete\\Reports\\CvReportTemplate.cs", 47);
			page.Footer().Element((Action<IContainer>)ComposeFooter, "ComposeFooter", "Compose", "C:\\Users\\kurtu\\Videos\\Code Archive\\C#\\C# Projects\\ATS\\ATS.Infrastructure\\Concrete\\Reports\\CvReportTemplate.cs", 48);
		});
	}

	private void ComposeHeader(IContainer container)
	{
		container.Column((ColumnDescriptor col) =>
		{
			col.Item().Background(_primaryColor).Padding(20f)
				.Row((RowDescriptor row) =>
				{
					row.RelativeItem().Column((ColumnDescriptor inner) =>
					{
						inner.Item().Text("ATS Score Report").FontSize(22f)
							.Bold()
							.FontColor(Colors.White);
						inner.Item().Text((_reportType == "general") ? "General Analysis" : "Job Match Analysis").FontSize(12f)
							.FontColor("#BDC3C7");
					});
					row.ConstantItem(120f).AlignRight().AlignMiddle()
						.Column((ColumnDescriptor inner) =>
						{
							inner.Item().Text($"{_cvScan.OverallScore}").FontSize(42f)
								.Bold()
								.FontColor(GetScoreColor(_cvScan.OverallScore));
							inner.Item().Text("/ 100").FontSize(14f)
								.FontColor("#BDC3C7")
								.AlignRight();
						});
				});
			col.Item().Background(_lightGray).Padding(12f)
				.Row((RowDescriptor row) =>
				{
					row.RelativeItem().Column((ColumnDescriptor inner) =>
					{
						inner.Item().Text((TextDescriptor t) =>
						{
							t.Span("Candidate: ").Bold();
							t.Span(_cvScan.CandidateName);
						});
					});
					row.RelativeItem().Column((ColumnDescriptor inner) =>
					{
						inner.Item().Text((TextDescriptor t) =>
						{
							t.Span("Date: ").Bold();
							t.Span(_cvScan.CreatedAt.ToLocalTime().ToString("dd MMM yyyy HH:mm"));
						});
					});
					row.RelativeItem().Column((ColumnDescriptor inner) =>
					{
						inner.Item().Text((TextDescriptor t) =>
						{
							t.Span("File Type: ").Bold();
							t.Span(_cvScan.FileType.ToUpperInvariant());
						});
					});
					row.RelativeItem().Column((ColumnDescriptor inner) =>
					{
						inner.Item().Text((TextDescriptor t) =>
						{
							t.Span("Status: ").Bold();
							t.Span((_cvScan.OverallScore >= 75) ? "PASSED" : "NEEDS IMPROVEMENT").FontColor((_cvScan.OverallScore >= 75) ? _successColor : _dangerColor).Bold();
						});
					});
				});
		});
	}

	private void ComposeContent(IContainer container)
	{
		container.Column((ColumnDescriptor col) =>
		{
			col.Spacing(16f);
			col.Item().Element((Action<IContainer>)ComposeScoreBar, "ComposeScoreBar", "ComposeContent", "C:\\Users\\kurtu\\Videos\\Code Archive\\C#\\C# Projects\\ATS\\ATS.Infrastructure\\Concrete\\Reports\\CvReportTemplate.cs", 119);
			if (_reportType == "jobmatch" && _cvScan.JobPosting != null)
			{
				col.Item().Element((Action<IContainer>)ComposeJobPostingInfo, "ComposeJobPostingInfo", "ComposeContent", "C:\\Users\\kurtu\\Videos\\Code Archive\\C#\\C# Projects\\ATS\\ATS.Infrastructure\\Concrete\\Reports\\CvReportTemplate.cs", 122);
			}
			col.Item().Element((Action<IContainer>)ComposeSectionScores, "ComposeSectionScores", "ComposeContent", "C:\\Users\\kurtu\\Videos\\Code Archive\\C#\\C# Projects\\ATS\\ATS.Infrastructure\\Concrete\\Reports\\CvReportTemplate.cs", 124);
			col.Item().Element((Action<IContainer>)ComposeFeedbackSection, "ComposeFeedbackSection", "ComposeContent", "C:\\Users\\kurtu\\Videos\\Code Archive\\C#\\C# Projects\\ATS\\ATS.Infrastructure\\Concrete\\Reports\\CvReportTemplate.cs", 125);
			RequirementReport requirementReport = RequirementReportBuilder.Build(_cvScan);
			if (requirementReport.HasRows)
			{
				col.Item().Element((IContainer element) =>
				{
					ComposeRequirementSection(element, requirementReport);
				}, "element => ComposeRequirementSection(element, requirementReport)", "ComposeContent", "C:\\Users\\kurtu\\Videos\\Code Archive\\C#\\C# Projects\\ATS\\ATS.Infrastructure\\Concrete\\Reports\\CvReportTemplate.cs", 129);
			}
			IReadOnlyList<SecurityReportLine> hiddenTextLines = SecurityReportLineBuilder.BuildHiddenTextLines(_cvScan.SecurityFindings);
			IReadOnlyList<SecurityReportLine> securityLines = SecurityReportLineBuilder.BuildFindingLines(_cvScan.SecurityFindings);
			IReadOnlyList<string> warningLines = SecurityReportLineBuilder.BuildWarningLines(_cvScan.AnalysisWarnings);
			if (hiddenTextLines.Count > 0)
			{
				col.Item().Element((IContainer element) =>
				{
					ComposeHiddenTextSection(element, hiddenTextLines);
				}, "element => ComposeHiddenTextSection(element, hiddenTextLines)", "ComposeContent", "C:\\Users\\kurtu\\Videos\\Code Archive\\C#\\C# Projects\\ATS\\ATS.Infrastructure\\Concrete\\Reports\\CvReportTemplate.cs", 136);
			}
			if (securityLines.Count > 0 || warningLines.Count > 0)
			{
				col.Item().Element((IContainer element) =>
				{
					ComposeSecuritySection(element, securityLines, warningLines);
				}, "element => ComposeSecuritySection(element, securityLines, warningLines)", "ComposeContent", "C:\\Users\\kurtu\\Videos\\Code Archive\\C#\\C# Projects\\ATS\\ATS.Infrastructure\\Concrete\\Reports\\CvReportTemplate.cs", 139);
			}
		});
	}

	private void ComposeScoreBar(IContainer container)
	{
		container.Column((ColumnDescriptor col) =>
		{
			col.Item().Text("Overall Score").FontSize(14f)
				.Bold()
				.FontColor(_primaryColor);
			col.Item().Height(4f).Background(_borderColor);
			col.Item().Height(8f);
			col.Item().Row((RowDescriptor row) =>
			{
				float num = (float)((double)_cvScan.OverallScore / 100.0);
				row.RelativeItem(num).Height(24f).Background(GetScoreColor(_cvScan.OverallScore));
				if (num < 1f)
				{
					row.RelativeItem(1f - num).Height(24f).Background(_borderColor);
				}
			});
			col.Item().Height(4f);
			col.Item().Text($"{_cvScan.OverallScore} / 100 points").FontSize(10f)
				.FontColor("#7F8C8D");
			if (_cvScan.IsJobMatched)
			{
				col.Item().Text(OverallScoreExplanation.Build()).FontSize(9f)
					.FontColor("#7F8C8D");
			}
		});
	}

	private void ComposeJobPostingInfo(IContainer container)
	{
		SectionScore sectionScore = _cvScan.SectionScores.FirstOrDefault((SectionScore s) => s.SectionName == "Job Match");
		int score = sectionScore?.Score ?? 0;
		int max = sectionScore?.MaxScore ?? 20;
		container.Border(1f).BorderColor(_accentColor).Padding(12f)
			.Column((ColumnDescriptor col) =>
			{
				col.Item().Text("Job Posting").FontSize(14f)
					.Bold()
					.FontColor(_primaryColor);
				col.Item().Height(6f);
				col.Item().Text((TextDescriptor t) =>
				{
					t.Span("Position: ").Bold();
					t.Span(_cvScan.JobPosting.Title);
				});
				col.Item().Text((TextDescriptor t) =>
				{
					t.Span("Match Score: ").Bold();
					t.Span($"{score} / {max}").FontColor(GetScoreColor((max > 0) ? (score * 100 / max) : 0));
				});
			});
	}

	private void ComposeSectionScores(IContainer container)
	{
		container.Column((ColumnDescriptor col) =>
		{
			col.Item().Text("Section Breakdown").FontSize(14f)
				.Bold()
				.FontColor(_primaryColor);
			col.Item().Height(4f).Background(_borderColor);
			col.Item().Height(8f);
			foreach (SectionScore section in _cvScan.SectionScores)
			{
				col.Item().Element((IContainer c) =>
				{
					ComposeSectionRow(c, section);
				}, "c => ComposeSectionRow(c, section)", "ComposeSectionScores", "C:\\Users\\kurtu\\Videos\\Code Archive\\C#\\C# Projects\\ATS\\ATS.Infrastructure\\Concrete\\Reports\\CvReportTemplate.cs", 202);
				col.Item().Height(6f);
			}
		});
	}

	private void ComposeSectionRow(IContainer container, SectionScore section)
	{
		float percentage = ((section.MaxScore > 0) ? ((float)section.Score / (float)section.MaxScore) : 0f);
		container.Background(_lightGray).Padding(10f).Column((ColumnDescriptor col) =>
		{
			col.Item().Row((RowDescriptor row) =>
			{
				row.RelativeItem().Text(section.SectionName).Bold();
				row.ConstantItem(80f).AlignRight().Text((TextDescriptor t) =>
				{
					t.Span($"{section.Score}/{section.MaxScore} pts").FontColor(section.IsPassed ? _successColor : _dangerColor).Bold();
				});
				row.ConstantItem(60f).AlignRight().Text(section.IsPassed ? "✓ PASS" : "✗ FAIL")
					.FontColor(section.IsPassed ? _successColor : _dangerColor)
					.Bold();
			});
			col.Item().Height(4f);
			col.Item().Row((RowDescriptor row) =>
			{
				if (percentage > 0f)
				{
					row.RelativeItem(percentage).Height(6f).Background(section.IsPassed ? _successColor : _warningColor);
				}
				if (percentage < 1f)
				{
					row.RelativeItem(1f - percentage).Height(6f).Background(_borderColor);
				}
			});
		});
	}

	private void ComposeFeedbackSection(IContainer container)
	{
		container.Column((ColumnDescriptor col) =>
		{
			col.Item().Text("Detailed Feedback").FontSize(14f)
				.Bold()
				.FontColor(_primaryColor);
			col.Item().Height(4f).Background(_borderColor);
			col.Item().Height(8f);
			foreach (SectionScore section in _cvScan.SectionScores)
			{
				if (!string.IsNullOrWhiteSpace(section.Feedback))
				{
					col.Item().Column((ColumnDescriptor inner) =>
					{
						inner.Item().Text(section.SectionName).FontSize(11f)
							.Bold()
							.FontColor(_accentColor);
						inner.Item().Height(4f);
						string[] array = section.Feedback.Split('|', StringSplitOptions.RemoveEmptyEntries);
						string[] array2 = array;
						foreach (string text in array2)
						{
							bool isMissing = text.StartsWith("MISSING:");
							bool isPositive = text.StartsWith("✓");
							string displayText = (isMissing ? text.Replace("MISSING:", "✗ Missing:") : text);
							inner.Item().Row((RowDescriptor row) =>
							{
								row.ConstantItem(8f);
								TextBlockDescriptor descriptor = row.RelativeItem().Text(displayText);
								string text2 = (isMissing ? _dangerColor : (isPositive ? _successColor : _warningColor));
								descriptor.FontColor(text2);
							});
							inner.Item().Height(2f);
						}
						inner.Item().Height(8f);
					});
				}
			}
		});
	}

	private void ComposeRequirementSection(IContainer container, RequirementReport requirementReport)
	{
		container.Column((ColumnDescriptor col) =>
		{
			col.Spacing(6f);
			col.Item().Text("Requirement analysis").FontSize(14f)
				.Bold()
				.FontColor(_primaryColor);
			col.Item().Height(4f).Background(_borderColor);
			if (requirementReport.ScoreComponentsText != null)
			{
				col.Item().Text(requirementReport.ScoreComponentsText).FontSize(10f)
					.Bold()
					.FontColor(_accentColor);
			}
			if (requirementReport.HasInterpretations)
			{
				col.Item().Text(RequirementReportTexts.BuildModelNote(requirementReport.ModelIdentity)).FontSize(9f)
					.FontColor("#7F8C8D");
			}
			foreach (RequirementReportRow row in requirementReport.Rows)
			{
				col.Item().Element((IContainer element) =>
				{
					ComposeRequirementRow(element, row);
				}, "element => ComposeRequirementRow(element, row)", "ComposeRequirementSection", "C:\\Users\\kurtu\\Videos\\Code Archive\\C#\\C# Projects\\ATS\\ATS.Infrastructure\\Concrete\\Reports\\CvReportTemplate.cs", 296);
			}
		});
	}

	private void ComposeRequirementRow(IContainer container, RequirementReportRow row)
	{
		container.Background(_lightGray).Padding(10f).Column((ColumnDescriptor col) =>
		{
			col.Spacing(3f);
			col.Item().Row((RowDescriptor headerRow) =>
			{
				headerRow.RelativeItem().Text(row.RequirementName).Bold();
				headerRow.ConstantItem(70f).AlignRight().Text(row.PriorityText)
					.FontSize(9f)
					.FontColor("#7F8C8D");
			});
			col.Item().Text((TextDescriptor text) =>
			{
				text.Span("Embedding match: ").FontSize(9f);
				text.Span(row.SemanticStatus.ToString()).FontSize(9f).Bold()
					.FontColor(GetMatchStatusColor(row.SemanticStatus));
				text.Span(" (similarity " + row.SimilarityText + ")").FontSize(9f).FontColor("#7F8C8D");
				if (row.ModelStatus.HasValue)
				{
					text.Span("     Language model: ").FontSize(9f);
					text.Span(row.ModelStatus.Value.ToString()).FontSize(9f).Bold()
						.FontColor(GetMatchStatusColor(row.ModelStatus.Value));
				}
			});
			if (!string.IsNullOrWhiteSpace(row.Explanation))
			{
				col.Item().Text(row.Explanation);
			}
			if (!string.IsNullOrWhiteSpace(row.EvidenceQuote))
			{
				col.Item().Text((TextDescriptor text) =>
				{
					text.Span("Evidence: ").Bold();
					text.Span("\"" + row.EvidenceQuote + "\"").Italic();
				});
			}
			if (!string.IsNullOrWhiteSpace(row.Suggestion))
			{
				col.Item().Text((TextDescriptor text) =>
				{
					text.Span("Suggestion: ").Bold();
					text.Span(row.Suggestion);
				});
			}
			if (row.HasDisagreement)
			{
				col.Item().Text("The embedding match and the language model disagree about this requirement. Check it by hand.").FontSize(9f)
					.FontColor(_warningColor);
			}
		});
	}

	private string GetMatchStatusColor(MatchStatus matchStatus)
	{
		if (1 == 0)
		{
		}
		string result = matchStatus switch
		{
			MatchStatus.Met => _successColor, 
			MatchStatus.Partial => _warningColor, 
			_ => _dangerColor, 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	private void ComposeHiddenTextSection(IContainer container, IReadOnlyList<SecurityReportLine> hiddenTextLines)
	{
		container.EnsureSpace(120f).Column((ColumnDescriptor col) =>
		{
			col.Spacing(6f);
			col.Item().Text("Hidden text in the CV file").FontSize(14f)
				.Bold()
				.FontColor(_primaryColor);
			col.Item().Height(4f).Background(_borderColor);
			col.Item().Text("This text was excluded from the score. It is listed here so the reader knows the CV contains hidden text.").FontSize(9f)
				.FontColor("#7F8C8D");
			foreach (SecurityReportLine hiddenTextLine in hiddenTextLines)
			{
				col.Item().Text($"[{hiddenTextLine.Severity}] {hiddenTextLine.Text}").FontColor(GetSeverityColor(hiddenTextLine.Severity));
			}
		});
	}

	private void ComposeSecuritySection(IContainer container, IReadOnlyList<SecurityReportLine> securityLines, IReadOnlyList<string> warningLines)
	{
		container.EnsureSpace(120f).Column((ColumnDescriptor col) =>
		{
			col.Spacing(6f);
			if (securityLines.Count > 0)
			{
				col.Item().Text("Security").FontSize(14f)
					.Bold()
					.FontColor(_primaryColor);
				col.Item().Height(4f).Background(_borderColor);
				col.Item().Text("These findings are only reported. The flagged text is still part of the analysis, so it may have influenced the score.").FontSize(9f)
					.FontColor("#7F8C8D");
				foreach (SecurityReportLine securityLine in securityLines)
				{
					col.Item().Text($"[{securityLine.Severity}] {securityLine.Text}").FontColor(GetSeverityColor(securityLine.Severity));
				}
			}
			if (warningLines.Count > 0)
			{
				col.Item().Text("Analysis Warnings").FontSize(14f)
					.Bold()
					.FontColor(_primaryColor);
				col.Item().Height(4f).Background(_borderColor);
				foreach (string warningLine in warningLines)
				{
					col.Item().Text(warningLine).FontColor(_warningColor);
				}
			}
		});
	}

	private string GetSeverityColor(SecurityFindingSeverity severity)
	{
		return (severity == SecurityFindingSeverity.High) ? _dangerColor : _warningColor;
	}

	private void ComposeFooter(IContainer container)
	{
		container.BorderTop(1f).BorderColor(_borderColor).Padding(8f)
			.Row((RowDescriptor row) =>
			{
				row.RelativeItem().Text("Generated by ATS Analyzer").FontSize(8f)
					.FontColor("#95A5A6");
				row.RelativeItem().AlignRight().Text((TextDescriptor x) =>
				{
					x.Span("Page ").FontSize(8f).FontColor("#95A5A6");
					x.CurrentPageNumber().FontSize(8f).FontColor("#95A5A6");
					x.Span(" of ").FontSize(8f).FontColor("#95A5A6");
					x.TotalPages().FontSize(8f).FontColor("#95A5A6");
				});
			});
	}

	private string GetScoreColor(int score)
	{
		if (1 == 0)
		{
		}
		string result;
		if (score >= 80)
		{
			result = _successColor;
		}
		else
		{
			result = ((score < 50) ? _dangerColor : _warningColor);
		}
		if (1 == 0)
		{
		}
		return result;
	}
}
