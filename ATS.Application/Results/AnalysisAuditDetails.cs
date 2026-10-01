using ATS.Application.Abstract.AI;

namespace ATS.Application.Results
{
    public sealed record AnalysisAuditDetails(
        string AiStatus,
        string? AiProviderName,
        int? DeterministicJobMatchScore,
        int? HybridJobMatchScore,
        double? SemanticJobMatchScore,
        int DurationMilliseconds,
        AiUsageSnapshot Usage);
}
