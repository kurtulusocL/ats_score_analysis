using ATS.Application.Options;
using Microsoft.Extensions.Configuration;

namespace ATS.Tests
{
    public class AiOptionsTests
    {
        private static AiOptions Bind(Dictionary<string, string?> values)
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
            return configuration.GetSection(AiOptions.SectionName).Get<AiOptions>() ?? new AiOptions();
        }

        [Fact]
        public void GetEffectiveProviderPriority_ReturnsOllamaThenOpenAiCompatible_WhenNothingIsConfigured()
        {
            var aiOptions = Bind(new Dictionary<string, string?> { ["Ai:Enabled"] = "true" });

            Assert.Equal(new[] { "Ollama", "OpenAiCompatible" }, aiOptions.GetEffectiveProviderPriority());
        }

        [Fact]
        public void GetEffectiveProviderPriority_UsesConfiguredOrderWithoutDuplicates()
        {
            var aiOptions = Bind(new Dictionary<string, string?>
            {
                ["Ai:ProviderPriority:0"] = "OpenAiCompatible",
                ["Ai:ProviderPriority:1"] = "Ollama"
            });

            Assert.Equal(new[] { "OpenAiCompatible", "Ollama" }, aiOptions.GetEffectiveProviderPriority());
        }

        [Fact]
        public void OllamaProviderOptions_IsConfigured_OnlyWhenBothModelsAreSet()
        {
            var aiOptions = Bind(new Dictionary<string, string?>
            {
                ["Ai:Ollama:ChatModel"] = "chat-model"
            });
            Assert.False(aiOptions.Ollama.IsConfigured);

            aiOptions = Bind(new Dictionary<string, string?>
            {
                ["Ai:Ollama:ChatModel"] = "chat-model",
                ["Ai:Ollama:EmbeddingModel"] = "embedding-model"
            });
            Assert.True(aiOptions.Ollama.IsConfigured);
        }

        [Fact]
        public void OpenAiCompatibleProviderOptions_IsConfigured_WithoutAnyApiKey_WhenEndpointAndBothModelsAreSet()
        {
            var aiOptions = Bind(new Dictionary<string, string?>
            {
                ["Ai:OpenAiCompatible:Endpoint"] = "http://localhost:1234/v1",
                ["Ai:OpenAiCompatible:ChatModel"] = "chat-model",
                ["Ai:OpenAiCompatible:EmbeddingModel"] = "embedding-model"
            });

            Assert.True(aiOptions.OpenAiCompatible.IsConfigured);
        }

        [Fact]
        public void OpenAiCompatibleProviderOptions_IsNotConfigured_WhenEndpointIsNotAnAbsoluteHttpAddress()
        {
            foreach (var endpoint in new[] { "", "not a url", "localhost:1234/v1" })
            {
                var aiOptions = Bind(new Dictionary<string, string?>
                {
                    ["Ai:OpenAiCompatible:Endpoint"] = endpoint,
                    ["Ai:OpenAiCompatible:ChatModel"] = "chat-model",
                    ["Ai:OpenAiCompatible:EmbeddingModel"] = "embedding-model"
                });

                Assert.False(aiOptions.OpenAiCompatible.IsConfigured, $"Endpoint '{endpoint}' must not count as configured.");
            }
        }

        [Fact]
        public void OpenAiCompatibleProviderOptions_IsNotConfigured_WhenAModelIsBlank()
        {
            var aiOptions = Bind(new Dictionary<string, string?>
            {
                ["Ai:OpenAiCompatible:Endpoint"] = "https://example.test/v1",
                ["Ai:OpenAiCompatible:ChatModel"] = "chat-model"
            });

            Assert.False(aiOptions.OpenAiCompatible.IsConfigured);
        }

        [Fact]
        public void OpenAiCompatibleProviderOptions_GetBaseUri_AddsTrailingSlash_SoRelativePathsKeepTheVersionSegment()
        {
            var openAiCompatibleOptions = new OpenAiCompatibleProviderOptions { Endpoint = "https://example.test/v1" };

            var baseUri = openAiCompatibleOptions.GetBaseUri();

            Assert.NotNull(baseUri);
            Assert.Equal("https://example.test/v1/", baseUri!.ToString());
            Assert.Equal("https://example.test/v1/models", new Uri(baseUri, "models").ToString());
        }

        [Fact]
        public void OpenAiCompatibleProviderOptions_GetBaseUri_ReturnsNull_WhenEndpointIsInvalid()
        {
            Assert.Null(new OpenAiCompatibleProviderOptions { Endpoint = "not a url" }.GetBaseUri());
            Assert.Null(new OpenAiCompatibleProviderOptions { Endpoint = string.Empty }.GetBaseUri());
        }

        [Fact]
        public void OpenAiCompatibleProviderOptions_VerifiesModelsWithProvider_ByDefault()
        {
            Assert.True(new OpenAiCompatibleProviderOptions().VerifyModelsWithProvider);
        }
    }
}
