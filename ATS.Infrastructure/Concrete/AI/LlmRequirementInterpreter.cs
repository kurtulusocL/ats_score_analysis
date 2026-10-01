using ATS.Application.Abstract.AI;
using ATS.Application.Knowledge;
using ATS.Application.Prompts;
using ATS.Application.Results;
using ATS.Application.Validation;
using ATS.Core.Helpers;
using Microsoft.Extensions.AI;
using Serilog;

namespace ATS.Infrastructure.Concrete.AI
{
    public class LlmRequirementInterpreter : IRequirementInterpreter
    {
        public const int BatchSize = 10;
        public const int MaximumCvCharacters = 12000;
        private const int MaximumRequirementsCharacters = 4000;
        private const int MaximumSkillKnowledgeCharacters = 8000;
        private const int TextLengthHint = 300;
        private const string CvLabel = "CV";
        private const string RequirementsLabel = "Requirements";
        private const string SkillKnowledgeLabel = "Skill knowledge";

        private static readonly string TaskInstruction =
            $"For each requirement listed in the {RequirementsLabel} document, decide from the {CvLabel} document alone whether the CV shows that the candidate meets it. "
            + "Use \"met\" when the CV clearly shows it, \"partial\" when the CV shows related or incomplete evidence, and \"missing\" when the CV shows no evidence. "
            + "Every \"met\" or \"partial\" decision must contain an evidence quote copied word for word from the CV. Never invent evidence. If you cannot quote the CV, the decision is \"missing\". "
            + "Explain each decision in one or two short sentences in English. "
            + "For \"partial\" and \"missing\" decisions give one short, concrete and honest suggestion for improving the CV. Never suggest claiming skills the candidate does not have. "
            + $"When a {SkillKnowledgeLabel} document is present, use it only to recognise equivalent wording in the CV, such as abbreviations and synonyms. It is not evidence by itself. "
            + "Do not give a score and do not decide whether the candidate should be hired.";

        private static readonly string OutputFormatInstruction =
            "Return one JSON object with exactly one property named \"interpretations\", whose value is an array with exactly one item for each requirement in the "
            + $"{RequirementsLabel} document. Each item is an object with exactly five properties: "
            + "\"requirement\" (the requirement text copied exactly), "
            + "\"status\" (\"met\", \"partial\" or \"missing\"), "
            + $"\"evidenceQuote\" (a word-for-word quote from the CV of at most {TextLengthHint} characters, or null when the status is \"missing\"), "
            + $"\"explanation\" (a string of at most {TextLengthHint} characters) and "
            + $"\"suggestion\" (a string of at most {TextLengthHint} characters, or null). "
            + "Use no other properties.";

        private readonly Lazy<IChatClient> _chatClient;
        private readonly LlmPromptBuilder _llmPromptBuilder;
        private readonly StructuredChatRequester _structuredChatRequester;
        private readonly ISkillKnowledgeRetriever _skillKnowledgeRetriever;
        private readonly IChatModelIdentityProvider _chatModelIdentityProvider;
        private readonly ILogger _logger = Log.ForContext<LlmRequirementInterpreter>();

        public LlmRequirementInterpreter(Lazy<IChatClient> chatClient, LlmPromptBuilder llmPromptBuilder, StructuredChatRequester structuredChatRequester,
            ISkillKnowledgeRetriever skillKnowledgeRetriever, IChatModelIdentityProvider chatModelIdentityProvider)
        {
            _chatClient = chatClient;
            _llmPromptBuilder = llmPromptBuilder;
            _structuredChatRequester = structuredChatRequester;
            _skillKnowledgeRetriever = skillKnowledgeRetriever;
            _chatModelIdentityProvider = chatModelIdentityProvider;
        }

        public async Task<RequirementInterpretationOutcome> InterpretAsync(IReadOnlyList<ExtractedRequirement> requirements, string cvText, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(requirements);

            var distinctRequirements = requirements
                .DistinctBy(requirement => TextWhitespaceHelper.Collapse(requirement.Name), StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (distinctRequirements.Count == 0)
                return RequirementInterpretationOutcome.None;

            if (string.IsNullOrWhiteSpace(cvText))
                return new RequirementInterpretationOutcome(Array.Empty<RequirementInterpretationResult>(), distinctRequirements.Count, 0, string.Empty);

            var knowledgeByRequirementName = await RetrieveKnowledgeAsync(distinctRequirements, cancellationToken);

            var interpretations = new List<RequirementInterpretationResult>();
            var uninterpretedCount = 0;
            var downgradedCount = 0;

            foreach (var batch in distinctRequirements.Chunk(BatchSize))
            {
                var batchInterpretations = await InterpretBatchAsync(batch, cvText, knowledgeByRequirementName, cancellationToken);

                if (batchInterpretations == null)
                {
                    uninterpretedCount += batch.Length;
                    continue;
                }

                var groundingResult = EvidenceGroundingChecker.Check(batchInterpretations, cvText);
                interpretations.AddRange(groundingResult.Interpretations);
                downgradedCount += groundingResult.DowngradedCount;
            }

            _logger.Information(
                "Requirement interpretation finished. Interpreted: {InterpretedCount}, Uninterpreted: {UninterpretedCount}, Downgraded: {DowngradedCount}",
                interpretations.Count, uninterpretedCount, downgradedCount);

            var modelIdentity = interpretations.Count > 0 ? _chatModelIdentityProvider.GetChatModelIdentity() : string.Empty;

            return new RequirementInterpretationOutcome(interpretations, uninterpretedCount, downgradedCount, modelIdentity);
        }

        private async Task<Dictionary<string, RequirementKnowledge>> RetrieveKnowledgeAsync(
            IReadOnlyList<ExtractedRequirement> requirements, CancellationToken cancellationToken)
        {
            var knowledgeByRequirementName = new Dictionary<string, RequirementKnowledge>(StringComparer.OrdinalIgnoreCase);

            try
            {
                foreach (var requirementKnowledge in await _skillKnowledgeRetriever.RetrieveAsync(requirements, cancellationToken))
                    knowledgeByRequirementName.TryAdd(TextWhitespaceHelper.Collapse(requirementKnowledge.Requirement.Name), requirementKnowledge);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.Warning(exception, "Skill knowledge could not be retrieved. The requirements are interpreted without it.");
                knowledgeByRequirementName.Clear();
            }

            return knowledgeByRequirementName;
        }

        private async Task<IReadOnlyList<RequirementInterpretationResult>?> InterpretBatchAsync(IReadOnlyList<ExtractedRequirement> batch, string cvText, Dictionary<string, RequirementKnowledge> knowledgeByRequirementName, CancellationToken cancellationToken)
        {
            var requirementNames = batch.Select(requirement => requirement.Name).ToList();

            var batchKnowledge = batch
                .Select(requirement => knowledgeByRequirementName.GetValueOrDefault(TextWhitespaceHelper.Collapse(requirement.Name)))
                .OfType<RequirementKnowledge>()
                .ToList();

            var documents = new List<PromptDocument>
            {
                new(CvLabel, cvText, MaximumCvCharacters),
                new(RequirementsLabel, string.Join("\n", requirementNames), MaximumRequirementsCharacters)
            };

            var skillKnowledgeText = SkillKnowledgePromptFormatter.Format(batchKnowledge);
            if (skillKnowledgeText != null)
                documents.Add(new PromptDocument(SkillKnowledgeLabel, skillKnowledgeText, MaximumSkillKnowledgeCharacters));

            var llmPrompt = _llmPromptBuilder.Build(TaskInstruction, OutputFormatInstruction, documents);

            foreach (var promptWarning in llmPrompt.Warnings)
                _logger.Warning("{PromptWarning}", promptWarning);

            var messages = new[]
            {
                new ChatMessage(ChatRole.System, llmPrompt.SystemInstruction),
                new ChatMessage(ChatRole.User, llmPrompt.UserMessage)
            };

            var batchInterpretations = await _structuredChatRequester.RequestAsync<IReadOnlyList<RequirementInterpretationResult>>(
                _chatClient.Value, messages, modelOutput => CheckOutput(modelOutput, requirementNames), cancellationToken);

            if (batchInterpretations == null)
                _logger.Warning("No valid interpretation could be obtained for a batch of {RequirementCount} requirements.", batch.Count);

            return batchInterpretations;
        }

        private static (IReadOnlyList<RequirementInterpretationResult>? Result, IReadOnlyList<string> Errors) CheckOutput(
            string? modelOutput, IReadOnlyList<string> requirementNames)
        {
            var validationResult = RequirementInterpretationOutputValidator.Validate(modelOutput, requirementNames);
            return (validationResult.IsValid ? validationResult.Interpretations : null, validationResult.Errors);
        }
    }
}
