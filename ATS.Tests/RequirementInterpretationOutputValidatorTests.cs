using ATS.Application.Validation;
using ATS.Domain.Enums;

namespace ATS.Tests
{
    public class RequirementInterpretationOutputValidatorTests
    {
        private static readonly string[] RequestedNames = { "SQL Server", "Kubernetes" };

        private static string Json(string? value) => value == null ? "null" : $"\"{value}\"";

        private static string RawEntry(string requirementJson, string statusJson, string evidenceQuoteJson, string explanationJson, string suggestionJson) =>
            $"{{\"requirement\":{requirementJson},\"status\":{statusJson},\"evidenceQuote\":{evidenceQuoteJson},\"explanation\":{explanationJson},\"suggestion\":{suggestionJson}}}";

        private static string Entry(string requirement, string status, string? evidenceQuote, string explanation, string? suggestion) =>
            RawEntry(Json(requirement), Json(status), Json(evidenceQuote), Json(explanation), Json(suggestion));

        private static string Wrap(params string[] entries) => $"{{\"interpretations\":[{string.Join(",", entries)}]}}";

        private static readonly string SqlServerEntry =
            Entry("SQL Server", "met", "Developed APIs with SQL Server", "Shown in the experience section.", null);

        private static readonly string KubernetesEntry =
            Entry("Kubernetes", "missing", null, "No container work is mentioned.", "Describe any container work.");

        private static string ValidOutput() => Wrap(SqlServerEntry, KubernetesEntry);

        public static IEnumerable<object?[]> InvalidOutputs()
        {
            yield return new object?[] { null };
            yield return new object?[] { "" };
            yield return new object?[] { "   " };
            yield return new object?[] { "not json" };
            yield return new object?[] { "Here is the JSON: " + ValidOutput() };
            yield return new object?[] { ValidOutput() + " hope this helps" };
            yield return new object?[] { "```json\n" + ValidOutput() };
            yield return new object?[] { "```json" };
            yield return new object?[] { "[" + SqlServerEntry + "," + KubernetesEntry + "]" };
            yield return new object?[] { "{\"interpretations\":[" + SqlServerEntry + "," + KubernetesEntry + "],\"score\":100}" };
            yield return new object?[] { "{\"items\":[" + SqlServerEntry + "," + KubernetesEntry + "]}" };
            yield return new object?[] { "{\"interpretations\":[],\"interpretations\":[]}" };
            yield return new object?[] { "{\"interpretations\":{}}" };
            yield return new object?[] { "{\"interpretations\":[\"text\"]}" };
            yield return new object?[] { Wrap(RawEntry("\"SQL Server\"", "\"met\"", "\"quote text\"", "\"why\"", "null").Replace("}", ",\"score\":100}"), KubernetesEntry) };
            yield return new object?[] { Wrap("{\"requirement\":\"SQL Server\",\"status\":\"met\",\"evidenceQuote\":\"quote text\",\"explanation\":\"why\"}", KubernetesEntry) };
            yield return new object?[] { Wrap("{\"requirement\":\"SQL Server\",\"requirement\":\"Kubernetes\",\"status\":\"met\",\"evidenceQuote\":\"quote text\",\"explanation\":\"why\",\"suggestion\":null}", KubernetesEntry) };
            yield return new object?[] { Wrap(Entry("SQL Server", "unclear", "quote text", "why", null), KubernetesEntry) };
            yield return new object?[] { Wrap(RawEntry("\"SQL Server\"", "1", "\"quote text\"", "\"why\"", "null"), KubernetesEntry) };
            yield return new object?[] { Wrap(Entry("SQL Server", "met", null, "why", null), KubernetesEntry) };
            yield return new object?[] { Wrap(Entry("SQL Server", "partial", "   ", "why", null), KubernetesEntry) };
            yield return new object?[] { Wrap(Entry("Cobol", "met", "quote text", "why", null), KubernetesEntry) };
            yield return new object?[] { Wrap(RawEntry("5", "\"met\"", "\"quote text\"", "\"why\"", "null"), KubernetesEntry) };
            yield return new object?[] { Wrap(SqlServerEntry, SqlServerEntry) };
            yield return new object?[] { Wrap(SqlServerEntry) };
            yield return new object?[] { Wrap(SqlServerEntry, KubernetesEntry, Entry("Third", "missing", null, "why", null)) };
            yield return new object?[] { Wrap(Entry("SQL Server", "met", "quote text", "   ", null), KubernetesEntry) };
            yield return new object?[] { Wrap(RawEntry("\"SQL Server\"", "\"met\"", "\"quote text\"", "null", "null"), KubernetesEntry) };
            yield return new object?[] { Wrap(RawEntry("\"SQL Server\"", "\"met\"", "\"quote text\"", "5", "null"), KubernetesEntry) };
            yield return new object?[] { Wrap(Entry("SQL Server", "met", "quote text", new string('e', RequirementInterpretationOutputValidator.MaximumExplanationLength + 1), null), KubernetesEntry) };
            yield return new object?[] { Wrap(Entry("SQL Server", "met", new string('q', RequirementInterpretationOutputValidator.MaximumEvidenceQuoteLength + 1), "why", null), KubernetesEntry) };
            yield return new object?[] { Wrap(Entry("SQL Server", "met", "quote text", "why", new string('s', RequirementInterpretationOutputValidator.MaximumSuggestionLength + 1)), KubernetesEntry) };
            yield return new object?[] { Wrap(Entry("SQL Server", "met", "quote text", "bad\\u0000text", null), KubernetesEntry) };
            yield return new object?[] { "{\"interpretations\":[" + SqlServerEntry + "," + KubernetesEntry + ",]}" };
            yield return new object?[] { "{\"interpretations\":[] /* comment */}" };
            yield return new object?[] { new string(' ', RequirementInterpretationOutputValidator.MaximumOutputCharacters + 1) + ValidOutput() };
        }

        [Fact]
        public void Validate_ReturnsTheInterpretationsInTheRequestedOrder_WhateverTheOrderOfTheModel()
        {
            var result = RequirementInterpretationOutputValidator.Validate(Wrap(KubernetesEntry, SqlServerEntry), RequestedNames);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
            Assert.Collection(result.Interpretations,
                interpretation =>
                {
                    Assert.Equal("SQL Server", interpretation.RequirementName);
                    Assert.Equal(MatchStatus.Met, interpretation.Status);
                    Assert.Equal("Developed APIs with SQL Server", interpretation.EvidenceQuote);
                    Assert.Equal("Shown in the experience section.", interpretation.Explanation);
                    Assert.Null(interpretation.Suggestion);
                },
                interpretation =>
                {
                    Assert.Equal("Kubernetes", interpretation.RequirementName);
                    Assert.Equal(MatchStatus.Missing, interpretation.Status);
                    Assert.Null(interpretation.EvidenceQuote);
                    Assert.Equal("Describe any container work.", interpretation.Suggestion);
                });
        }

        [Theory]
        [InlineData("```json\n{0}\n```")]
        [InlineData("```\n{0}\n```")]
        [InlineData("  ```JSON\r\n{0}\r\n```  ")]
        public void Validate_AcceptsASingleOuterCodeFence(string template)
        {
            var result = RequirementInterpretationOutputValidator.Validate(template.Replace("{0}", ValidOutput()), RequestedNames);

            Assert.True(result.IsValid);
            Assert.Equal(2, result.Interpretations.Count);
        }

        [Fact]
        public void Validate_MatchesTheRequirementNameIgnoringCaseAndSurroundingWhitespace_AndReturnsTheRequestedSpelling()
        {
            var output = Wrap(Entry("  sql   SERVER ", "met", "quote text", "why", null), KubernetesEntry);

            var result = RequirementInterpretationOutputValidator.Validate(output, RequestedNames);

            Assert.True(result.IsValid);
            Assert.Equal("SQL Server", result.Interpretations[0].RequirementName);
        }

        [Fact]
        public void Validate_AcceptsTheStatusIgnoringCaseAndWhitespace()
        {
            var output = Wrap(Entry("SQL Server", " PARTIAL ", "quote text", "why", null), Entry("Kubernetes", "Missing", null, "why", null));

            var result = RequirementInterpretationOutputValidator.Validate(output, RequestedNames);

            Assert.True(result.IsValid);
            Assert.Equal(MatchStatus.Partial, result.Interpretations[0].Status);
            Assert.Equal(MatchStatus.Missing, result.Interpretations[1].Status);
        }

        [Fact]
        public void Validate_CollapsesWhitespaceAndLineBreaksInTheTextFields()
        {
            var output = Wrap(Entry("SQL Server", "met", "line one\\nline   two", " why\\r\\n so ", "  do\\tthis "), KubernetesEntry);

            var interpretation = RequirementInterpretationOutputValidator.Validate(output, RequestedNames).Interpretations[0];

            Assert.Equal("line one line two", interpretation.EvidenceQuote);
            Assert.Equal("why so", interpretation.Explanation);
            Assert.Equal("do this", interpretation.Suggestion);
        }

        [Fact]
        public void Validate_TreatsABlankQuoteOrSuggestionAsNothing()
        {
            var output = Wrap(Entry("SQL Server", "met", "quote text", "why", "   "), Entry("Kubernetes", "missing", "", "why", ""));

            var result = RequirementInterpretationOutputValidator.Validate(output, RequestedNames);

            Assert.True(result.IsValid);
            Assert.Null(result.Interpretations[0].Suggestion);
            Assert.Null(result.Interpretations[1].EvidenceQuote);
            Assert.Null(result.Interpretations[1].Suggestion);
        }

        [Fact]
        public void Validate_DiscardsTheQuoteOfAMissingRequirement()
        {
            var output = Wrap(SqlServerEntry, Entry("Kubernetes", "missing", "some quote text", "why", null));

            var result = RequirementInterpretationOutputValidator.Validate(output, RequestedNames);

            Assert.True(result.IsValid);
            Assert.Null(result.Interpretations[1].EvidenceQuote);
        }

        [Fact]
        public void Validate_AcceptsValuesExactlyAtTheLimits()
        {
            var output = Wrap(
                Entry("SQL Server", "met",
                    new string('q', RequirementInterpretationOutputValidator.MaximumEvidenceQuoteLength),
                    new string('e', RequirementInterpretationOutputValidator.MaximumExplanationLength),
                    new string('s', RequirementInterpretationOutputValidator.MaximumSuggestionLength)),
                KubernetesEntry);

            var result = RequirementInterpretationOutputValidator.Validate(output, RequestedNames);

            Assert.True(result.IsValid);
        }

        [Theory]
        [MemberData(nameof(InvalidOutputs))]
        public void Validate_RejectsInvalidOutput_WithErrorsAndNoInterpretations(string? modelOutput)
        {
            var result = RequirementInterpretationOutputValidator.Validate(modelOutput, RequestedNames);

            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
            Assert.Empty(result.Interpretations);
        }

        [Fact]
        public void Validate_NamesTheMissingRequirementByItsNumber()
        {
            var result = RequirementInterpretationOutputValidator.Validate(Wrap(SqlServerEntry), RequestedNames);

            var error = Assert.Single(result.Errors);
            Assert.Contains("number 2", error);
        }

        [Fact]
        public void Validate_DoesNotEchoTextFromTheModelOutputInErrors()
        {
            var output = Wrap(
                Entry("SECRET-REQUIREMENT", "met", "quote text", "why", null),
                Entry("Kubernetes", "met", "quote text", new string('x', 1001) + "SECRETTEXT", null));

            var result = RequirementInterpretationOutputValidator.Validate(output, RequestedNames);

            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
            Assert.All(result.Errors, error => Assert.DoesNotContain("SECRET", error));
        }

        [Fact]
        public void Validate_ReportsAtMostTenErrors()
        {
            var requestedNames = Enumerable.Range(1, 15).Select(index => $"Requirement {index}").ToArray();
            var entries = Enumerable.Range(1, 15).Select(index => Entry($"Unknown {index}", "missing", null, "why", null)).ToArray();

            var result = RequirementInterpretationOutputValidator.Validate(Wrap(entries), requestedNames);

            Assert.False(result.IsValid);
            Assert.Equal(RequirementInterpretationOutputValidator.MaximumReportedErrors, result.Errors.Count);
        }

        [Fact]
        public void Validate_Throws_WhenTheRequestedNamesAreNull()
        {
            Assert.Throws<ArgumentNullException>(() => RequirementInterpretationOutputValidator.Validate(ValidOutput(), null!));
        }

        [Fact]
        public void Validate_Throws_WhenNoRequirementWasRequested()
        {
            Assert.Throws<ArgumentException>(() => RequirementInterpretationOutputValidator.Validate(ValidOutput(), Array.Empty<string>()));
        }

        [Theory]
        [InlineData("SQL Server", "sql  server")]
        [InlineData("SQL Server", "   ")]
        public void Validate_Throws_WhenTheRequestedNamesAreBlankOrNotUnique(string first, string second)
        {
            Assert.Throws<ArgumentException>(() => RequirementInterpretationOutputValidator.Validate(ValidOutput(), new[] { first, second }));
        }
    }
}
