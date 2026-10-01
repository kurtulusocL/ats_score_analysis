using ATS.Application.Results;

namespace ATS.Application.Abstract.AI
{
    public interface IRequirementInterpreter
    {
        Task<RequirementInterpretationOutcome> InterpretAsync(IReadOnlyList<ExtractedRequirement> requirements, string cvText, CancellationToken cancellationToken = default);
    }
}
