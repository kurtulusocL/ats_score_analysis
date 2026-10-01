using ATS.Application.Abstract.AI;
using ATS.Application.Results;

namespace ATS.Tests.TestSupport
{
    public sealed class FakeRequirementInterpreter(Func<IReadOnlyList<ExtractedRequirement>, RequirementInterpretationOutcome> respond) : IRequirementInterpreter
    {
        public int CallCount { get; private set; }
        public List<IReadOnlyList<ExtractedRequirement>> ReceivedRequirementLists { get; } = new();
        public List<string> ReceivedCvTexts { get; } = new();

        public static FakeRequirementInterpreter ReturningNothing() => new(_ => RequirementInterpretationOutcome.None);

        public Task<RequirementInterpretationOutcome> InterpretAsync(IReadOnlyList<ExtractedRequirement> requirements, string cvText, CancellationToken cancellationToken = default)
        {
            CallCount++;
            ReceivedRequirementLists.Add(requirements);
            ReceivedCvTexts.Add(cvText);
            return Task.FromResult(respond(requirements));
        }
    }
}
