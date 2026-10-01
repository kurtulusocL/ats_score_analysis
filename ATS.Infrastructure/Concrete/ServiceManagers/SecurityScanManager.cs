using ATS.Application.Abstract.Services;
using ATS.Application.Results;
using ATS.Application.Security;
using ATS.Domain.Enums;
using Serilog;

namespace ATS.Infrastructure.Concrete.ServiceManagers
{
    public class SecurityScanManager : ISecurityScanService
    {
        private const int MaximumFindings = 100;

        private readonly IEnumerable<IHiddenTextFileScanner> _hiddenTextFileScanners;
        private readonly InstructionPatternDetector _instructionPatternDetector;
        private readonly ILogger _logger = Log.ForContext<SecurityScanManager>();

        public SecurityScanManager(IEnumerable<IHiddenTextFileScanner> hiddenTextFileScanners, InstructionPatternDetector instructionPatternDetector)
        {
            _hiddenTextFileScanners = hiddenTextFileScanners;
            _instructionPatternDetector = instructionPatternDetector;
        }

        public Task<SecurityScanResult> ScanAsync(string filePath, string fileType, string extractedText, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var warnings = new List<string>();
            var hiddenTextFindings = ScanHiddenText(filePath, fileType, warnings);

            cancellationToken.ThrowIfCancellationRequested();

            var instructionFindings = _instructionPatternDetector.Detect(extractedText ?? string.Empty).ToList();
            RemoveInstructionsAlreadyReportedAsHiddenText(hiddenTextFindings, instructionFindings);

            var findings = hiddenTextFindings
                .Concat(instructionFindings.OrderByDescending(finding => finding.Severity).Take(MaximumFindings)).OrderByDescending(finding => finding.Severity).ToList();

            _logger.Information(
                "Security scan completed. File: {FilePath}, Findings: {FindingCount}, High: {HighCount}, Warnings: {WarningCount}",
                filePath, findings.Count, findings.Count(finding => finding.Severity == SecurityFindingSeverity.High), warnings.Count);

            return Task.FromResult(new SecurityScanResult(findings, warnings));
        }

        private IReadOnlyList<SecurityFindingResult> ScanHiddenText(string filePath, string fileType, List<string> warnings)
        {
            var hiddenTextFileScanner = _hiddenTextFileScanners.FirstOrDefault(scanner =>
                string.Equals(scanner.FileType, fileType, StringComparison.OrdinalIgnoreCase));

            if (hiddenTextFileScanner == null)
                return Array.Empty<SecurityFindingResult>();

            try
            {
                return hiddenTextFileScanner.ScanFile(filePath);
            }
            catch (Exception exception)
            {
                _logger.Warning(exception, "Hidden text scan failed. File: {FilePath}", filePath);
                warnings.Add($"The hidden text scan of the {fileType.ToUpperInvariant()} file could not be completed, so hidden text was not checked.");
                return Array.Empty<SecurityFindingResult>();
            }
        }

        private void RemoveInstructionsAlreadyReportedAsHiddenText(IReadOnlyList<SecurityFindingResult> hiddenTextFindings, List<SecurityFindingResult> instructionFindings)
        {
            foreach (var hiddenTextFinding in hiddenTextFindings.Where(finding => finding.Severity == SecurityFindingSeverity.High))
            {
                foreach (var containedInstruction in _instructionPatternDetector.Detect(hiddenTextFinding.Snippet ?? string.Empty))
                {
                    var containedSnippet = NormalizeWhitespace(containedInstruction.Snippet);
                    var duplicateIndex = instructionFindings.FindIndex(finding =>
                        string.Equals(NormalizeWhitespace(finding.Snippet), containedSnippet, StringComparison.OrdinalIgnoreCase));

                    if (duplicateIndex >= 0)
                        instructionFindings.RemoveAt(duplicateIndex);
                }
            }
        }

        public Task<SecurityScanResult> ScanJobPostingAsync(string jobPostingText, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var findings = _instructionPatternDetector.Detect(jobPostingText ?? string.Empty)
                .Select(finding => new SecurityFindingResult(
                    SecurityFindingType.JobPostingInstructionPattern, finding.Severity, finding.Description, finding.Snippet))
                .OrderByDescending(finding => finding.Severity)
                .Take(MaximumFindings)
                .ToList();

            _logger.Information(
                "Job posting security scan completed. Findings: {FindingCount}, High: {HighCount}",
                findings.Count, findings.Count(finding => finding.Severity == SecurityFindingSeverity.High));

            return Task.FromResult(new SecurityScanResult(findings, Array.Empty<string>()));
        }

        private static string NormalizeWhitespace(string? text) =>
            string.Join(' ', (text ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
