using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace EduOnline.Core.Api.Extensions;

public static class ObservabilityExtensions
{
    public static WebApplicationBuilder AddStructuredLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddJsonConsole(options =>
        {
            options.IncludeScopes = true;
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ ";
        });

        return builder;
    }

    public static WebApplicationBuilder AddApiHealthChecks(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("API disponível"));

        return builder;
    }

    public static WebApplicationBuilder AddApiHealthChecks<TDbContext>(this WebApplicationBuilder builder, bool includeRabbitMqWhenEnabled = false)
        where TDbContext : class
    {
        var healthChecks = builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("API disponível"))
            .AddCheck<DbContextConnectivityHealthCheck<TDbContext>>("database", tags: ["db"]);

        if (includeRabbitMqWhenEnabled)
        {
            healthChecks.AddCheck<RabbitMqTcpHealthCheck>("rabbitmq", tags: ["messaging"]);
        }

        return builder;
    }

    public static WebApplication UseApiHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("db") || check.Tags.Contains("messaging")
        });

        return app;
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
