using ATS.Application.Abstract.AI;
using ATS.Domain.Entities;

namespace ATS.Application.Results
{
    public sealed class AnalysisResult
    {
        public AnalysisResult(CvScan scan, IReadOnlyList<string> warnings, AiAvailability? ai = null, IReadOnlyList<SecurityFindingResult>? securityFindings = null)
        {
            Scan = scan;
            Warnings = warnings;
            Ai = ai;
            SecurityFindings = securityFindings ?? Array.Empty<SecurityFindingResult>();
        }

        public CvScan Scan { get; }
        public IReadOnlyList<string> Warnings { get; }
        public AiAvailability? Ai { get; }
        public IReadOnlyList<SecurityFindingResult> SecurityFindings { get; }
    }
}
