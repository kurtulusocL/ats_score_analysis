
namespace ATS.Application.Prompts
{
    public sealed record LlmPrompt(string SystemInstruction, string UserMessage, IReadOnlyList<string> Warnings);
}
