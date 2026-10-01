using System.Text.Json;

namespace ATS.Application.Validation
{
    public static class ModelJsonOutputReader
    {
        private const string CodeFence = "```";

        private static readonly JsonDocumentOptions ParseOptions = new()
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 8
        };

        public static bool TryRemoveCodeFence(string modelOutput, out string jsonText)
        {
            var text = modelOutput.Trim();
            jsonText = text;

            if (!text.StartsWith(CodeFence, StringComparison.Ordinal))
                return true;

            var openingLineEnd = text.IndexOf('\n');
            if (openingLineEnd < 0)
                return false;

            var openingLine = text[..openingLineEnd].Trim();
            var hasValidOpening = openingLine == CodeFence || openingLine.Equals(CodeFence + "json", StringComparison.OrdinalIgnoreCase);

            if (!hasValidOpening || !text.EndsWith(CodeFence, StringComparison.Ordinal) || text.Length - CodeFence.Length <= openingLineEnd)
                return false;

            jsonText = text[(openingLineEnd + 1)..^CodeFence.Length];
            return true;
        }

        public static JsonDocument Parse(string jsonText) => JsonDocument.Parse(jsonText, ParseOptions);

        public static bool TryReadProperties(JsonElement element, out Dictionary<string, JsonElement> properties)
        {
            properties = new Dictionary<string, JsonElement>(StringComparer.Ordinal);

            foreach (var property in element.EnumerateObject())
            {
                if (!properties.TryAdd(property.Name, property.Value))
                    return false;
            }

            return true;
        }
    }
}
