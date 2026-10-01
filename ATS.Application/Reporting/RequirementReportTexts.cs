
namespace ATS.Application.Reporting
{
    public static class RequirementReportTexts
    {
        public const string SectionTitle = "Requirement analysis";
        public const string SemanticLabel = "Embedding match";
        public const string ModelLabel = "Language model";
        public const string ExplanationLabel = "Explanation";
        public const string EvidenceLabel = "Evidence";
        public const string SuggestionLabel = "Suggestion";
        public const string DisagreementNote = "The embedding match and the language model disagree about this requirement. Check it by hand.";

        public static string BuildModelNote(string? modelIdentity) =>
            string.IsNullOrWhiteSpace(modelIdentity)
                ? "The explanations were written by a language model. They do not change the score."
                : $"The explanations were written by a language model ({modelIdentity}). They do not change the score.";
    }
}
