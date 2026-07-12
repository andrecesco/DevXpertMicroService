using EduOnline.Conteudos.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Conteudos.ApiRest.Extensions;

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
        var context = scope.ServiceProvider.GetRequiredService<ConteudosContext>();

        if (env.IsDevelopment() || env.IsEnvironment("Docker"))
        {
            var enableMigrations = ObterBoolean(configuration, "SeedSettings:EnableMigrations", false);
            if (enableMigrations)
            {
                await context.Database.MigrateAsync();
            }

            var enableSeedData = ObterBoolean(configuration, "SeedSettings:EnableSeedData", false);
            if (enableSeedData)
            {
                await ConteudoSeedHelper.SeedAsync(context);
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
