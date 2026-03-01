using EduOnline.Auth.ApiRest.Configurations;
using EduOnline.Auth.ApiRest.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddDatabaseSelector()
    .AddApiConfig()
    .RegisterServices()
    .AddIdentityConfig()
    .AddJwtConfig()
    .AddSwaggerConfig();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Total");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseDbMigrationHelper();

app.Run();
