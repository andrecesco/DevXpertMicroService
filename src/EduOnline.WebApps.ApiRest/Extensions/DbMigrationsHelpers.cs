using EduOnline.Alunos.Data.Context;
using EduOnline.Conteudos.Data.Context;
using EduOnline.Pagamentos.Data;
using EduOnline.WebApps.ApiRest.Data;
using Microsoft.EntityFrameworkCore;

namespace EduOnline.WebApps.ApiRest.Extensions;

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

        var contextId = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var contextAlunos = scope.ServiceProvider.GetRequiredService<AlunosContext>();
        var contextConteudos = scope.ServiceProvider.GetRequiredService<ConteudosContext>();
        var contextPagamentos = scope.ServiceProvider.GetRequiredService<PagamentosContext>();

        if (env.IsDevelopment())
        {
            await contextId.Database.MigrateAsync();
            await AdicionarAdministrador(contextId, scope.ServiceProvider);
            var alunoId = await AdicionarAluno(contextId, scope.ServiceProvider);

            await contextAlunos.Database.MigrateAsync();
            await contextAlunos.SeedAsync(alunoId);

            await contextConteudos.Database.MigrateAsync();
            await contextConteudos.SeedAsync();

            await contextPagamentos.Database.MigrateAsync();
        }
    }

    public async static Task<Guid> AdicionarAdministrador(ApplicationDbContext identityDb, IServiceProvider serviceProvider)
    {
        var userExists = await identityDb.Users.FirstOrDefaultAsync(a => a.UserName == "admin@admin.com");
        if (userExists is not null)
        {
            return Guid.Parse(userExists.Id);
        }

        var roleManager = serviceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Administrador"))
        {
            await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("Administrador"));
        }

        var idUsuario = Guid.NewGuid();
        await identityDb.Users.AddAsync(new Microsoft.AspNetCore.Identity.IdentityUser
        {
            Id = idUsuario.ToString(),
            UserName = "admin@admin.com",
            NormalizedUserName = "ADMIN@ADMIN.COM",
            Email = "admin@admin.com",
            NormalizedEmail = "ADMIN@ADMIN.COM",
            AccessFailedCount = 0,
            LockoutEnabled = false,
            PasswordHash = "AQAAAAIAAYagAAAAEA8BzmHCVEcOD+VNHR7Z91SjCRm9Zc4yodRPaowNC98ttq1IuwawRlqBzwUPidXCnw==",
            TwoFactorEnabled = false,
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        });

        await identityDb.SaveChangesAsync();

        //set role Administrador for user
        var userManager = serviceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser>>();
        var user = await userManager.FindByIdAsync(idUsuario.ToString());
        if (user != null)
        {
            await userManager.AddToRoleAsync(user, "Administrador");
        }

        return idUsuario;
    }

    public static async Task<Guid> AdicionarAluno(ApplicationDbContext identityDb, IServiceProvider serviceProvider)
    {
        var userExists = await identityDb.Users.FirstOrDefaultAsync(a => a.UserName == "aluno@aluno.com");
        if (userExists is not null)
        {
            return Guid.Parse(userExists.Id);
        }

        var roleManager = serviceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Aluno"))
        {
            await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("Aluno"));
        }

        var idUsuario = Guid.NewGuid();
        await identityDb.Users.AddAsync(new Microsoft.AspNetCore.Identity.IdentityUser
        {
            Id = idUsuario.ToString(),
            UserName = "aluno@aluno.com",
            NormalizedUserName = "ALUNO@ALUNO.COM",
            Email = "aluno@aluno.com",
            NormalizedEmail = "ALUNO@ALUNO.COM",
            AccessFailedCount = 0,
            LockoutEnabled = false,
            PasswordHash = "AQAAAAIAAYagAAAAEA8BzmHCVEcOD+VNHR7Z91SjCRm9Zc4yodRPaowNC98ttq1IuwawRlqBzwUPidXCnw==",
            TwoFactorEnabled = false,
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        });

        await identityDb.SaveChangesAsync();

        //set role Administrador for user
        var userManager = serviceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser>>();
        var user = await userManager.FindByIdAsync(idUsuario.ToString());
        if (user != null)
        {
            await userManager.AddToRoleAsync(user, "Aluno");
        }

        return idUsuario;
    }
}
