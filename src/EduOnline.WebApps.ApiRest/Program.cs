using EduOnline.Core.Api.Extensions;
using EduOnline.Core.Api.Identidade;
using EduOnline.WebApps.ApiRest.Configurations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddStructuredLogging()
    .AddApplicationObservability()
    .AddApiConfig()
    .AddJwtConfiguration()
    .RegisterServices()
    .AddApiHealthChecks()
    .AddSwaggerConfig();

builder.Services.AddMediatR(c => c.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "EduOnline Auth API v1");
        options.DisplayRequestDuration();
        options.EnableTryItOutByDefault();
    });
}

app.UseHttpsRedirection();

app.UseCors("Total");

app.UseRouting();

app.UseObservability();
app.UseCorrelationId();

app.UseAuthConfiguration();
app.UseApiHealthChecks();

app.MapControllers();

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program
{
    private Program() { }
}
