using EduOnline.Core.Api.Extensions;
using EduOnline.Core.Api.Identidade;
using EduOnline.Pagamentos.ApiRest.Configurations;
using EduOnline.Pagamentos.ApiRest.Extensions;
using EduOnline.Pagamentos.Data;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddStructuredLogging()
    .AddApplicationObservability()
    .AddApiConfig()
    .AddJwtConfiguration()
    .AddDatabaseSelector()
    .ValidateRabbitMqWhenEnabled()
    .RegisterServices()
    .AddApiHealthChecks<PagamentosContext>(includeRabbitMqWhenEnabled: false)
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

app.MapControllers();
app.UseApiHealthChecks();

app.UseDbMigrationHelper();

app.Run();
