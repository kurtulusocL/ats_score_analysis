using ATS.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ATS.Infrastructure.Concrete.Reports
{
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

        public DocumentMetadata GetMetadata() => new()
        {
            Title = $"ATS Score Report — {_cvScan.CandidateName}",
            Author = "ATS Analyzer",
            CreationDate = DateTimeOffset.UtcNow
        };

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(t => t.FontSize(10).FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Background(_primaryColor).Padding(20).Row(row =>
                {
                    row.RelativeItem().Column(inner =>
                    {
                        inner.Item().Text("ATS Score Report").FontSize(22).Bold().FontColor(Colors.White);
                        inner.Item().Text(_reportType == "general" ? "General Analysis" : "Job Match Analysis").FontSize(12).FontColor("#BDC3C7");
                    });

                    row.ConstantItem(120).AlignRight().AlignMiddle().Column(inner =>
                    {
                        inner.Item().Text($"{_cvScan.OverallScore}").FontSize(42).Bold().FontColor(GetScoreColor(_cvScan.OverallScore));
                        inner.Item().Text("/ 100").FontSize(14).FontColor("#BDC3C7").AlignRight();
                    });
                });

                col.Item().Background(_lightGray).Padding(12).Row(row =>
                {
                    row.RelativeItem().Column(inner =>
                    {
                        inner.Item().Text(t =>
                        {
                            t.Span("Candidate: ").Bold();
                            t.Span(_cvScan.CandidateName);
                        });
                    });

                    row.RelativeItem().Column(inner =>
                    {
                        inner.Item().Text(t =>
                        {
                            t.Span("Date: ").Bold();
                            t.Span(_cvScan.CreatedAt.ToString("dd MMM yyyy HH:mm"));
                        });
                    });

                    row.RelativeItem().Column(inner =>
                    {
                        inner.Item().Text(t =>
                        {
                            t.Span("File Type: ").Bold();
                            t.Span(_cvScan.FileType.ToUpperInvariant());
                        });
                    });

                    row.RelativeItem().Column(inner =>
                    {
                        inner.Item().Text(t =>
                        {
                            t.Span("Status: ").Bold();
                            t.Span(_cvScan.OverallScore >= 50 ? "PASSED" : "NEEDS IMPROVEMENT")
                             .FontColor(_cvScan.OverallScore >= 50 ? _successColor : _dangerColor)
                             .Bold();
                        });
                    });
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.Column(col =>
            {
                col.Spacing(16);
                col.Item().Element(ComposeScoreBar);

                if (_reportType == "jobmatch" && _cvScan.JobPosting != null)
                    col.Item().Element(ComposeJobPostingInfo);

                col.Item().Element(ComposeSectionScores);
                col.Item().Element(ComposeFeedbackSection);
            });
        }

        private void ComposeScoreBar(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Overall Score").FontSize(14).Bold().FontColor(_primaryColor);
                col.Item().Height(4).Background(_borderColor);
                col.Item().Height(8);

                col.Item().Row(row =>
                {
                    var scoreWidth = (float)(_cvScan.OverallScore / 100.0);
                    row.RelativeItem((float)scoreWidth).Height(24).Background(GetScoreColor(_cvScan.OverallScore));

                    if (scoreWidth < 1)
                        row.RelativeItem(1 - scoreWidth).Height(24).Background(_borderColor);
                });

                col.Item().Height(4);
                col.Item().Text($"{_cvScan.OverallScore} / 100 points").FontSize(10).FontColor("#7F8C8D");
            });
        }

        private void ComposeJobPostingInfo(IContainer container)
        {
            container.Border(1).BorderColor(_accentColor).Padding(12)
                     .Column(col =>
                     {
                         col.Item().Text("Job Posting").FontSize(14).Bold().FontColor(_primaryColor);
                         col.Item().Height(6);
                         col.Item().Text(t =>
                         {
                             t.Span("Position: ").Bold();
                             t.Span(_cvScan.JobPosting!.Title);
                         });
                         col.Item().Text(t =>
                         {
                             t.Span("Match Score: ").Bold();
                             t.Span($"{_cvScan.JobPosting!.MatchScore} / 20")
                              .FontColor(GetScoreColor(_cvScan.JobPosting!.MatchScore * 5));
                         });
                     });
        }

        private void ComposeSectionScores(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Section Breakdown").FontSize(14).Bold().FontColor(_primaryColor);
                col.Item().Height(4).Background(_borderColor);
                col.Item().Height(8);

                foreach (var section in _cvScan.SectionScores)
                {
                    col.Item().Element(c => ComposeSectionRow(c, section));
                    col.Item().Height(6);
                }
            });
        }

        private void ComposeSectionRow(IContainer container, SectionScore section)
        {
            var percentage = section.MaxScore > 0
                ? (float)section.Score / section.MaxScore
                : 0f;

            container.Background(_lightGray).Padding(10).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text(section.SectionName).Bold();
                    row.ConstantItem(80).AlignRight().Text(t =>
                    {
                        t.Span($"{section.Score}/{section.MaxScore} pts")
                         .FontColor(section.IsPassed ? _successColor : _dangerColor)
                         .Bold();
                    });
                    row.ConstantItem(60).AlignRight().Text(section.IsPassed ? "✓ PASS" : "✗ FAIL").FontColor(section.IsPassed ? _successColor : _dangerColor).Bold();
                });

                col.Item().Height(4);
                col.Item().Row(row =>
                {
                    if (percentage > 0)
                    {
                        row.RelativeItem((float)percentage).Height(6).Background(section.IsPassed ? _successColor : _warningColor);
                    }
                    if (percentage < 1)
                    {
                        row.RelativeItem(1 - (float)percentage).Height(6).Background(_borderColor);
                    }
                });
            });
        }

        private void ComposeFeedbackSection(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Detailed Feedback").FontSize(14).Bold().FontColor(_primaryColor);
                col.Item().Height(4).Background(_borderColor);
                col.Item().Height(8);

                foreach (var section in _cvScan.SectionScores)
                {
                    if (string.IsNullOrWhiteSpace(section.Feedback)) continue;

                    col.Item().Column(inner =>
                    {
                        inner.Item().Text(section.SectionName).FontSize(11).Bold().FontColor(_accentColor);
                        inner.Item().Height(4);

                        var parts = section.Feedback.Split('|', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var part in parts)
                        {
                            var isMissing = part.StartsWith("MISSING:");
                            var isPositive = part.StartsWith("✓");
                            var displayText = isMissing ? part.Replace("MISSING:", "✗ Missing:") : part;

                            inner.Item().Row(row =>
                            {
                                row.ConstantItem(8);
                                row.RelativeItem().Text(displayText).FontColor(isMissing ? _dangerColor : isPositive ? _successColor : _warningColor);
                            });
                            inner.Item().Height(2);
                        }
                        inner.Item().Height(8);
                    });
                }
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.BorderTop(1).BorderColor(_borderColor).Padding(8).Row(row =>
            {
                row.RelativeItem().Text("Generated by ATS Analyzer").FontSize(8).FontColor("#95A5A6");

                row.RelativeItem().AlignRight()
                .Text(x =>
                   {
                       x.Span("Page ").FontSize(8).FontColor("#95A5A6");
                       x.CurrentPageNumber().FontSize(8).FontColor("#95A5A6");
                       x.Span(" of ").FontSize(8).FontColor("#95A5A6");
                       x.TotalPages().FontSize(8).FontColor("#95A5A6");
                   });
            });
        }

        private string GetScoreColor(int score) => score switch
        {
            >= 80 => _successColor,
            >= 50 => _warningColor,
            _ => _dangerColor
        };
    }
}
