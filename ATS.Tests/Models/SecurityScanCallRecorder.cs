

namespace ATS.Tests.Models
{
    public sealed class SecurityScanCallRecorder
    {
        public int ScanCallCount { get; private set; }
        public string? LastScannedText { get; private set; }
        public int JobPostingScanCallCount { get; private set; }
        public string? LastScannedJobPostingText { get; private set; }

        public void RecordScan(string? scannedText)
        {
            ScanCallCount++;
            LastScannedText = scannedText;
        }

        public void RecordJobPostingScan(string? scannedJobPostingText)
        {
            JobPostingScanCallCount++;
            LastScannedJobPostingText = scannedJobPostingText;
        }
    }
}
