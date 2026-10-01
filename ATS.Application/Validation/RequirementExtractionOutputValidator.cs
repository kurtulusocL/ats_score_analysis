using ATS.Application.Results;
using System.Text.Json;

namespace ATS.Application.Validation
{
    public static class RequirementExtractionOutputValidator
    {
        public const int MaximumOutputCharacters = 20000;
        public const int MaximumRequirementCount = 50;
        public const int MaximumNameLength = 100;
        public const int MaximumCategoryLength = 50;
        public const int MaximumReportedErrors = 10;

        private const string RequirementsPropertyName = "requirements";
        private const string NamePropertyName = "name";
        private const string IsMandatoryPropertyName = "isMandatory";
        private const string CategoryPropertyName = "category";

        private static readonly string[] ItemPropertyNames = { NamePropertyName, IsMandatoryPropertyName, CategoryPropertyName };

        public static RequirementExtractionValidationResult Validate(string? modelOutput)
        {
            if (string.IsNullOrWhiteSpace(modelOutput))
                return Invalid("The model output was empty.");

            if (modelOutput.Length > MaximumOutputCharacters)
                return Invalid($"The model output is longer than {MaximumOutputCharacters} characters.");

            if (!ModelJsonOutputReader.TryRemoveCodeFence(modelOutput, out var jsonText))
                return Invalid("The model output must contain only JSON, optionally inside one code fence.");

            try
            {
                return ValidateJson(jsonText);
            }
            catch (Exception exception) when (exception is JsonException or ArgumentException or InvalidOperationException)
            {
                return Invalid("The model output is not valid, readable JSON.");
            }
        }

        private static RequirementExtractionValidationResult ValidateJson(string jsonText)
        {
            using var document = ModelJsonOutputReader.Parse(jsonText);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object
                || !ModelJsonOutputReader.TryReadProperties(root, out var rootProperties)
                || rootProperties.Count != 1
                || !rootProperties.TryGetValue(RequirementsPropertyName, out var requirementsElement))
            {
                return Invalid($"The root must be an object that contains only the property '{RequirementsPropertyName}'.");
            }

            if (requirementsElement.ValueKind != JsonValueKind.Array)
                return Invalid($"The property '{RequirementsPropertyName}' must be an array.");

            if (requirementsElement.GetArrayLength() > MaximumRequirementCount)
                return Invalid($"The array contains more than {MaximumRequirementCount} requirements.");

            var errors = new List<string>();
            var requirements = new List<ExtractedRequirement>();
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var number = 0;

            foreach (var item in requirementsElement.EnumerateArray())
            {
                number++;

                if (errors.Count >= MaximumReportedErrors)
                    break;

                var requirement = ReadRequirement(item, number, errors);
                if (requirement == null)
                    continue;

                if (!names.Add(requirement.Name))
                {
                    errors.Add($"Requirement {number} repeats the name of an earlier requirement.");
                    continue;
                }

                requirements.Add(requirement);
            }

            return errors.Count == 0
                ? RequirementExtractionValidationResult.CreateValid(requirements)
                : RequirementExtractionValidationResult.CreateInvalid(errors);
        }

        private static ExtractedRequirement? ReadRequirement(JsonElement item, int number, List<string> errors)
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                errors.Add($"Requirement {number} must be an object.");
                return null;
            }

            if (!ModelJsonOutputReader.TryReadProperties(item, out var properties)
                || properties.Count != ItemPropertyNames.Length
                || ItemPropertyNames.Any(propertyName => !properties.ContainsKey(propertyName)))
            {
                errors.Add($"Requirement {number} must contain exactly the properties '{NamePropertyName}', '{IsMandatoryPropertyName}' and '{CategoryPropertyName}', each once.");
                return null;
            }

            var errorCountBefore = errors.Count;
            var name = ReadText(properties[NamePropertyName], NamePropertyName, MaximumNameLength, number, errors);
            var category = ReadText(properties[CategoryPropertyName], CategoryPropertyName, MaximumCategoryLength, number, errors);

            var isMandatoryElement = properties[IsMandatoryPropertyName];
            if (isMandatoryElement.ValueKind != JsonValueKind.True && isMandatoryElement.ValueKind != JsonValueKind.False)
                errors.Add($"Requirement {number}: '{IsMandatoryPropertyName}' must be true or false.");

            if (errors.Count > errorCountBefore)
                return null;

            return new ExtractedRequirement(name!, isMandatoryElement.GetBoolean(), category!);
        }

        private static string? ReadText(JsonElement element, string propertyName, int maximumLength, int number, List<string> errors)
        {
            var value = element.ValueKind == JsonValueKind.String ? element.GetString()?.Trim() : null;

            if (string.IsNullOrEmpty(value) || value.Length > maximumLength || value.Any(char.IsControl))
            {
                errors.Add($"Requirement {number}: '{propertyName}' must be a non-empty string of at most {maximumLength} characters without control characters.");
                return null;
            }

            return value;
        }

        private static RequirementExtractionValidationResult Invalid(string error) =>
            RequirementExtractionValidationResult.CreateInvalid(new[] { error });
    }
}
