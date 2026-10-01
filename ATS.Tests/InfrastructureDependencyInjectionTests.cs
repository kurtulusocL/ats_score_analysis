using ATS.Application.Abstract.AI;
using ATS.Application.Abstract.Services;
using ATS.Application.Semantic;
using ATS.Infrastructure.Concrete.AI;
using ATS.Infrastructure.DependencyResolver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATS.Tests
{
    public class InfrastructureDependencyInjectionTests
    {
        private static ServiceProvider BuildServiceProvider()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=(local);Database=AtsTestDb;Trusted_Connection=true;TrustServerCertificate=true"
                })
                .Build();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddInfrastructureServices(configuration);

            return services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });
        }

        [Theory]
        [InlineData(typeof(ICvScanService))]
        [InlineData(typeof(IAiAvailabilityService))]
        [InlineData(typeof(ISecurityScanService))]
        [InlineData(typeof(IEmbeddingService))]
        [InlineData(typeof(ISemanticMatcher))]
        [InlineData(typeof(IRequirementExtractor))]
        [InlineData(typeof(HybridScoreCalculator))]
        [InlineData(typeof(ISkillKnowledgeRetriever))]
        [InlineData(typeof(IRequirementInterpreter))]
        [InlineData(typeof(IChatModelIdentityProvider))]
        [InlineData(typeof(AiUsageTracker))]
        public void ScopedServices_CanBeResolved(Type serviceType)
        {
            using var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            Assert.NotNull(scope.ServiceProvider.GetRequiredService(serviceType));
        }

        [Fact]
        public void BothAiProviderAvailabilityCheckersAreRegistered()
        {
            using var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var providerNames = scope.ServiceProvider.GetServices<IAiProviderAvailabilityChecker>()
                .Select(checker => checker.ProviderName)
                .OrderBy(providerName => providerName, StringComparer.Ordinal);

            Assert.Equal(new[] { "Ollama", "OpenAiCompatible" }, providerNames);
        }

        [Fact]
        public void BothAiProviderClientFactoriesAreRegistered()
        {
            using var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var providerNames = scope.ServiceProvider.GetServices<IAiProviderClientFactory>()
                .Select(factory => factory.ProviderName)
                .OrderBy(providerName => providerName, StringComparer.Ordinal);

            Assert.Equal(new[] { "Ollama", "OpenAiCompatible" }, providerNames);
        }

        [Fact]
        public void BothHiddenTextFileScannersAreRegistered()
        {
            using var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var fileTypes = scope.ServiceProvider.GetServices<IHiddenTextFileScanner>()
                .Select(scanner => scanner.FileType)
                .OrderBy(fileType => fileType, StringComparer.Ordinal);

            Assert.Equal(new[] { "docx", "pdf" }, fileTypes);
        }

        [Fact]
        public void RequirementExtractor_IsTheLanguageModelExtractor()
        {
            using var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            Assert.IsType<LlmRequirementExtractor>(scope.ServiceProvider.GetRequiredService<IRequirementExtractor>());
        }

        [Fact]
        public void AiUsageTracker_IsSharedInsideAScope_AndSeparateBetweenScopes()
        {
            using var serviceProvider = BuildServiceProvider();
            using var firstScope = serviceProvider.CreateScope();
            using var secondScope = serviceProvider.CreateScope();

            var firstTracker = firstScope.ServiceProvider.GetRequiredService<AiUsageTracker>();

            Assert.Same(firstTracker, firstScope.ServiceProvider.GetRequiredService<AiUsageTracker>());
            Assert.NotSame(firstTracker, secondScope.ServiceProvider.GetRequiredService<AiUsageTracker>());
        }
    }
}
