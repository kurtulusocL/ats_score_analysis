
namespace ATS.Application.Options
{
    public class OllamaProviderOptions
    {
        public string Endpoint { get; set; } = "http://localhost:11434";
        public string ChatModel { get; set; } = string.Empty;
        public string EmbeddingModel { get; set; } = string.Empty;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(Endpoint)
            && !string.IsNullOrWhiteSpace(ChatModel)
            && !string.IsNullOrWhiteSpace(EmbeddingModel);
    }
}
