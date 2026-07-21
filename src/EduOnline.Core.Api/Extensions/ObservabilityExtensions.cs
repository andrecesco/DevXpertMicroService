using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using System.Net.Sockets;
using System.Reflection;

namespace EduOnline.Core.Api.Extensions;

public static class ObservabilityExtensions
{
    public static WebApplicationBuilder AddStructuredLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.Configure(options =>
        {
            options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId |
                                              ActivityTrackingOptions.SpanId |
                                              ActivityTrackingOptions.ParentId |
                                              ActivityTrackingOptions.Baggage;
        });

        builder.Logging.AddJsonConsole(options =>
        {
            options.IncludeScopes = true;
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ ";
        });

        return builder;
    }

    public static WebApplicationBuilder AddApplicationObservability(this WebApplicationBuilder builder)
    {
        var serviceName = builder.Environment.ApplicationName;
        var serviceVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
            ?? typeof(ObservabilityExtensions).Assembly.GetName().Version?.ToString()
            ?? "unknown";

        builder.Services.UseHttpClientMetrics();

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName, serviceVersion: serviceVersion))
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                });

                tracing.AddHttpClientInstrumentation();
                tracing.AddSource(serviceName);

                if (builder.Environment.IsDevelopment())
                {
                    tracing.AddConsoleExporter();
                }

                if (TryGetOtlpEndpoint(builder.Configuration, out var endpoint))
                {
                    tracing.AddOtlpExporter(options => options.Endpoint = endpoint);
                }
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddHttpClientInstrumentation();
                metrics.AddRuntimeInstrumentation();

                if (builder.Environment.IsDevelopment())
                {
                    metrics.AddConsoleExporter();
                }

                if (TryGetOtlpEndpoint(builder.Configuration, out var endpoint))
                {
                    metrics.AddOtlpExporter(options => options.Endpoint = endpoint);
                }
            });

        return builder;
    }

    public static WebApplicationBuilder AddApiHealthChecks(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("API disponível"), tags: ["live", "ready"])
            .ForwardToPrometheus();

        return builder;
    }

    public static WebApplicationBuilder AddApiHealthChecks<TDbContext>(this WebApplicationBuilder builder, bool includeRabbitMqWhenEnabled = false)
        where TDbContext : class
    {
        var healthChecks = builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("API disponível"), tags: ["live", "ready"])
            .AddCheck<DbContextConnectivityHealthCheck<TDbContext>>("database", tags: ["ready", "db"]);

        if (includeRabbitMqWhenEnabled)
        {
            healthChecks.AddCheck<RabbitMqTcpHealthCheck>("rabbitmq", tags: ["ready", "messaging"]);
        }

        healthChecks.ForwardToPrometheus();

        return builder;
    }

    public static WebApplication UseObservability(this WebApplication app)
    {
        app.UseHttpMetrics();
        app.MapMetrics();

        return app;
    }

    public static WebApplication UseApiHealthChecks(this WebApplication app)
    {
        // Agregado: todos os checks, útil para dashboards/diagnóstico manual - não usar em probes do Kubernetes.
        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Readiness: inclui dependências externas (banco, RabbitMQ) - falha remove o pod do Service sem reiniciá-lo.
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Liveness: só confirma que o processo está respondendo - não deve depender de recursos externos.
        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return app;
    }

    private static bool TryGetOtlpEndpoint(IConfiguration configuration, out Uri endpoint)
    {
        var endpointValue = configuration["Observability:OtlpEndpoint"]
            ?? configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

        if (!string.IsNullOrWhiteSpace(endpointValue) && Uri.TryCreate(endpointValue, UriKind.Absolute, out endpoint))
        {
            return true;
        }

        endpoint = null!;
        return false;
    }

    private sealed class DbContextConnectivityHealthCheck<TDbContext>(IServiceScopeFactory scopeFactory) : IHealthCheck
        where TDbContext : class
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<TDbContext>();

            if (dbContext is null)
                return HealthCheckResult.Unhealthy($"DbContext {typeof(TDbContext).Name} não registrado.");

            var databaseProperty = typeof(TDbContext).GetProperty("Database");
            if (databaseProperty?.GetValue(dbContext) is not Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database)
                return HealthCheckResult.Unhealthy($"DbContext {typeof(TDbContext).Name} sem facade de banco.");

            try
            {
                var canConnect = await database.CanConnectAsync(cancellationToken);
                return canConnect
                    ? HealthCheckResult.Healthy("Conexão com banco OK")
                    : HealthCheckResult.Unhealthy("Falha ao conectar no banco");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Erro ao validar conexão com banco", ex);
            }
        }
    }

    private sealed class RabbitMqTcpHealthCheck(IConfiguration configuration) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var enabled = bool.TryParse(configuration["RabbitMq:Enabled"], out var isEnabled) && isEnabled;
            if (!enabled)
                return HealthCheckResult.Healthy("RabbitMQ desabilitado para este ambiente");

            var host = configuration["RabbitMq:HostName"] ?? "localhost";
            var port = int.TryParse(configuration["RabbitMq:Port"], out var parsedPort) ? parsedPort : 5672;

            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(host, port, cancellationToken).AsTask();
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                var completed = await Task.WhenAny(connectTask, timeoutTask);
                if (completed != connectTask || !client.Connected)
                    return HealthCheckResult.Unhealthy($"RabbitMQ indisponível em {host}:{port}");

                return HealthCheckResult.Healthy($"RabbitMQ disponível em {host}:{port}");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Falha ao conectar ao RabbitMQ em {host}:{port}", ex);
            }
        }
    }
}
