using ATS.Application.Security;
using ATS.Domain.Entities;
using ATS.Domain.Enums;

namespace ATS.Tests
{
    public class SecurityReportLineBuilderTests
    {
        private static SecurityFinding CreateFinding(
            SecurityFindingSeverity severity, string snippet,
            SecurityFindingType type = SecurityFindingType.InstructionPattern, string description = "description")
            => new() { Type = type, Severity = severity, Description = description, Snippet = snippet };

        [Fact]
        public void BuildFindingLines_ReturnsEmpty_WhenFindingsAreNull()
        {
            Assert.Empty(SecurityReportLineBuilder.BuildFindingLines(null));
        }

        [Fact]
        public void BuildFindingLines_ReturnsEmpty_WhenThereAreNoFindings()
        {
            Assert.Empty(SecurityReportLineBuilder.BuildFindingLines(Array.Empty<SecurityFinding>()));
        }

        [Fact]
        public void BuildFindingLines_PutsHighBeforeLow_AndKeepsOrderWithinTheSameSeverity()
        {
            var findings = new[]
            {
                CreateFinding(SecurityFindingSeverity.Low, "low-1"),
                CreateFinding(SecurityFindingSeverity.High, "high-1"),
                CreateFinding(SecurityFindingSeverity.Low, "low-2"),
                CreateFinding(SecurityFindingSeverity.High, "high-2")
            };

            var lines = SecurityReportLineBuilder.BuildFindingLines(findings);

            Assert.Equal(4, lines.Count);
            Assert.Collection(lines,
                line => { Assert.Equal(SecurityFindingSeverity.High, line.Severity); Assert.Contains("high-1", line.Text); },
                line => { Assert.Equal(SecurityFindingSeverity.High, line.Severity); Assert.Contains("high-2", line.Text); },
                line => { Assert.Equal(SecurityFindingSeverity.Low, line.Severity); Assert.Contains("low-1", line.Text); },
                line => { Assert.Equal(SecurityFindingSeverity.Low, line.Severity); Assert.Contains("low-2", line.Text); });
        }

        [Fact]
        public void BuildFindingLines_PrefixesTheTypeLabel()
        {
            var lines = SecurityReportLineBuilder.BuildFindingLines(
                new[] { CreateFinding(SecurityFindingSeverity.High, "first", SecurityFindingType.InstructionPattern) });

            Assert.Equal("Instruction-like text: first", Assert.Single(lines).Text);
        }

        [Fact]
        public void BuildFindingLines_LeavesOutHiddenTextFindings()
        {
            var findings = new[]
            {
                CreateFinding(SecurityFindingSeverity.High, "hidden words", SecurityFindingType.HiddenText),
                CreateFinding(SecurityFindingSeverity.High, "visible instruction", SecurityFindingType.InstructionPattern)
            };

            var lines = SecurityReportLineBuilder.BuildFindingLines(findings);

            Assert.Equal("Instruction-like text: visible instruction", Assert.Single(lines).Text);
        }

        [Fact]
        public void BuildFindingLines_TruncatesLongSnippets()
        {
            var lines = SecurityReportLineBuilder.BuildFindingLines(new[] { CreateFinding(SecurityFindingSeverity.High, new string('a', 300)) });

            var text = Assert.Single(lines).Text;
            var typeLabelPrefix = $"{SecurityFindingMessageFormatter.GetTypeLabel(SecurityFindingType.InstructionPattern)}: ";

            Assert.Equal(typeLabelPrefix.Length + SecurityFindingTextShortener.MaximumLength, text.Length);
            Assert.EndsWith("...", text);
        }

        [Fact]
        public void BuildFindingLines_FallsBackToDescription_WhenSnippetIsEmpty()
        {
            var lines = SecurityReportLineBuilder.BuildFindingLines(
                new[] { CreateFinding(SecurityFindingSeverity.High, string.Empty, description: "Hidden text was found") });

            Assert.Contains("Hidden text was found", Assert.Single(lines).Text);
        }

        [Fact]
        public void BuildFindingLines_CollapsesLineBreaksIntoASingleLine()
        {
            var lines = SecurityReportLineBuilder.BuildFindingLines(
                new[] { CreateFinding(SecurityFindingSeverity.High, "line one\r\nline two") });

            Assert.Contains("line one line two", Assert.Single(lines).Text);
        }

        [Fact]
        public void BuildFindingLines_LabelsAJobPostingFindingAsComingFromTheJobPosting()
        {
            var lines = SecurityReportLineBuilder.BuildFindingLines(
                new[] { CreateFinding(SecurityFindingSeverity.High, "snippet", SecurityFindingType.JobPostingInstructionPattern) });

            Assert.Equal("Instruction-like text in job posting: snippet", Assert.Single(lines).Text);
        }

        [Fact]
        public void BuildHiddenTextLines_ReturnsEmpty_WhenFindingsAreNull()
        {
            Assert.Empty(SecurityReportLineBuilder.BuildHiddenTextLines(null));
        }

        [Fact]
        public void BuildHiddenTextLines_ReturnsOnlyHiddenTextFindings()
        {
            var findings = new[]
            {
                CreateFinding(SecurityFindingSeverity.High, "visible instruction", SecurityFindingType.InstructionPattern),
                CreateFinding(SecurityFindingSeverity.Low, "hidden words", SecurityFindingType.HiddenText),
                CreateFinding(SecurityFindingSeverity.High, "posting instruction", SecurityFindingType.JobPostingInstructionPattern)
            };

            var lines = SecurityReportLineBuilder.BuildHiddenTextLines(findings);

            Assert.Contains("hidden words", Assert.Single(lines).Text);
        }

        [Fact]
        public void BuildHiddenTextLines_KeepsTheFullTextWithoutTruncating()
        {
            var fullText = string.Join(" ", Enumerable.Repeat("keyword", 300));

            var lines = SecurityReportLineBuilder.BuildHiddenTextLines(
                new[] { CreateFinding(SecurityFindingSeverity.Low, fullText, SecurityFindingType.HiddenText) });

            Assert.EndsWith(fullText, Assert.Single(lines).Text);
        }

        [Fact]
        public void BuildHiddenTextLines_StartsWithTheDescriptionThenTheText()
        {
            var lines = SecurityReportLineBuilder.BuildHiddenTextLines(new[]
            {
                CreateFinding(SecurityFindingSeverity.Low, "secret words", SecurityFindingType.HiddenText,
                    "Hidden text (white or near-white text) on page 1")
            });

            Assert.Equal("Hidden text (white or near-white text) on page 1: secret words", Assert.Single(lines).Text);
        }

        [Fact]
        public void BuildHiddenTextLines_UsesOnlyTheDescription_WhenTheTextIsEmpty()
        {
            var lines = SecurityReportLineBuilder.BuildHiddenTextLines(new[]
            {
                CreateFinding(SecurityFindingSeverity.Low, string.Empty, SecurityFindingType.HiddenText, "Hidden text on page 2")
            });

            Assert.Equal("Hidden text on page 2", Assert.Single(lines).Text);
        }

        [Fact]
        public void BuildHiddenTextLines_PutsHighBeforeLow_AndKeepsOrderWithinTheSameSeverity()
        {
            var findings = new[]
            {
                CreateFinding(SecurityFindingSeverity.Low, "low-1", SecurityFindingType.HiddenText),
                CreateFinding(SecurityFindingSeverity.High, "high-1", SecurityFindingType.HiddenText),
                CreateFinding(SecurityFindingSeverity.Low, "low-2", SecurityFindingType.HiddenText)
            };

            var lines = SecurityReportLineBuilder.BuildHiddenTextLines(findings);

            Assert.Collection(lines,
                line => Assert.Contains("high-1", line.Text),
                line => Assert.Contains("low-1", line.Text),
                line => Assert.Contains("low-2", line.Text));
        }

        [Fact]
        public void BuildWarningLines_ReturnsEmpty_WhenWarningsAreNull()
        {
            Assert.Empty(SecurityReportLineBuilder.BuildWarningLines(null));
        }

        [Fact]
        public void BuildWarningLines_SkipsBlankMessages_AndKeepsOrder()
        {
            var warnings = new[]
            {
                new AnalysisWarning { Message = "first" },
                new AnalysisWarning { Message = "   " },
                new AnalysisWarning { Message = "second" }
            };

            var lines = SecurityReportLineBuilder.BuildWarningLines(warnings);

            Assert.Equal(new[] { "first", "second" }, lines);
        }
    }
}
