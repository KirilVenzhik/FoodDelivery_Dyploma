using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace Observability.Correlation;

public class CorrelationMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    public CorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    private readonly RequestDelegate _next;

    public async Task InvokeAsync(
        HttpContext context,
        CorrelationContext correlationContext)
    {
        var correlationIdHeader = context.Request.Headers[HeaderName];

        var correlationId = correlationIdHeader.FirstOrDefault();

        if (!Guid.TryParse(correlationId, out var parsedCorrelationId))
        {
            parsedCorrelationId = Guid.NewGuid();
        }

        correlationContext.CorrelationId = parsedCorrelationId;

        context.Response.Headers[HeaderName] = parsedCorrelationId.ToString();

        await _next(context);
    }
}
