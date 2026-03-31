using EduOnline.Pagamentos.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Pagamentos.ApiRest.Extensions;

[ExcludeFromCodeCoverage]
public static class DbMigrationsHelpers
{
    public static void UseDbMigrationHelper(this WebApplication app)
    {
        EnsureSeedData(app).Wait();
    }

    public static async Task EnsureSeedData(WebApplication serviceScope)
    {
        var services = serviceScope.Services.CreateScope().ServiceProvider;

        using var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var context = scope.ServiceProvider.GetRequiredService<PagamentosContext>();

        if (env.IsDevelopment())
        {
            var enableMigrations = ObterBoolean(configuration, "SeedSettings:EnableMigrations", true);
            if (enableMigrations)
            {
                await context.Database.MigrateAsync();
            }
        }
    }

    private static bool ObterBoolean(IConfiguration configuration, string key, bool defaultValue)
    {
        var raw = configuration[key];
        return bool.TryParse(raw, out var value)
            ? value
            : defaultValue;
    }
}
