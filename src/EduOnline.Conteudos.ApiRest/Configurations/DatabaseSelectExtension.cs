using EduOnline.Conteudos.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Conteudos.ApiRest.Configurations;

[ExcludeFromCodeCoverage]
public static class DatabaseSelectExtension
{
    public static WebApplicationBuilder AddDatabaseSelector(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionLite") ?? throw new InvalidOperationException("Connection string 'DefaultConnectionLite' not found.");
            builder.Services.AddDbContext<ConteudosContext>(options =>
                options.UseSqlite(connectionString));
        }
        else
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ConteudosContext>(options =>
                options.UseSqlServer(connectionString));
        }

        return builder;
    }
}
