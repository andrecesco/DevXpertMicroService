using EduOnline.Alunos.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace EduOnline.Alunos.ApiRest.Extensions;

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

        var context = scope.ServiceProvider.GetRequiredService<AlunosContext>();

        if (env.IsDevelopment())
        {
            await context.Database.MigrateAsync();
            await AlunoSeedHelper.SeedAsync(context, Guid.Parse("d9475e09-793f-4bd8-8f63-93e5038c0d16"));
        }
    }
}
