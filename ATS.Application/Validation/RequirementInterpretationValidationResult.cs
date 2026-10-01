using ATS.Application.Results;

namespace ATS.Application.Validation
{
    public sealed record RequirementInterpretationValidationResult(
        bool IsValid,
        IReadOnlyList<RequirementInterpretationResult> Interpretations,
        IReadOnlyList<string> Errors)
    {
        public static RequirementInterpretationValidationResult CreateValid(IReadOnlyList<RequirementInterpretationResult> interpretations) =>
            new(true, interpretations, Array.Empty<string>());

        public static RequirementInterpretationValidationResult CreateInvalid(IReadOnlyList<string> errors) =>
            new(false, Array.Empty<RequirementInterpretationResult>(), errors);
    }
}
