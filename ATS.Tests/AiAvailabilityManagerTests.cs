using ATS.Application.Abstract.AI;
using ATS.Application.Options;
using ATS.Infrastructure.Concrete.AI;
using Microsoft.Extensions.Options;

namespace ATS.Tests
{
    public class AiAvailabilityManagerTests
    {
        private sealed class FakeProviderAvailabilityChecker(string providerName, Func<AiAvailability> respond) : IAiProviderAvailabilityChecker
        {
            public int CallCount { get; private set; }

            public string ProviderName => providerName;

            public Task<AiAvailability> CheckAsync(CancellationToken cancellationToken = default)
            {
                CallCount++;
                return Task.FromResult(respond());
            }
        }

        private static AiAvailabilityManager CreateManager(bool enabled, string[] providerPriority, params IAiProviderAvailabilityChecker[] checkers) =>
            CreateManager(new AiProviderSelection(), enabled, providerPriority, checkers);

        private static AiAvailabilityManager CreateManager(AiProviderSelection selection, bool enabled, string[] providerPriority, params IAiProviderAvailabilityChecker[] checkers)
        {
            var aiOptions = new AiOptions { Enabled = enabled, ProviderPriority = providerPriority.ToList() };
            return new AiAvailabilityManager(checkers, Options.Create(aiOptions), selection);
        }

        private static FakeProviderAvailabilityChecker AvailableChecker(string providerName) =>
            new(providerName, () => AiAvailability.Ok(providerName));

        [Fact]
        public async Task CheckAsync_ReturnsDisabled_WithoutCallingAnyChecker_WhenAiIsDisabled()
        {
            var ollamaChecker = AvailableChecker("Ollama");
            var manager = CreateManager(false, new[] { "Ollama" }, ollamaChecker);

            var result = await manager.CheckAsync();

            Assert.Equal(AiStatus.Disabled, result.Status);
            Assert.Equal(0, ollamaChecker.CallCount);
        }

        [Fact]
        public async Task CheckAsync_ReturnsFirstAvailableProvider_AndDoesNotCheckTheOthers()
        {
            var ollamaChecker = AvailableChecker("Ollama");
            var apiChecker = AvailableChecker("OpenAiCompatible");
            var manager = CreateManager(true, new[] { "Ollama", "OpenAiCompatible" }, ollamaChecker, apiChecker);

            var result = await manager.CheckAsync();

            Assert.Equal(AiStatus.Available, result.Status);
            Assert.Equal("Ollama", result.ProviderName);
            Assert.Equal(1, ollamaChecker.CallCount);
            Assert.Equal(0, apiChecker.CallCount);
        }

        [Fact]
        public async Task CheckAsync_FallsBackToSecondProvider_WhenFirstIsUnreachable()
        {
            var ollamaChecker = new FakeProviderAvailabilityChecker("Ollama", () => AiAvailability.Unreachable("Ollama", "http://localhost:11434"));
            var apiChecker = AvailableChecker("OpenAiCompatible");
            var manager = CreateManager(true, new[] { "Ollama", "OpenAiCompatible" }, ollamaChecker, apiChecker);

            var result = await manager.CheckAsync();

            Assert.Equal(AiStatus.Available, result.Status);
            Assert.Equal("OpenAiCompatible", result.ProviderName);
            Assert.Equal(1, ollamaChecker.CallCount);
            Assert.Equal(1, apiChecker.CallCount);
        }

        [Fact]
        public async Task CheckAsync_RespectsConfiguredProviderPriority()
        {
            var ollamaChecker = AvailableChecker("Ollama");
            var apiChecker = AvailableChecker("OpenAiCompatible");
            var manager = CreateManager(true, new[] { "OpenAiCompatible", "Ollama" }, ollamaChecker, apiChecker);

            var result = await manager.CheckAsync();

            Assert.Equal("OpenAiCompatible", result.ProviderName);
            Assert.Equal(0, ollamaChecker.CallCount);
        }

        [Fact]
        public async Task CheckAsync_WhenAllProvidersFail_ReturnsFirstFailureStatusAndCombinesMessages()
        {
            var ollamaChecker = new FakeProviderAvailabilityChecker("Ollama", () => AiAvailability.Unreachable("Ollama", "http://localhost:11434"));
            var apiChecker = new FakeProviderAvailabilityChecker("OpenAiCompatible", () => AiAvailability.MissingModel("OpenAiCompatible", "gpt-x"));
            var manager = CreateManager(true, new[] { "Ollama", "OpenAiCompatible" }, ollamaChecker, apiChecker);

            var result = await manager.CheckAsync();

            Assert.Equal(AiStatus.ProviderUnreachable, result.Status);
            Assert.Contains("Ollama is not reachable", result.Message);
            Assert.Contains("gpt-x", result.Message);
            Assert.Contains("Running deterministic analysis only", result.Message);
        }

        [Fact]
        public async Task CheckAsync_WhenEveryProviderIsNotConfigured_ReturnsNotConfigured()
        {
            var ollamaChecker = new FakeProviderAvailabilityChecker("Ollama", () => AiAvailability.NotConfigured("Ollama"));
            var apiChecker = new FakeProviderAvailabilityChecker("OpenAiCompatible", () => AiAvailability.NotConfigured("OpenAiCompatible"));
            var manager = CreateManager(true, new[] { "Ollama", "OpenAiCompatible" }, ollamaChecker, apiChecker);

            var result = await manager.CheckAsync();

            Assert.Equal(AiStatus.NotConfigured, result.Status);
            Assert.Contains("no provider is configured", result.Message);
        }

        [Fact]
        public async Task CheckAsync_TreatsACheckerThatThrowsAsUnavailable_AndTriesTheNextProvider()
        {
            var ollamaChecker = new FakeProviderAvailabilityChecker("Ollama", () => throw new InvalidOperationException("boom"));
            var apiChecker = AvailableChecker("OpenAiCompatible");
            var manager = CreateManager(true, new[] { "Ollama", "OpenAiCompatible" }, ollamaChecker, apiChecker);

            var result = await manager.CheckAsync();

            Assert.Equal("OpenAiCompatible", result.ProviderName);
        }

        [Fact]
        public async Task CheckAsync_ReturnsNotConfigured_WhenNoCheckerIsRegistered()
        {
            var manager = CreateManager(true, new[] { "Ollama", "OpenAiCompatible" });

            var result = await manager.CheckAsync();

            Assert.Equal(AiStatus.NotConfigured, result.Status);
        }

        [Fact]
        public async Task CheckAsync_PropagatesCancellation_WhenCallerCancels()
        {
            var ollamaChecker = new FakeProviderAvailabilityChecker("Ollama", () => throw new OperationCanceledException());
            var manager = CreateManager(true, new[] { "Ollama" }, ollamaChecker);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => manager.CheckAsync(cts.Token));
        }

        [Fact]
        public async Task CheckAsync_RecordsTheSelectedProvider()
        {
            var selection = new AiProviderSelection();
            var ollamaChecker = new FakeProviderAvailabilityChecker("Ollama", () => AiAvailability.Unreachable("Ollama", "http://localhost:11434"));
            var apiChecker = AvailableChecker("OpenAiCompatible");
            var manager = CreateManager(selection, true, new[] { "Ollama", "OpenAiCompatible" }, ollamaChecker, apiChecker);

            await manager.CheckAsync();

            Assert.Equal("OpenAiCompatible", selection.SelectedProviderName);
        }

        [Fact]
        public async Task CheckAsync_ClearsThePreviousSelection_WhenNoProviderIsUsable()
        {
            var selection = new AiProviderSelection();
            selection.Select("Ollama");
            var ollamaChecker = new FakeProviderAvailabilityChecker("Ollama", () => AiAvailability.Unreachable("Ollama", "http://localhost:11434"));
            var manager = CreateManager(selection, true, new[] { "Ollama" }, ollamaChecker);

            await manager.CheckAsync();

            Assert.Null(selection.SelectedProviderName);
        }

        [Fact]
        public async Task CheckAsync_ClearsThePreviousSelection_WhenAiIsDisabled()
        {
            var selection = new AiProviderSelection();
            selection.Select("Ollama");
            var manager = CreateManager(selection, false, new[] { "Ollama" });

            await manager.CheckAsync();

            Assert.Null(selection.SelectedProviderName);
        }
    }
}
