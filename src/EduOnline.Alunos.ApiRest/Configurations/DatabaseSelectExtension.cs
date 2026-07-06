using EduOnline.Alunos.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Alunos.ApiRest.Configurations;

[ExcludeFromCodeCoverage]
public static class DatabaseSelectExtension
{
    public static WebApplicationBuilder AddDatabaseSelector(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionLite") ?? throw new InvalidOperationException("Connection string 'DefaultConnectionLite' not found.");
            builder.Services.AddDbContext<AlunosContext>(options =>
                options.UseSqlite(connectionString));
        }
        else
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<AlunosContext>(options =>
                options.UseSqlServer(connectionString));
        }

        return builder;
    }
}
