using EduOnline.Auth.ApiRest.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduOnline.Auth.ApiRest.IntegrationTest;

public class AuthApiTestFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@eduonline.com";
    public const string AlunoEmail = "aluno@eduonline.com";
    public const string Password = "Teste@123";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var dbFile = Path.Combine(Path.GetTempPath(), $"auth-it-{Guid.NewGuid():N}.db");

        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnectionLite"] = $"Data Source={dbFile}",
                ["SeedSettings:EnableMigrations"] = "false",
                ["SeedSettings:EnableSeedData"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            using var scope = services.BuildServiceProvider().CreateScope();
            var provider = scope.ServiceProvider;

            var dbContext = provider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureCreated();

            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = provider.GetRequiredService<UserManager<EduOnlineUser>>();

            SeedIdentityData(roleManager, userManager).GetAwaiter().GetResult();
        });
    }

    public async Task ExpireRefreshTokenAsync(Guid token)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var refreshToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token);
        if (refreshToken is null) return;

        refreshToken.ExpirationDate = DateTime.UtcNow.AddMinutes(-5);
        await dbContext.SaveChangesAsync();
    }

    public async Task<string?> GetUserIdByEmailAsync(string email)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<EduOnlineUser>>();
        var user = await userManager.FindByEmailAsync(email);
        return user?.Id;
    }

    private static async Task SeedIdentityData(RoleManager<IdentityRole> roleManager, UserManager<EduOnlineUser> userManager)
    {
        if (!await roleManager.RoleExistsAsync("Administrador"))
            await roleManager.CreateAsync(new IdentityRole("Administrador"));

        if (!await roleManager.RoleExistsAsync("Aluno"))
            await roleManager.CreateAsync(new IdentityRole("Aluno"));

        var admin = await userManager.FindByEmailAsync(AdminEmail);
        if (admin is null)
        {
            admin = new EduOnlineUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
                StatusId = Status.Cadastrado.Id,
                StatusNome = Status.Cadastrado.Nome
            };

            var createResult = await userManager.CreateAsync(admin, Password);
            if (!createResult.Succeeded)
                throw new InvalidOperationException("Falha ao criar usuário admin para testes.");

            await userManager.AddToRoleAsync(admin, "Administrador");
        }

        var aluno = await userManager.FindByEmailAsync(AlunoEmail);
        if (aluno is null)
        {
            aluno = new EduOnlineUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = AlunoEmail,
                Email = AlunoEmail,
                EmailConfirmed = true,
                StatusId = Status.Cadastrado.Id,
                StatusNome = Status.Cadastrado.Nome
            };

            var createResult = await userManager.CreateAsync(aluno, Password);
            if (!createResult.Succeeded)
                throw new InvalidOperationException("Falha ao criar usuário aluno para testes.");

            await userManager.AddToRoleAsync(aluno, "Aluno");
        }
    }
}
