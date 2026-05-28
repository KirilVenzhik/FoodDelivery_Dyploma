using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Observability.Logging;

public static class LoggingExtensions
{
    public static void ConfigureLogging(
        this IHostBuilder hostBuilder,
        IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Seq("http://seq:80")
            .CreateLogger();

        hostBuilder.UseSerilog();
    }
}