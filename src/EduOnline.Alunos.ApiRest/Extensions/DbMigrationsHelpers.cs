using EduOnline.Alunos.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Alunos.ApiRest.Extensions;

[ExcludeFromCodeCoverage]
public static class DbMigrationsHelpers
{
    private const string DefaultAlunoId = "d9475e09-793f-4bd8-8f63-93e5038c0d16";
    private const string DefaultAlunoNome = "Aluno Teste";
    private const string DefaultAlunoEmail = "aluno@eduonline.com";

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
        var context = scope.ServiceProvider.GetRequiredService<AlunosContext>();

        if (env.IsDevelopment() || env.IsEnvironment("Testing"))
        {
            await context.Database.EnsureCreatedAsync();

            var enableSeedData = ObterBoolean(configuration, "SeedSettings:EnableSeedData", true);
            if (enableSeedData)
            {
                var alunoId = ObterAlunoSeedId(configuration);
                var alunoNome = configuration["SeedSettings:AlunoNomePadrao"] ?? DefaultAlunoNome;
                var alunoEmail = configuration["SeedSettings:AlunoEmailPadrao"] ?? DefaultAlunoEmail;

                await AlunoSeedHelper.SeedAsync(context, alunoId, alunoNome, alunoEmail);
            }
        }
    }

    private static Guid ObterAlunoSeedId(IConfiguration configuration)
    {
        var raw = configuration["SeedSettings:AlunoIdPadrao"];
        return Guid.TryParse(raw, out var id)
            ? id
            : Guid.Parse(DefaultAlunoId);
    }

    private static bool ObterBoolean(IConfiguration configuration, string key, bool defaultValue)
    {
        var raw = configuration[key];
        return bool.TryParse(raw, out var value)
            ? value
            : defaultValue;
    }
}
