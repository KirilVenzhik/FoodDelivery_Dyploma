using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Observability.Correlation;

namespace Observability.Extensions;

public static class CorrelationExtensions
{
    public static IServiceCollection AddCorrelation(
        this IServiceCollection services)
    {
        services.AddScoped<CorrelationContext>();

        return services;
    }

    public static IApplicationBuilder UseCorrelation(
        this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationMiddleware>();

        return app;
    }
}