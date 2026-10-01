using ATS.Application.Results;

namespace ATS.Application.Validation
{
    public sealed record EvidenceGroundingResult(IReadOnlyList<RequirementInterpretationResult> Interpretations, int DowngradedCount);
}
