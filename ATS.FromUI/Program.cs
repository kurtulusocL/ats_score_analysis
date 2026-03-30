using ATS.Core.Logging;
using ATS.Infrastructure.DependencyResolver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ATS.FromUI
{
    internal static class Program
    {       
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var host = CreateHostBuilder().Build();

            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            var form = services.GetRequiredService<Form1>();
            System.Windows.Forms.Application.Run(form);
        }
        private static IHostBuilder CreateHostBuilder() =>
             Host.CreateDefaultBuilder()
                 .ConfigureAppConfiguration((context, config) =>
                 {
                     config.SetBasePath(AppContext.BaseDirectory);
                     config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                 })
                 .ConfigureServices((context, services) =>
                 {
                     SerilogConfiguration.Configure(context.Configuration);
                     services.AddInfrastructureServices(context.Configuration);
                     services.AddTransient<Form1>();
                 }).UseSerilog();
    }
}