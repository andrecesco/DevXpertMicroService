using EduOnline.Auth.ApiRest.Configurations;
using EduOnline.Auth.ApiRest.Data;
using EduOnline.Auth.ApiRest.Extensions;
using EduOnline.Core.Api.Extensions;
using EduOnline.Core.Api.Identidade;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddStructuredLogging()
    .AddApplicationObservability()
    .AddIdentityConfig()
    .AddJwtConfiguration()
    .AddDatabaseSelector()
    .AddApiConfig()
    .RegisterServices()
    .AddApiHealthChecks<ApplicationDbContext>()
    .AddSwaggerConfig();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "EduOnline Auth API v1");
        options.RoutePrefix = "swagger";
        options.DisplayRequestDuration();
        options.EnableTryItOutByDefault();
    });
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseObservability();
//app.UseCors("Total");

app.UseCorrelationId();

app.UseAuthConfiguration();

app.MapControllers();
app.UseApiHealthChecks();

app.UseDbMigrationHelper();

app.Run();
