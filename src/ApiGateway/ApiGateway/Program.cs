using Auth;
using Auth.Jwt;
using Observability.Correlation;
using Observability.Extensions;
using Observability.Logging;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureLogging(builder.Configuration);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(ctx =>
    {
        ctx.AddRequestTransform(async transform =>
        {
            var correlationContext = transform.HttpContext.RequestServices
                .GetRequiredService<CorrelationContext>();
            transform.ProxyRequest.Headers.Remove(CorrelationMiddleware.HeaderName);
            transform.ProxyRequest.Headers.TryAddWithoutValidation(
                CorrelationMiddleware.HeaderName,
                correlationContext.CorrelationId.ToString());
            await Task.CompletedTask;
        });
    });

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthServices();
builder.Services.AddCorrelation();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("authenticated", policy => policy.RequireAuthenticatedUser());
});

var app = builder.Build();

app.UseCorrelation();
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();

app.Run();
