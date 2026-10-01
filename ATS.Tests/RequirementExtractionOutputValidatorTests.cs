using ATS.Application.Validation;

namespace ATS.Tests
{
    public class RequirementExtractionOutputValidatorTests
    {
        private static string RawItem(string nameJson, string isMandatoryJson, string categoryJson) =>
            $"{{\"name\":{nameJson},\"isMandatory\":{isMandatoryJson},\"category\":{categoryJson}}}";

        private static string Item(string name = "SQL Server", bool isMandatory = true, string category = "Database") =>
            RawItem($"\"{name}\"", isMandatory ? "true" : "false", $"\"{category}\"");

        private static string Wrap(params string[] items) =>
            $"{{\"requirements\":[{string.Join(",", items)}]}}";

        public static IEnumerable<object?[]> InvalidOutputs()
        {
            yield return new object?[] { null };
            yield return new object?[] { "" };
            yield return new object?[] { "   " };
            yield return new object?[] { "not json" };
            yield return new object?[] { "Here is the JSON: " + Wrap(Item()) };
            yield return new object?[] { Wrap(Item()) + " hope this helps" };
            yield return new object?[] { "```json\n" + Wrap(Item()) };
            yield return new object?[] { "```python\n" + Wrap(Item()) + "\n```" };
            yield return new object?[] { "```json\n```json\n" + Wrap(Item()) + "\n```\n```" };
            yield return new object?[] { "[" + Item() + "]" };
            yield return new object?[] { "{\"requirements\":[" + Item() + "],\"score\":100}" };
            yield return new object?[] { "{\"items\":[" + Item() + "]}" };
            yield return new object?[] { "{\"requirements\":{}}" };
            yield return new object?[] { "{\"requirements\":[\"text\"]}" };
            yield return new object?[] { Wrap("{\"name\":\"SQL\",\"isMandatory\":true,\"category\":\"Database\",\"score\":100}") };
            yield return new object?[] { Wrap("{\"name\":\"SQL\",\"isMandatory\":true}") };
            yield return new object?[] { Wrap("{\"name\":\"A\",\"name\":\"B\",\"isMandatory\":true,\"category\":\"Database\"}") };
            yield return new object?[] { Wrap(RawItem("\"SQL\"", "\"true\"", "\"Database\"")) };
            yield return new object?[] { Wrap(RawItem("\"SQL\"", "1", "\"Database\"")) };
            yield return new object?[] { Wrap(RawItem("123", "true", "\"Database\"")) };
            yield return new object?[] { Wrap(Item("   ")) };
            yield return new object?[] { Wrap(RawItem("null", "true", "\"Database\"")) };
            yield return new object?[] { Wrap(Item(new string('a', RequirementExtractionOutputValidator.MaximumNameLength + 1))) };
            yield return new object?[] { Wrap(Item("line\\nbreak")) };
            yield return new object?[] { Wrap(Item(category: new string('c', RequirementExtractionOutputValidator.MaximumCategoryLength + 1))) };
            yield return new object?[] { Wrap(Item(category: "   ")) };
            yield return new object?[] { Wrap(Item("SQL Server"), Item("sql server")) };
            yield return new object?[] { Wrap(Enumerable.Range(1, RequirementExtractionOutputValidator.MaximumRequirementCount + 1).Select(index => Item($"Requirement {index}")).ToArray()) };
            yield return new object?[] { "{\"requirements\":[" + Item() + ",]}" };
            yield return new object?[] { "{\"requirements\":[] /* comment */}" };
            yield return new object?[] { new string(' ', RequirementExtractionOutputValidator.MaximumOutputCharacters + 1) + Wrap(Item()) };
            yield return new object?[] { "{\"requirements\":[],\"requirements\":[]}" };
            yield return new object?[] { "```json" };
        }

        [Fact]
        public void Validate_ReturnsTheRequirementsInOrder_ForValidJson()
        {
            var result = RequirementExtractionOutputValidator.Validate(
                Wrap(Item("SQL Server", true, "Database"), Item("Kubernetes", false, "Tool")));

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
            Assert.Collection(result.Requirements,
                requirement => { Assert.Equal("SQL Server", requirement.Name); Assert.True(requirement.IsMandatory); Assert.Equal("Database", requirement.Category); },
                requirement => { Assert.Equal("Kubernetes", requirement.Name); Assert.False(requirement.IsMandatory); Assert.Equal("Tool", requirement.Category); });
        }

        [Theory]
        [InlineData("```json\n{0}\n```")]
        [InlineData("```\n{0}\n```")]
        [InlineData("```JSON\n{0}\n```")]
        [InlineData("  ```json\r\n{0}\r\n```  ")]
        public void Validate_AcceptsASingleOuterCodeFence(string template)
        {
            var result = RequirementExtractionOutputValidator.Validate(template.Replace("{0}", Wrap(Item())));

            Assert.True(result.IsValid);
            Assert.Single(result.Requirements);
        }

        [Fact]
        public void Validate_TrimsWhitespaceAroundNameAndCategory()
        {
            var result = RequirementExtractionOutputValidator.Validate(Wrap(Item("  SQL Server  ", true, "  Database ")));

            var requirement = Assert.Single(result.Requirements);
            Assert.Equal("SQL Server", requirement.Name);
            Assert.Equal("Database", requirement.Category);
        }

        [Fact]
        public void Validate_AcceptsAnEmptyRequirementList()
        {
            var result = RequirementExtractionOutputValidator.Validate(Wrap());

            Assert.True(result.IsValid);
            Assert.Empty(result.Requirements);
        }

        [Fact]
        public void Validate_AcceptsValuesExactlyAtTheLimits()
        {
            var items = new List<string>
            {
                Item(new string('n', RequirementExtractionOutputValidator.MaximumNameLength), true, new string('c', RequirementExtractionOutputValidator.MaximumCategoryLength))
            };
            items.AddRange(Enumerable.Range(2, RequirementExtractionOutputValidator.MaximumRequirementCount - 1).Select(index => Item($"Requirement {index}")));

            var result = RequirementExtractionOutputValidator.Validate(Wrap(items.ToArray()));

            Assert.True(result.IsValid);
            Assert.Equal(RequirementExtractionOutputValidator.MaximumRequirementCount, result.Requirements.Count);
        }

        [Theory]
        [MemberData(nameof(InvalidOutputs))]
        public void Validate_RejectsInvalidOutput_WithErrorsAndNoRequirements(string? modelOutput)
        {
            var result = RequirementExtractionOutputValidator.Validate(modelOutput);

            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
            Assert.Empty(result.Requirements);
        }

        [Fact]
        public void Validate_DoesNotEchoTextFromTheModelOutputInErrors()
        {
            var output = Wrap(
                "{\"name\":\"SQL\",\"isMandatory\":true,\"category\":\"Database\",\"IGNORE-PREVIOUS-INSTRUCTIONS\":1}",
                Item(string.Concat(Enumerable.Repeat("SECRETTEXT", 11))));

            var result = RequirementExtractionOutputValidator.Validate(output);

            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
            Assert.All(result.Errors, error =>
            {
                Assert.DoesNotContain("IGNORE-PREVIOUS", error);
                Assert.DoesNotContain("SECRETTEXT", error);
            });
        }

        [Fact]
        public void Validate_ReportsAtMostTenErrors()
        {
            var items = Enumerable.Range(1, 15).Select(_ => Item("   ")).ToArray();

            var result = RequirementExtractionOutputValidator.Validate(Wrap(items));

            Assert.False(result.IsValid);
            Assert.Equal(RequirementExtractionOutputValidator.MaximumReportedErrors, result.Errors.Count);
        }
    }
}
