using EduOnline.Conteudos.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace EduOnline.Conteudos.ApiRest.Extensions;

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

        var context = scope.ServiceProvider.GetRequiredService<ConteudosContext>();

        if (env.IsDevelopment())
        {
            await context.Database.MigrateAsync();
            await ConteudoSeedHelper.SeedAsync(context);
        }
    }
}
