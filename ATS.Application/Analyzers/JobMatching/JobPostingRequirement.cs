
namespace ATS.Application.Analyzers.JobMatching
{
    public sealed record JobPostingRequirement
    (
        string Text,
        bool IsMandatory,
        int? RequiredYears,
        IReadOnlyList<RequirementTerm> Terms
    );
}
