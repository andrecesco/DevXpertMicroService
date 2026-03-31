using EduOnline.Conteudos.ApiRest.Configurations;
using EduOnline.Conteudos.ApiRest.Extensions;
using EduOnline.Conteudos.Data.Context;
using EduOnline.Core.Api.Identidade;
using EduOnline.Core.Api.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddStructuredLogging()
    .AddApiConfig()
    .AddJwtConfiguration()
    .AddDatabaseSelector()
    .RegisterServices()
    .AddApiHealthChecks<ConteudosContext>()
    .AddSwaggerConfig();

builder.Services.AddMediatR(c => c.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
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

app.UseCorrelationId();

app.UseAuthConfiguration();

app.MapControllers();
app.UseApiHealthChecks();

app.UseDbMigrationHelper();

app.Run();
