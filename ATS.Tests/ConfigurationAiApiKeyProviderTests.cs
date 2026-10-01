using ATS.Infrastructure.Concrete.AI;
using Microsoft.Extensions.Configuration;

namespace ATS.Tests
{
    public class ConfigurationAiApiKeyProviderTests
    {
        private static IConfigurationRoot CreateConfiguration(params (string Key, string? Value)[] values)
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            foreach (var (key, value) in values)
                configuration[key] = value;

            return configuration;
        }

        [Fact]
        public void GetApiKey_ReturnsNull_WhenNoKeyIsConfiguredOrItIsBlank()
        {
            Assert.Null(new ConfigurationAiApiKeyProvider(CreateConfiguration()).GetApiKey());
            Assert.Null(new ConfigurationAiApiKeyProvider(
                CreateConfiguration((ConfigurationAiApiKeyProvider.ConfigurationKey, "   "))).GetApiKey());
        }

        [Fact]
        public void GetApiKey_ReturnsTheTrimmedKey()
        {
            var provider = new ConfigurationAiApiKeyProvider(
                CreateConfiguration((ConfigurationAiApiKeyProvider.ConfigurationKey, "  secret-key \n")));

            Assert.Equal("secret-key", provider.GetApiKey());
        }

        [Fact]
        public void GetApiKey_PicksUpAKeyThatIsAddedAfterTheProviderWasCreated()
        {
            var configuration = CreateConfiguration();
            var provider = new ConfigurationAiApiKeyProvider(configuration);
            Assert.Null(provider.GetApiKey());

            configuration[ConfigurationAiApiKeyProvider.ConfigurationKey] = "added-later";

            Assert.Equal("added-later", provider.GetApiKey());
        }
    }
}
