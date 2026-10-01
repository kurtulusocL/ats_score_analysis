
namespace ATS.Application.Options
{
    public class OpenAiCompatibleProviderOptions
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ChatModel { get; set; } = string.Empty;
        public string EmbeddingModel { get; set; } = string.Empty;
        public bool VerifyModelsWithProvider { get; set; } = true;

        public bool IsConfigured =>
            TryGetEndpointUri(out _)
            && !string.IsNullOrWhiteSpace(ChatModel)
            && !string.IsNullOrWhiteSpace(EmbeddingModel);

        public Uri? GetBaseUri()
        {
            if (!TryGetEndpointUri(out var endpointUri))
                return null;

            var uriBuilder = new UriBuilder(endpointUri);
            if (!uriBuilder.Path.EndsWith('/'))
                uriBuilder.Path += "/";

            return uriBuilder.Uri;
        }

        private bool TryGetEndpointUri(out Uri endpointUri)
        {
            if (Uri.TryCreate(Endpoint, UriKind.Absolute, out var parsedUri)
                && (parsedUri.Scheme == Uri.UriSchemeHttp || parsedUri.Scheme == Uri.UriSchemeHttps))
            {
                endpointUri = parsedUri;
                return true;
            }

            endpointUri = null!;
            return false;
        }
    }
}
