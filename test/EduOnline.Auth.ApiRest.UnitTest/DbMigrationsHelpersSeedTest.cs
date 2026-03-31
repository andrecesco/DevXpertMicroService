using EduOnline.Auth.ApiRest.Data;
using EduOnline.Auth.ApiRest.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EduOnline.Auth.ApiRest.UnitTest;

public class DbMigrationsHelpersSeedTest
{
    [Fact(DisplayName = "AdicionarAdministrador não deve criar usuário quando e-mail já existir")]
    public async Task AdicionarAdministrador_QuandoUsuarioExiste_NaoDeveCriar()
    {
        var userManagerMock = CreateUserManagerMock();
        var roleManagerMock = CreateRoleManagerMock();

        userManagerMock
            .Setup(x => x.FindByEmailAsync("admin@eduonline.com"))
            .ReturnsAsync(new EduOnlineUser { Email = "admin@eduonline.com", UserName = "admin@eduonline.com" });

        var services = CreateServices(userManagerMock.Object, roleManagerMock.Object);

        await DbMigrationsHelpers.AdicionarAdministrador(services, "admin@eduonline.com", "Teste@123");

        roleManagerMock.Verify(x => x.RoleExistsAsync(It.IsAny<string>()), Times.Never);
        userManagerMock.Verify(x => x.CreateAsync(It.IsAny<EduOnlineUser>(), It.IsAny<string>()), Times.Never);
        userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<EduOnlineUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact(DisplayName = "AdicionarAdministrador deve criar role e usuário quando não existir")]
    public async Task AdicionarAdministrador_QuandoNaoExiste_DeveCriarRoleEUsuario()
    {
        var userManagerMock = CreateUserManagerMock();
        var roleManagerMock = CreateRoleManagerMock();

        userManagerMock
            .Setup(x => x.FindByEmailAsync("admin@eduonline.com"))
            .ReturnsAsync((EduOnlineUser?)null);

        roleManagerMock
            .Setup(x => x.RoleExistsAsync("Administrador"))
            .ReturnsAsync(false);

        roleManagerMock
            .Setup(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == "Administrador")))
            .ReturnsAsync(IdentityResult.Success);

        userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<EduOnlineUser>(), "Teste@123"))
            .ReturnsAsync(IdentityResult.Success);

        userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<EduOnlineUser>(), "Administrador"))
            .ReturnsAsync(IdentityResult.Success);

        var services = CreateServices(userManagerMock.Object, roleManagerMock.Object);

        await DbMigrationsHelpers.AdicionarAdministrador(services, "admin@eduonline.com", "Teste@123");

        roleManagerMock.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == "Administrador")), Times.Once);
        userManagerMock.Verify(x => x.CreateAsync(It.Is<EduOnlineUser>(u =>
            u.Email == "admin@eduonline.com" &&
            u.UserName == "admin@eduonline.com" &&
            u.StatusId == Status.Cadastrado.Id), "Teste@123"), Times.Once);
        userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<EduOnlineUser>(), "Administrador"), Times.Once);
    }

    [Fact(DisplayName = "AdicionarAluno deve usar GUID informado como Id do usuário")]
    public async Task AdicionarAluno_DeveUsarGuidInformadoComoId()
    {
        var userManagerMock = CreateUserManagerMock();
        var roleManagerMock = CreateRoleManagerMock();
        var alunoId = Guid.NewGuid();

        userManagerMock
            .Setup(x => x.FindByEmailAsync("aluno@eduonline.com"))
            .ReturnsAsync((EduOnlineUser?)null);

        roleManagerMock
            .Setup(x => x.RoleExistsAsync("Aluno"))
            .ReturnsAsync(true);

        userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<EduOnlineUser>(), "Teste@123"))
            .ReturnsAsync(IdentityResult.Success);

        userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<EduOnlineUser>(), "Aluno"))
            .ReturnsAsync(IdentityResult.Success);

        var services = CreateServices(userManagerMock.Object, roleManagerMock.Object);

        await DbMigrationsHelpers.AdicionarAluno(services, alunoId, "aluno@eduonline.com", "Teste@123");

        userManagerMock.Verify(x => x.CreateAsync(It.Is<EduOnlineUser>(u =>
            u.Id == alunoId.ToString() &&
            u.Email == "aluno@eduonline.com" &&
            u.UserName == "aluno@eduonline.com"), "Teste@123"), Times.Once);
        userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<EduOnlineUser>(), "Aluno"), Times.Once);
    }

    [Fact(DisplayName = "AdicionarAluno deve lançar exceção quando criação do usuário falhar")]
    public async Task AdicionarAluno_QuandoCriacaoFalhar_DeveLancarExcecao()
    {
        var userManagerMock = CreateUserManagerMock();
        var roleManagerMock = CreateRoleManagerMock();

        userManagerMock
            .Setup(x => x.FindByEmailAsync("aluno@eduonline.com"))
            .ReturnsAsync((EduOnlineUser?)null);

        roleManagerMock
            .Setup(x => x.RoleExistsAsync("Aluno"))
            .ReturnsAsync(true);

        userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<EduOnlineUser>(), "Teste@123"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "erro ao criar" }));

        var services = CreateServices(userManagerMock.Object, roleManagerMock.Object);

        var action = async () => await DbMigrationsHelpers.AdicionarAluno(services, Guid.NewGuid(), "aluno@eduonline.com", "Teste@123");

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Falha ao criar usuário aluno de seed*");
    }

    private static IServiceProvider CreateServices(UserManager<EduOnlineUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        var services = new ServiceCollection();
        services.AddSingleton(userManager);
        services.AddSingleton(roleManager);
        return services.BuildServiceProvider();
    }

    private static Mock<UserManager<EduOnlineUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<EduOnlineUser>>();
        return new Mock<UserManager<EduOnlineUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }

    private static Mock<RoleManager<IdentityRole>> CreateRoleManagerMock()
    {
        var roleStore = new Mock<IRoleStore<IdentityRole>>();

        return new Mock<RoleManager<IdentityRole>>(
            roleStore.Object,
            Array.Empty<IRoleValidator<IdentityRole>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new Mock<Microsoft.Extensions.Logging.ILogger<RoleManager<IdentityRole>>>().Object);
    }
}
