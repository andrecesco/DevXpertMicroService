using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace EduOnline.Core.Api.Extensions;

public class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-Id";

    private static readonly Meter _meter = new("EduOnline.Observability", "1.0");
    private static readonly Counter<long> _requestCounter = _meter.CreateCounter<long>("eduonline_requests_total", description: "Total number of requests");
    private static readonly Histogram<double> _requestDuration = _meter.CreateHistogram<double>("eduonline_request_duration_ms", unit: "ms", description: "Request duration in ms");

    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : Guid.NewGuid().ToString("N");

        context.TraceIdentifier = correlationId;
        context.Response.Headers[HeaderName] = correlationId;
        Activity.Current?.SetTag("correlation.id", correlationId);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        var start = Stopwatch.GetTimestamp();
        var traceId = Activity.Current?.TraceId.ToString() ?? string.Empty;

        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["TraceId"] = traceId
        }))
        {
            try
            {
                await next(context);
            }
            finally
            {
                var elapsedMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;

                // Record metrics for requests and duration
                _requestCounter.Add(1,
                    new KeyValuePair<string, object>("correlation_id", correlationId),
                    new KeyValuePair<string, object>("trace_id", traceId),
                    new KeyValuePair<string, object>("path", context.Request.Path.Value ?? string.Empty),
                    new KeyValuePair<string, object>("status_code", context.Response.StatusCode.ToString()));

                _requestDuration.Record(elapsedMs,
                    new KeyValuePair<string, object>("correlation_id", correlationId),
                    new KeyValuePair<string, object>("path", context.Request.Path.Value ?? string.Empty),
                    new KeyValuePair<string, object>("status_code", context.Response.StatusCode.ToString()));

                logger.LogInformation(
                    "Request completed. CorrelationId={CorrelationId} TraceId={TraceId} RequestPath={RequestPath} StatusCode={StatusCode} ElapsedMs={ElapsedMs}",
                    correlationId,
                    traceId,
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
