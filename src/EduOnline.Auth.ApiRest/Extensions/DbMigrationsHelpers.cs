using EduOnline.Auth.ApiRest.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Auth.ApiRest.Extensions;

[ExcludeFromCodeCoverage]
public static class DbMigrationsHelpers
{
    private const string DefaultAlunoId = "d9475e09-793f-4bd8-8f63-93e5038c0d16";
    private const string DefaultSeedPassword = "Teste@123";
    private const string DefaultAdminEmail = "admin@eduonline.com";
    private const string DefaultAlunoEmail = "aluno@eduonline.com";

    public static void UseDbMigrationHelper(this WebApplication app)
    {
        EnsureSeedData(app).Wait();
    }

    public static async Task EnsureSeedData(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var env = services.GetRequiredService<IWebHostEnvironment>();
        var context = services.GetRequiredService<ApplicationDbContext>();
        var configuration = services.GetRequiredService<IConfiguration>();

        if (env.IsDevelopment())
        {
            var enableMigrations = ObterBoolean(configuration, "SeedSettings:EnableMigrations", true);
            if (enableMigrations)
            {
                await context.Database.EnsureDeletedAsync();

                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    await context.Database.MigrateAsync();
                }
            }

            var enableSeedData = ObterBoolean(configuration, "SeedSettings:EnableSeedData", true);
            if (enableSeedData)
            {
                var seedPassword = configuration["SeedSettings:SenhaPadrao"] ?? DefaultSeedPassword;
                var adminEmail = configuration["SeedSettings:AdminEmailPadrao"] ?? DefaultAdminEmail;
                var alunoEmail = configuration["SeedSettings:AlunoEmailPadrao"] ?? DefaultAlunoEmail;
                var alunoId = ObterAlunoSeedId(configuration);

                await AdicionarAdministrador(services, adminEmail, seedPassword);
                await AdicionarAluno(services, alunoId, alunoEmail, seedPassword);
            }
        }
    }

    public static async Task AdicionarAdministrador(IServiceProvider services, string email, string senhaPadrao)
    {
        var userManager = services.GetRequiredService<UserManager<EduOnlineUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        var userExists = await userManager.FindByEmailAsync(email);
        if (userExists is not null) return;

        if (!await roleManager.RoleExistsAsync("Administrador"))
        {
            await roleManager.CreateAsync(new IdentityRole("Administrador"));
        }

        var adminUser = new EduOnlineUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            StatusId = Status.Cadastrado.Id,
            StatusNome = Status.Cadastrado.Nome
        };

        var createResult = await userManager.CreateAsync(adminUser, senhaPadrao);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException($"Falha ao criar usuário administrador de seed: {string.Join(" | ", createResult.Errors.Select(e => e.Description))}");
        }

        var addRoleResult = await userManager.AddToRoleAsync(adminUser, "Administrador");
        if (!addRoleResult.Succeeded)
        {
            throw new InvalidOperationException($"Falha ao vincular papel Administrador ao usuário de seed: {string.Join(" | ", addRoleResult.Errors.Select(e => e.Description))}");
        }
    }

    public static async Task AdicionarAluno(IServiceProvider services, Guid alunoId, string email, string senhaPadrao)
    {
        var userManager = services.GetRequiredService<UserManager<EduOnlineUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        var userExists = await userManager.FindByEmailAsync(email);
        if (userExists is not null) return;

        if (!await roleManager.RoleExistsAsync("Aluno"))
        {
            await roleManager.CreateAsync(new IdentityRole("Aluno"));
        }

        var alunoUser = new EduOnlineUser
        {
            Id = alunoId.ToString(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            StatusId = Status.Cadastrado.Id,
            StatusNome = Status.Cadastrado.Nome
        };

        var createResult = await userManager.CreateAsync(alunoUser, senhaPadrao);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException($"Falha ao criar usuário aluno de seed: {string.Join(" | ", createResult.Errors.Select(e => e.Description))}");
        }

        var addRoleResult = await userManager.AddToRoleAsync(alunoUser, "Aluno");
        if (!addRoleResult.Succeeded)
        {
            throw new InvalidOperationException($"Falha ao vincular papel Aluno ao usuário de seed: {string.Join(" | ", addRoleResult.Errors.Select(e => e.Description))}");
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
