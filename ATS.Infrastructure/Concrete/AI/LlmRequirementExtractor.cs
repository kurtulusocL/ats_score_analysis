using ATS.Application.Abstract.AI;
using ATS.Application.Prompts;
using ATS.Application.Results;
using ATS.Application.Validation;
using Microsoft.Extensions.AI;
using Serilog;

namespace ATS.Infrastructure.Concrete.AI
{
    public class LlmRequirementExtractor : IRequirementExtractor
    {
        public const int MaximumJobPostingCharacters = 8000;
        private const string JobPostingLabel = "Job posting";
        private static readonly string TaskInstruction =
            "Read the job posting document and list the separate requirements a candidate must or should meet. "
            + "A requirement is a skill, tool, qualification, level of experience, certification, language or responsibility that a CV could show. "
            + "Ignore company descriptions, benefits, slogans and locations. "
            + $"Write each requirement name in English as a short noun phrase of at most {RequirementExtractionOutputValidator.MaximumNameLength} characters. "
            + "Mark a requirement as mandatory unless the posting presents it as preferred, a plus or nice to have. "
            + "Do not merge unrelated requirements and do not list the same requirement twice. "
            + $"Give each requirement a short category label of at most {RequirementExtractionOutputValidator.MaximumCategoryLength} characters, "
            + "for example Technical skill, Tool, Education, Experience, Language, Soft skill or Responsibility.";

        private static readonly string OutputFormatInstruction =
            "Return one JSON object with exactly one property named \"requirements\", whose value is an array. "
            + "Each item of the array is an object with exactly three properties: \"name\" (string), \"isMandatory\" (true or false) and \"category\" (string). "
            + "Use no other properties. "
            + $"Return at most {RequirementExtractionOutputValidator.MaximumRequirementCount} items. "
            + "If the document contains no requirements, return {\"requirements\":[]}.";

        private readonly Lazy<IChatClient> _chatClient;
        private readonly LlmPromptBuilder _llmPromptBuilder;
        private readonly StructuredChatRequester _structuredChatRequester;
        private readonly ILogger _logger = Log.ForContext<LlmRequirementExtractor>();

        public LlmRequirementExtractor(Lazy<IChatClient> chatClient, LlmPromptBuilder llmPromptBuilder, StructuredChatRequester structuredChatRequester)
        {
            _chatClient = chatClient;
            _llmPromptBuilder = llmPromptBuilder;
            _structuredChatRequester = structuredChatRequester;
        }

        public async Task<IReadOnlyList<ExtractedRequirement>> ExtractAsync(string jobPostingText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(jobPostingText))
                return Array.Empty<ExtractedRequirement>();

            var llmPrompt = _llmPromptBuilder.Build(
                TaskInstruction,
                OutputFormatInstruction,
                new[] { new PromptDocument(JobPostingLabel, jobPostingText, MaximumJobPostingCharacters) });

            foreach (var promptWarning in llmPrompt.Warnings)
                _logger.Warning("{PromptWarning}", promptWarning);

            var messages = new[]
            {
                new ChatMessage(ChatRole.System, llmPrompt.SystemInstruction),
                new ChatMessage(ChatRole.User, llmPrompt.UserMessage)
            };

            var requirements = await _structuredChatRequester.RequestAsync<IReadOnlyList<ExtractedRequirement>>(
                _chatClient.Value, messages, CheckOutput, cancellationToken);

            if (requirements == null)
            {
                _logger.Warning("No valid requirement list could be obtained from the model.");
                return Array.Empty<ExtractedRequirement>();
            }

            _logger.Information("Requirements extracted. Count: {RequirementCount}", requirements.Count);
            return requirements;
        }

        private static (IReadOnlyList<ExtractedRequirement>? Result, IReadOnlyList<string> Errors) CheckOutput(string? modelOutput)
        {
            var validationResult = RequirementExtractionOutputValidator.Validate(modelOutput);
            return (validationResult.IsValid ? validationResult.Requirements : null, validationResult.Errors);
        }
    }
}
