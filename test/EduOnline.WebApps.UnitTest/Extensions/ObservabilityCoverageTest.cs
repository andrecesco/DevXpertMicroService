using EduOnline.Core.Api.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EduOnline.WebApps.UnitTest.Extensions;

public class ObservabilityCoverageTest
{
    [Fact]
    public async Task AddApiHealthChecks_SemDbContextRegistrado_DeveMarcarReadinessComoUnhealthy()
    {
        var app = CriarApp(builder => builder.AddApiHealthChecks<HealthDbContext>());
        var healthCheckService = app.Services.GetRequiredService<HealthCheckService>();

        var report = await healthCheckService.CheckHealthAsync(registration => registration.Tags.Contains("ready"));

        Assert.Equal(HealthStatus.Unhealthy, report.Status);
        Assert.Equal("DbContext HealthDbContext não registrado.", report.Entries["database"].Description);
    }

    [Fact]
    public async Task AddApiHealthChecks_ComClasseSemDatabaseFacade_DeveMarcarReadinessComoUnhealthy()
    {
        var app = CriarApp(builder =>
        {
            builder.Services.AddSingleton(new PlainHealthDependency());
            builder.AddApiHealthChecks<PlainHealthDependency>();
        });
        var healthCheckService = app.Services.GetRequiredService<HealthCheckService>();

        var report = await healthCheckService.CheckHealthAsync(registration => registration.Tags.Contains("ready"));

        Assert.Equal(HealthStatus.Unhealthy, report.Status);
        Assert.Equal("DbContext PlainHealthDependency sem facade de banco.", report.Entries["database"].Description);
    }

    [Fact]
    public async Task AddApiHealthChecks_ComDbContextValido_DeveMarcarReadinessComoHealthy()
    {
        var app = CriarApp(builder =>
        {
            builder.Services.AddDbContext<HealthDbContext>(options => options.UseInMemoryDatabase("health-db"));
            builder.AddApiHealthChecks<HealthDbContext>();
        });
        var healthCheckService = app.Services.GetRequiredService<HealthCheckService>();

        var report = await healthCheckService.CheckHealthAsync(registration => registration.Tags.Contains("ready"));

        Assert.Equal(HealthStatus.Healthy, report.Status);
        Assert.Equal("Conexão com banco OK", report.Entries["database"].Description);
    }

    [Fact]
    public async Task AddApiHealthChecks_ComRabbitMqDesabilitado_DeveConsiderarDependenciaComoHealthy()
    {
        var app = CriarApp(
            builder =>
            {
                builder.Services.AddDbContext<HealthDbContext>(options => options.UseInMemoryDatabase("health-rabbit-disabled"));
                builder.AddApiHealthChecks<HealthDbContext>(includeRabbitMqWhenEnabled: true);
            },
            new Dictionary<string, string?>
            {
                ["RabbitMq:Enabled"] = "false"
            });
        var healthCheckService = app.Services.GetRequiredService<HealthCheckService>();

        var report = await healthCheckService.CheckHealthAsync(registration => registration.Tags.Contains("ready"));

        Assert.Equal(HealthStatus.Healthy, report.Status);
        Assert.Equal("RabbitMQ desabilitado para este ambiente", report.Entries["rabbitmq"].Description);
    }

    [Fact]
    public async Task AddApiHealthChecks_ComRabbitMqInvalido_DeveMarcarReadinessComoUnhealthy()
    {
        var app = CriarApp(
            builder =>
            {
                builder.Services.AddDbContext<HealthDbContext>(options => options.UseInMemoryDatabase("health-rabbit-invalid"));
                builder.AddApiHealthChecks<HealthDbContext>(includeRabbitMqWhenEnabled: true);
            },
            new Dictionary<string, string?>
            {
                ["RabbitMq:Enabled"] = "true",
                ["RabbitMq:HostName"] = "127.0.0.1",
                ["RabbitMq:Port"] = "1"
            });
        var healthCheckService = app.Services.GetRequiredService<HealthCheckService>();

        var report = await healthCheckService.CheckHealthAsync(registration => registration.Tags.Contains("ready"));

        Assert.Equal(HealthStatus.Unhealthy, report.Status);
        Assert.Contains("RabbitMQ", report.Entries["rabbitmq"].Description);
    }

    private static WebApplication CriarApp(Action<WebApplicationBuilder> configureBuilder, IDictionary<string, string?>? configuration = null)
    {
        var builder = WebApplication.CreateBuilder();

        if (configuration is not null)
        {
            builder.Configuration.AddInMemoryCollection(configuration);
        }

        configureBuilder(builder);

        return builder.Build();
    }

    private sealed class PlainHealthDependency;

    private sealed class HealthDbContext(DbContextOptions<HealthDbContext> options) : DbContext(options);
}
