using ATS.Application.Results;

namespace ATS.Application.Validation
{
    public sealed record RequirementExtractionValidationResult(
        bool IsValid,
        IReadOnlyList<ExtractedRequirement> Requirements,
        IReadOnlyList<string> Errors)
    {
        public static RequirementExtractionValidationResult CreateValid(IReadOnlyList<ExtractedRequirement> requirements) =>
            new(true, requirements, Array.Empty<string>());

        public static RequirementExtractionValidationResult CreateInvalid(IReadOnlyList<string> errors) =>
            new(false, Array.Empty<ExtractedRequirement>(), errors);
    }
}
