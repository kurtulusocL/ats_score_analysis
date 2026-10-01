

namespace ATS.Application.Results
{
    public sealed record RequirementInterpretationOutcome(
         IReadOnlyList<RequirementInterpretationResult> Interpretations,
         int UninterpretedRequirementCount,
         int DowngradedCount,
         string ModelIdentity)
    {
        public static RequirementInterpretationOutcome None { get; } =
            new(Array.Empty<RequirementInterpretationResult>(), 0, 0, string.Empty);
    }
}
