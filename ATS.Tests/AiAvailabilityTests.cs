using ATS.Application.Abstract.AI;

namespace ATS.Tests
{
    public class AiAvailabilityTests
    {
        [Fact]
        public void InvalidCredentials_WithoutApiKey_MentionsThatNoKeyIsConfigured()
        {
            var availability = AiAvailability.InvalidCredentials("OpenAiCompatible", apiKeyWasSent: false);

            Assert.Equal(AiStatus.InvalidCredentials, availability.Status);
            Assert.False(availability.IsAvailable);
            Assert.Contains("none is configured", availability.Message);
        }

        [Fact]
        public void InvalidCredentials_WithApiKey_MentionsThatTheKeyWasRejected()
        {
            var availability = AiAvailability.InvalidCredentials("OpenAiCompatible", apiKeyWasSent: true);

            Assert.Contains("rejected", availability.Message);
        }

        [Fact]
        public void NoProviderAvailable_ReturnsInvalidCredentialsStatus_WhenThatIsTheFirstMeaningfulFailure()
        {
            var failures = new[]
            {
            AiAvailability.NotConfigured("Ollama"),
            AiAvailability.InvalidCredentials("OpenAiCompatible", apiKeyWasSent: false)
        };

            var result = AiAvailability.NoProviderAvailable(failures);

            Assert.Equal(AiStatus.InvalidCredentials, result.Status);
            Assert.Contains("OpenAiCompatible requires an API key", result.Message);
            Assert.Contains("Running deterministic analysis only", result.Message);
        }
    }
}
