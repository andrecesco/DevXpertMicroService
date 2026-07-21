extern alias alunosApi;

using EduOnline.Auth.ApiRest.Data;
using EduOnline.Auth.ApiRest.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;

namespace EduOnline.Alunos.IntegrationTest;

public class AlunosApiTestFactory : WebApplicationFactory<alunosApi::Program>
{
    public const string AdminEmail = "admin@eduonline.com";
    public const string AlunoEmail = "aluno@eduonline.com";
    public const string Password = "Teste@123";
    private static readonly Guid _testRunId = Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var authDbFile = Path.Combine(Path.GetTempPath(), $"alunos-auth-it-{_testRunId:N}.db");

        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SeedSettings:EnableMigrations"] = "false",
                ["SeedSettings:EnableSeedData"] = "true",
                ["SeedSettings:AlunoIdPadrao"] = "d9475e09-793f-4bd8-8f63-93e5038c0d16",
                ["SeedSettings:AlunoEmailPadrao"] = AlunoEmail,
                ["SeedSettings:AlunoNomePadrao"] = "Aluno Teste",
                ["AppTokenSettings:Segredo"] = "ChaveSuperSecretaParaJWT_2024_EduOnline_MinhaChaveDeve_TerMaisde32Caracteres!@#$%^&*",
                ["AppTokenSettings:Issuer"] = "https://localhost:7020",
                ["AppTokenSettings:Audience"] = "EduOnline-Dev",
                ["AppTokenSettings:RefreshTokenExpiration"] = "8"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remover o registro anterior do DbContext (se houver)
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Adicionar DbContext do Identity (Auth) com nome único
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                options.UseSqlite($"Data Source={authDbFile}");
            });

            services.AddOptions<EduOnline.Auth.ApiRest.Extensions.AppTokenSettings>()
                .BindConfiguration("AppTokenSettings");

            // Adicionar Identity
            services.AddIdentity<EduOnlineUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Adicionar serviços de Auth
            services.AddScoped<AuthenticationService>();
            services.RemoveAll<AlunoProvisioningService>();
            services.AddScoped<AlunoProvisioningService>(sp =>
                new AlunoProvisioningService(new HttpClient(new SuccessHttpMessageHandler())
                {
                    BaseAddress = new Uri("https://localhost:7254/api/alunos/")
                }));

            // Adicionar controllers de Auth
            services.AddControllers()
                .AddApplicationPart(typeof(EduOnline.Auth.ApiRest.Controllers.AuthController).Assembly);
        });
    }

    public async Task InitializeAuthDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var provider = scope.ServiceProvider;

        var authDbContext = provider.GetRequiredService<ApplicationDbContext>();
        await authDbContext.Database.EnsureCreatedAsync();

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<EduOnlineUser>>();

        await SeedIdentityData(roleManager, userManager);
    }

    private static async Task SeedIdentityData(RoleManager<IdentityRole> roleManager, UserManager<EduOnlineUser> userManager)
    {
        // Criar roles
        if (!await roleManager.RoleExistsAsync("Administrador"))
            await roleManager.CreateAsync(new IdentityRole("Administrador"));

        if (!await roleManager.RoleExistsAsync("Aluno"))
            await roleManager.CreateAsync(new IdentityRole("Aluno"));

        // Criar usuário admin
        var adminUser = await userManager.FindByEmailAsync(AdminEmail);
        if (adminUser is null)
        {
            adminUser = new EduOnlineUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, Password);
            await userManager.AddToRoleAsync(adminUser, "Administrador");
        }

        // Criar usuário aluno
        var alunoUser = await userManager.FindByEmailAsync(AlunoEmail);
        if (alunoUser is null)
        {
            alunoUser = new EduOnlineUser
            {
                Id = "d9475e09-793f-4bd8-8f63-93e5038c0d16", // Corresponde ao Memo do aluno seeded
                UserName = AlunoEmail,
                Email = AlunoEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(alunoUser, Password);
            await userManager.AddToRoleAsync(alunoUser, "Aluno");
        }
    }

    private class SuccessHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
