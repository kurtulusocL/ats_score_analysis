

namespace ATS.Application.Results
{
    public sealed record HybridScoreResult
    (
        int HybridScore,
        int DeterministicScore,
        double? SemanticScore,
        bool IsSemanticScoreApplied
    );
}
