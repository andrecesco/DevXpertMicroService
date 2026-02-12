using EduOnline.Alunos.Application.Automapper;
using EduOnline.WebApps.ApiRest.Configurations;
using EduOnline.WebApps.ApiRest.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddDatabaseSelector()
    .AddApiConfig()
    .RegisterServices()
    .AddIdentityConfig()
    .AddJwtConfig()
    .AddSwaggerConfig();

builder.Services.AddAutoMapperApplication();

builder.Services.AddMediatR(c => c.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
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

public partial class Program
{
    private Program() { }
}
