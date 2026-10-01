
namespace ATS.Application.Options
{
    public class SemanticOptions
    {
        public const string SectionName = "Semantic";

        public double MetSimilarityThreshold { get; set; } = 0.75;
        public double PartialSimilarityThreshold { get; set; } = 0.55;
        public int MaxChunkCharacters { get; set; } = 400;
        public int KnowledgeHitsPerRequirement { get; set; } = 3;
        public double KnowledgeSimilarityThreshold { get; set; } = 0.5;
        public double DeterministicWeight { get; set; } = 0.5;
        public double SemanticWeight { get; set; } = 0.5;
        public double MandatoryRequirementWeight { get; set; } = 2.0;
        public double OptionalRequirementWeight { get; set; } = 1.0;
        public double PartialMatchCredit { get; set; } = 0.5;
    }
}
