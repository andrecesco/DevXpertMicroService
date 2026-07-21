using EduOnline.Api.Status;
using HealthChecks.UI.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Status API disponível"), tags: ["live", "ready"]);

var healthCheckBuilder = builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetHeaderText("EduOnline - Status Page");

    var endpoints = builder.Configuration.GetSection("ENDPOINTS").Get<string>() ?? throw (new Exception("Nenhum endpoint encontrado!"));

    foreach (var endpoint in endpoints.Split(";"))
    {
        var name = endpoint.Split('|')[0];
        var uri = endpoint.Split('|')[1];

        setup.AddHealthCheckEndpoint(name, uri);
    }

    setup.UseApiEndpointHttpMessageHandler(sp => HttpExtensions.ConfigureClientHandler());
});

healthCheckBuilder.AddInMemoryStorage();

var app = builder.Build();

// Under certain scenarios, e.g minikube / linux environment / behind load balancer
// https redirection could lead dev's to over complicated configuration for testing purpouses
// In production is a good practice to keep it true
if (app.Configuration["USE_HTTPS_REDIRECTION"] == "true")
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Under certain scenarios, e.g minikube / linux environment / behind load balancer
// https redirection could lead dev's to over complicated configuration for testing purpouses
// In production is a good practice to keep it true
if (app.Configuration["USE_HTTPS_REDIRECTION"] == "true")
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.MapHealthChecksUI(setup =>
{
    setup.AddCustomStylesheet("eduonline.css");
    setup.UIPath = "/";
    setup.PageTitle = "EduOnline - Status";
});
app.Run();
