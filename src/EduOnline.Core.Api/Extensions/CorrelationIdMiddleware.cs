using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EduOnline.Core.Api.Extensions;

public class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : Guid.NewGuid().ToString("N");

        context.TraceIdentifier = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        var start = Stopwatch.GetTimestamp();

        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            try
            {
                await next(context);
            }
            finally
            {
                var elapsedMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;

                logger.LogInformation(
                    "Request completed. CorrelationId={CorrelationId} RequestPath={RequestPath} StatusCode={StatusCode} ElapsedMs={ElapsedMs}",
                    correlationId,
                    context.Request.Path.Value ?? string.Empty,
                    context.Response.StatusCode,
                    elapsedMs);
            }
        }
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
