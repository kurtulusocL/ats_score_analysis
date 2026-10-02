using System;
using System.Net.Http;
using ATS.Application.Abstract.AI;
using ATS.Application.Abstract.Repository;
using ATS.Application.Abstract.Services;
using ATS.Application.Knowledge;
using ATS.Application.Options;
using ATS.Application.Prompts;
using ATS.Application.Security;
using ATS.Application.Semantic;
using ATS.Infrastructure.Concrete.AI;
using ATS.Infrastructure.Concrete.Repository;
using ATS.Infrastructure.Concrete.Security;
using ATS.Infrastructure.Concrete.ServiceManagers;
using ATS.Infrastructure.Persistence.Context.Mssql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ATS.Infrastructure.DependencyResolver;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<ApplicationDbContext>((DbContextOptionsBuilder options) =>
		{
			options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), (SqlServerDbContextOptionsBuilder sqlServerOptions) =>
			{
				sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
			}).UseLazyLoadingProxies();
		});
		services.Configure<AiOptions>(configuration.GetSection("Ai"));
		services.Configure<SemanticOptions>(configuration.GetSection("Semantic"));
		services.AddHttpClient<ITranslatorService, RapidApiTranslator>((HttpClient httpClient) =>
		{
			int num = ((int.TryParse(configuration["RapidApi:TimeoutSeconds"], out var result) && result > 0) ? result : 20);
			httpClient.Timeout = TimeSpan.FromSeconds(num);
		});
		services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
		services.AddScoped<ICvParserService, CvParserManager>();
		services.AddScoped<ICvScanService, CvScanManager>();
		services.AddScoped<IReportService, ReportManager>();
		services.AddHttpClient<OllamaAvailabilityManager>((IServiceProvider serviceProvider, HttpClient httpClient) =>
		{
			AiOptions value = serviceProvider.GetRequiredService<IOptions<AiOptions>>().Value;
			if (Uri.TryCreate(value.Ollama.Endpoint, UriKind.Absolute, out Uri result))
			{
				httpClient.BaseAddress = result;
			}
		});
		services.AddHttpClient<OpenAiCompatibleAvailabilityManager>();
		services.AddHttpClient("OpenAiCompatibleLlm", (IServiceProvider serviceProvider, HttpClient httpClient) =>
		{
			AiOptions value = serviceProvider.GetRequiredService<IOptions<AiOptions>>().Value;
			httpClient.Timeout = TimeSpan.FromSeconds(value.TimeoutSeconds);
		});
		services.AddScoped<IAiProviderClientFactory, OpenAiCompatibleClientFactory>();
		services.AddTransient((Func<IServiceProvider, IAiProviderAvailabilityChecker>)((IServiceProvider serviceProvider) => serviceProvider.GetRequiredService<OllamaAvailabilityManager>()));
		services.AddTransient((Func<IServiceProvider, IAiProviderAvailabilityChecker>)((IServiceProvider serviceProvider) => serviceProvider.GetRequiredService<OpenAiCompatibleAvailabilityManager>()));
		services.AddSingleton<IAiApiKeyProvider, ConfigurationAiApiKeyProvider>();
		services.AddScoped<AiProviderSelection>();
		services.AddScoped<IAiAvailabilityService, AiAvailabilityManager>();
		services.AddHttpClient("OllamaLlm", (IServiceProvider serviceProvider, HttpClient httpClient) =>
		{
			AiOptions value = serviceProvider.GetRequiredService<IOptions<AiOptions>>().Value;
			if (Uri.TryCreate(value.Ollama.Endpoint, UriKind.Absolute, out Uri result))
			{
				httpClient.BaseAddress = result;
			}
			httpClient.Timeout = TimeSpan.FromSeconds(value.TimeoutSeconds);
		});
		services.AddScoped<IAiProviderClientFactory, OllamaClientFactory>();
		services.AddScoped<AiProviderClientResolver>();
		services.AddScoped<AiUsageTracker>();
		services.AddScoped((Func<IServiceProvider, IChatClient>)((IServiceProvider serviceProvider) => new LlmCallLoggingChatClient(serviceProvider.GetRequiredService<AiProviderClientResolver>().Resolve().CreateChatClient(), serviceProvider.GetRequiredService<AiUsageTracker>())));
		services.AddScoped((IServiceProvider serviceProvider) => new Lazy<IChatClient>(() => serviceProvider.GetRequiredService<IChatClient>()));
		services.AddSingleton<StructuredChatRequester>();
		services.AddSingleton<IPromptDelimiterProvider, RandomPromptDelimiterProvider>();
		services.AddSingleton<LlmPromptBuilder>();
		services.AddScoped<IEmbeddingService, CachedEmbeddingManager>();
		services.AddScoped<IRequirementExtractor, LlmRequirementExtractor>();
		services.AddScoped<ISkillKnowledgeSource, SkillKnowledgeSource>();
		services.AddScoped((Func<IServiceProvider, ISkillKnowledgeRetriever>)((IServiceProvider serviceProvider) => new SkillKnowledgeRetriever(serviceProvider.GetRequiredService<ISkillKnowledgeSource>(), serviceProvider.GetRequiredService<IEmbeddingService>(), serviceProvider.GetRequiredService<IOptions<SemanticOptions>>().Value)));
		services.AddScoped<IRequirementInterpreter, LlmRequirementInterpreter>();
		services.AddScoped<IChatModelIdentityProvider, SelectedProviderChatModelIdentityProvider>();
		services.AddScoped((Func<IServiceProvider, ISemanticMatcher>)((IServiceProvider serviceProvider) => new SemanticMatcher(serviceProvider.GetRequiredService<IEmbeddingService>(), serviceProvider.GetRequiredService<IOptions<SemanticOptions>>().Value)));
		services.AddScoped((IServiceProvider serviceProvider) => new HybridScoreCalculator(serviceProvider.GetRequiredService<IOptions<SemanticOptions>>().Value));
		services.AddScoped((IServiceProvider serviceProvider) => new Lazy<IRequirementExtractor>(() => serviceProvider.GetRequiredService<IRequirementExtractor>()));
		services.AddScoped((IServiceProvider serviceProvider) => new Lazy<ISemanticMatcher>(() => serviceProvider.GetRequiredService<ISemanticMatcher>()));
		services.AddScoped((IServiceProvider serviceProvider) => new Lazy<HybridScoreCalculator>(() => serviceProvider.GetRequiredService<HybridScoreCalculator>()));
		services.AddScoped((IServiceProvider serviceProvider) => new Lazy<IRequirementInterpreter>(() => serviceProvider.GetRequiredService<IRequirementInterpreter>()));
		services.AddSingleton<InstructionPatternDetector>();
		services.AddSingleton<HiddenTextDetector>();
		services.AddSingleton<WordHiddenTextDetector>();
		services.AddTransient<IHiddenTextFileScanner, PdfHiddenTextScanner>();
		services.AddTransient<IHiddenTextFileScanner, DocxHiddenTextScanner>();
		services.AddScoped<ISecurityScanService, SecurityScanManager>();
		return services;
	}
}
