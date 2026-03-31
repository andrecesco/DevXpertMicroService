using System.Diagnostics.CodeAnalysis;
using EduOnline.Auth.ApiRest.Data;
using Microsoft.EntityFrameworkCore;

namespace EduOnline.Auth.ApiRest.Configurations;

[ExcludeFromCodeCoverage]
public static class DatabaseSelectExtension
{
    public static WebApplicationBuilder AddDatabaseSelector(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionLite") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));
        }
        else
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

        return builder;
    }
}
