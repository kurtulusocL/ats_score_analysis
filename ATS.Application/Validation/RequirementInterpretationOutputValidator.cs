using ATS.Application.Results;
using ATS.Core.Constants;
using ATS.Core.Helpers;
using ATS.Domain.Enums;
using System.Text.Json;

namespace ATS.Application.Validation
{
    public static class RequirementInterpretationOutputValidator
    {
        public const int MaximumOutputCharacters = 40000;
        public const int MaximumReportedErrors = 10;
        public const int MaximumEvidenceQuoteLength = PersistenceLimits.RequirementInterpretationEvidenceQuoteMaximumLength;
        public const int MaximumExplanationLength = PersistenceLimits.RequirementInterpretationExplanationMaximumLength;
        public const int MaximumSuggestionLength = PersistenceLimits.RequirementInterpretationSuggestionMaximumLength;

        private const string InterpretationsPropertyName = "interpretations";
        private const string RequirementPropertyName = "requirement";
        private const string StatusPropertyName = "status";
        private const string EvidenceQuotePropertyName = "evidenceQuote";
        private const string ExplanationPropertyName = "explanation";
        private const string SuggestionPropertyName = "suggestion";

        private static readonly string[] ItemPropertyNames =
        {
            RequirementPropertyName, StatusPropertyName, EvidenceQuotePropertyName, ExplanationPropertyName, SuggestionPropertyName
        };

        public static RequirementInterpretationValidationResult Validate(string? modelOutput, IReadOnlyList<string> requestedRequirementNames)
        {
            var requestedNamesByKey = IndexRequestedNames(requestedRequirementNames);

            if (string.IsNullOrWhiteSpace(modelOutput))
                return Invalid("The model output was empty.");

            if (modelOutput.Length > MaximumOutputCharacters)
                return Invalid($"The model output is longer than {MaximumOutputCharacters} characters.");

            if (!ModelJsonOutputReader.TryRemoveCodeFence(modelOutput, out var jsonText))
                return Invalid("The model output must contain only JSON, optionally inside one code fence.");

            try
            {
                return ValidateJson(jsonText, requestedRequirementNames, requestedNamesByKey);
            }
            catch (Exception exception) when (exception is JsonException or ArgumentException or InvalidOperationException)
            {
                return Invalid("The model output is not valid, readable JSON.");
            }
        }

        private static Dictionary<string, string> IndexRequestedNames(IReadOnlyList<string> requestedRequirementNames)
        {
            ArgumentNullException.ThrowIfNull(requestedRequirementNames);

            if (requestedRequirementNames.Count == 0)
                throw new ArgumentException("At least one requested requirement name is required.", nameof(requestedRequirementNames));

            var requestedNamesByKey = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var requestedName in requestedRequirementNames)
            {
                if (string.IsNullOrWhiteSpace(requestedName))
                    throw new ArgumentException("A requested requirement name must not be blank.", nameof(requestedRequirementNames));

                if (!requestedNamesByKey.TryAdd(TextWhitespaceHelper.Collapse(requestedName), requestedName))
                    throw new ArgumentException("The requested requirement names must be unique.", nameof(requestedRequirementNames));
            }

            return requestedNamesByKey;
        }

        private static RequirementInterpretationValidationResult ValidateJson(
            string jsonText,
            IReadOnlyList<string> requestedRequirementNames,
            Dictionary<string, string> requestedNamesByKey)
        {
            using var document = ModelJsonOutputReader.Parse(jsonText);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object
                || !ModelJsonOutputReader.TryReadProperties(root, out var rootProperties)
                || rootProperties.Count != 1
                || !rootProperties.TryGetValue(InterpretationsPropertyName, out var interpretationsElement))
            {
                return Invalid($"The root must be an object that contains only the property '{InterpretationsPropertyName}'.");
            }

            if (interpretationsElement.ValueKind != JsonValueKind.Array)
                return Invalid($"The property '{InterpretationsPropertyName}' must be an array.");

            if (interpretationsElement.GetArrayLength() > requestedRequirementNames.Count)
                return Invalid($"The array contains more entries than the {requestedRequirementNames.Count} requested requirements.");

            var errors = new List<string>();
            var interpretationsByName = new Dictionary<string, RequirementInterpretationResult>(StringComparer.Ordinal);
            var number = 0;

            foreach (var item in interpretationsElement.EnumerateArray())
            {
                number++;

                if (errors.Count >= MaximumReportedErrors)
                    break;

                var interpretation = ReadInterpretation(item, number, requestedNamesByKey, errors);
                if (interpretation == null)
                    continue;

                if (!interpretationsByName.TryAdd(interpretation.RequirementName, interpretation))
                    errors.Add($"Entry {number} repeats the requirement of an earlier entry.");
            }

            if (errors.Count == 0)
                AddMissingRequirementErrors(requestedRequirementNames, interpretationsByName, errors);

            return errors.Count == 0
                ? RequirementInterpretationValidationResult.CreateValid(
                    requestedRequirementNames.Select(requestedName => interpretationsByName[requestedName]).ToList())
                : RequirementInterpretationValidationResult.CreateInvalid(errors);
        }

        private static void AddMissingRequirementErrors(
            IReadOnlyList<string> requestedRequirementNames,
            Dictionary<string, RequirementInterpretationResult> interpretationsByName,
            List<string> errors)
        {
            for (var index = 0; index < requestedRequirementNames.Count && errors.Count < MaximumReportedErrors; index++)
            {
                if (!interpretationsByName.ContainsKey(requestedRequirementNames[index]))
                    errors.Add($"No interpretation was given for requested requirement number {index + 1}.");
            }
        }

        private static RequirementInterpretationResult? ReadInterpretation(
            JsonElement item, int number, Dictionary<string, string> requestedNamesByKey, List<string> errors)
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                errors.Add($"Entry {number} must be an object.");
                return null;
            }

            if (!ModelJsonOutputReader.TryReadProperties(item, out var properties)
                || properties.Count != ItemPropertyNames.Length
                || ItemPropertyNames.Any(propertyName => !properties.ContainsKey(propertyName)))
            {
                errors.Add($"Entry {number} must contain exactly the properties '{RequirementPropertyName}', '{StatusPropertyName}', '{EvidenceQuotePropertyName}', '{ExplanationPropertyName}' and '{SuggestionPropertyName}', each once.");
                return null;
            }

            var errorCountBefore = errors.Count;

            var requirementName = ReadRequirementName(properties[RequirementPropertyName], requestedNamesByKey);
            if (requirementName == null)
                errors.Add($"Entry {number}: '{RequirementPropertyName}' must match one of the requested requirements.");

            var status = ReadStatus(properties[StatusPropertyName]);
            if (status == null)
                errors.Add($"Entry {number}: '{StatusPropertyName}' must be \"met\", \"partial\" or \"missing\".");

            if (!TryReadText(properties[EvidenceQuotePropertyName], MaximumEvidenceQuoteLength, isRequired: false, out var evidenceQuote))
                errors.Add(CreateTextError(number, EvidenceQuotePropertyName, MaximumEvidenceQuoteLength, isRequired: false));

            if (!TryReadText(properties[ExplanationPropertyName], MaximumExplanationLength, isRequired: true, out var explanation))
                errors.Add(CreateTextError(number, ExplanationPropertyName, MaximumExplanationLength, isRequired: true));

            if (!TryReadText(properties[SuggestionPropertyName], MaximumSuggestionLength, isRequired: false, out var suggestion))
                errors.Add(CreateTextError(number, SuggestionPropertyName, MaximumSuggestionLength, isRequired: false));

            if (errors.Count == errorCountBefore && status is MatchStatus.Met or MatchStatus.Partial && evidenceQuote == null)
                errors.Add($"Entry {number}: a met or partial status needs an evidence quote.");

            if (errors.Count > errorCountBefore)
                return null;

            return new RequirementInterpretationResult(
                requirementName!, status!.Value, status == MatchStatus.Missing ? null : evidenceQuote, explanation!, suggestion);
        }

        private static string? ReadRequirementName(JsonElement element, Dictionary<string, string> requestedNamesByKey)
        {
            if (element.ValueKind != JsonValueKind.String)
                return null;

            return requestedNamesByKey.GetValueOrDefault(TextWhitespaceHelper.Collapse(element.GetString()));
        }

        private static MatchStatus? ReadStatus(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.String)
                return null;

            return element.GetString()?.Trim().ToLowerInvariant() switch
            {
                "met" => MatchStatus.Met,
                "partial" => MatchStatus.Partial,
                "missing" => MatchStatus.Missing,
                _ => null
            };
        }

        private static bool TryReadText(JsonElement element, int maximumLength, bool isRequired, out string? text)
        {
            text = null;

            if (element.ValueKind == JsonValueKind.Null)
                return !isRequired;

            if (element.ValueKind != JsonValueKind.String)
                return false;

            var collapsedText = TextWhitespaceHelper.Collapse(element.GetString());

            if (collapsedText.Length > maximumLength || collapsedText.Any(char.IsControl))
                return false;

            if (collapsedText.Length == 0)
                return !isRequired;

            text = collapsedText;
            return true;
        }

        private static string CreateTextError(int number, string propertyName, int maximumLength, bool isRequired) =>
            $"Entry {number}: '{propertyName}' must be {(isRequired ? "a non-empty string" : "a string or null")} of at most {maximumLength} characters without control characters.";

        private static RequirementInterpretationValidationResult Invalid(string error) =>
            RequirementInterpretationValidationResult.CreateInvalid(new[] { error });
    }
}
