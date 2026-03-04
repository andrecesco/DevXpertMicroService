using EduOnline.Pagamentos.Data;
using Microsoft.EntityFrameworkCore;

namespace EduOnline.Pagamentos.ApiRest.Extensions;

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

        var context = scope.ServiceProvider.GetRequiredService<PagamentosContext>();

        if (env.IsDevelopment())
        {
            await context.Database.MigrateAsync();
        }
    }
}
