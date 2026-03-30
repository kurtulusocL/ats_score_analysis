using ATS.Application.Abstract.Repository;
using ATS.Application.Abstract.Services;
using ATS.Infrastructure.Concrete.Repository;
using ATS.Infrastructure.Concrete.ServiceManagers;
using ATS.Infrastructure.Persistence.Context.Mssql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATS.Infrastructure.DependencyResolver
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")).UseLazyLoadingProxies());

            services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
            services.AddScoped<ICvParserService, CvParserManager>();
            services.AddScoped<ICvScanService, CvScanManager>();
            services.AddScoped<IReportService, ReportManager>();
            services.AddHttpClient<ITranslatorService, RapidApiTranslator>();

            return services;
        }
    }
}
