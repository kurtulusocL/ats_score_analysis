using ATS.Application.Abstract.Services;
using ATS.Application.Results;
using ATS.Tests.Models;

namespace ATS.Tests.TestSupport
{
    public sealed class FakeSecurityScanService : ISecurityScanService
    {
        private readonly IReadOnlyList<SecurityFindingResult> _findings;
        private readonly IReadOnlyList<string> _warnings;
        private readonly Exception? _exceptionToThrow;
        private readonly IReadOnlyList<SecurityFindingResult> _jobPostingFindings;
        private readonly Exception? _jobPostingExceptionToThrow;

        public FakeSecurityScanService(IReadOnlyList<SecurityFindingResult>? findings = null, IReadOnlyList<string>? warnings = null, Exception? exceptionToThrow = null, IReadOnlyList<SecurityFindingResult>? jobPostingFindings = null, Exception? jobPostingExceptionToThrow = null)
        {
            _findings = findings ?? Array.Empty<SecurityFindingResult>();
            _warnings = warnings ?? Array.Empty<string>();
            _exceptionToThrow = exceptionToThrow;
            _jobPostingFindings = jobPostingFindings ?? Array.Empty<SecurityFindingResult>();
            _jobPostingExceptionToThrow = jobPostingExceptionToThrow;
        }

        public SecurityScanCallRecorder Recorder { get; } = new();

        public Task<SecurityScanResult> ScanAsync(string filePath, string fileType, string extractedText, CancellationToken cancellationToken = default)
        {
            Recorder.RecordScan(extractedText);

            if (_exceptionToThrow != null)
                throw _exceptionToThrow;

            return Task.FromResult(new SecurityScanResult(_findings, _warnings));
        }

        public Task<SecurityScanResult> ScanJobPostingAsync(string jobPostingText, CancellationToken cancellationToken = default)
        {
            Recorder.RecordJobPostingScan(jobPostingText);

            if (_jobPostingExceptionToThrow != null)
                throw _jobPostingExceptionToThrow;

            return Task.FromResult(new SecurityScanResult(_jobPostingFindings, Array.Empty<string>()));
        }
    }
}
