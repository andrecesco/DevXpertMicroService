using EduOnline.Alunos.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace EduOnline.Alunos.ApiRest.Configurations;

public static class DatabaseSelectExtension
{
    public static WebApplicationBuilder AddDatabaseSelector(this WebApplicationBuilder builder)
    {
        if (!builder.Environment.IsDevelopment())
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionLite")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnectionLite' not found.");
            builder.Services.AddDbContext<AlunosContext>(options =>
                options.UseSqlite(connectionString));
        }
        else
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<AlunosContext>(options =>
                options.UseSqlServer(connectionString));
        }

        return builder;
    }
}
