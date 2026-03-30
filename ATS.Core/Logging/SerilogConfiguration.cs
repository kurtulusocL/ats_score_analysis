using Microsoft.Extensions.Configuration;
using Serilog;

namespace ATS.Core.Logging
{
    public static class SerilogConfiguration
    {
        public static void Configure(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).Enrich.FromLogContext().Enrich.WithMachineName().Enrich.WithThreadId().WriteTo.Console().WriteTo.File(
                    path: Path.Combine(AppContext.BaseDirectory, "Logs", "ats-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}").CreateLogger();
        }
    }
}