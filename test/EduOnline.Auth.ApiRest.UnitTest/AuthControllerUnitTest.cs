using EduOnline.Auth.ApiRest.Controllers;
using EduOnline.Auth.ApiRest.Data;
using EduOnline.Auth.ApiRest.Extensions;
using EduOnline.Auth.ApiRest.Models;
using EduOnline.Auth.ApiRest.Services;
using EduOnline.Core.Api.Controllers;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Mensagens;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace EduOnline.Auth.ApiRest.UnitTest;

public class AuthControllerUnitTest
{
    [Fact(DisplayName = "Registrar deve retornar BadRequest quando ModelState for inválido")]
    public async Task Registrar_ModelStateInvalido_DeveRetornarBadRequest()
    {
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);
        var context = CreateContext();
        var options = Options.Create(CreateTokenSettings());

        var service = new AuthenticationService(
            signInManager.Object,
            userManager.Object,
            options,
            context,
            new Mock<IAspNetUser>().Object);

        var roleManager = CreateRoleManagerMock();
        var notificador = new InMemoryNotificador();
        var appUser = Mock.Of<IAspNetUser>(x => x.IsAuthenticated() == false);
        var logger = new Mock<ILogger<AuthController>>();

        var controller = new AuthController(notificador, service, appUser, roleManager.Object, logger.Object);
        controller.ModelState.AddModelError("Email", "Email é obrigatório");

        var result = await controller.Registrar(new UsuarioRegistroModel
        {
            Nome = "Teste",
            Email = "",
            Senha = "Teste@123",
            ConfirmaSenha = "Teste@123",
            Perfil = "Aluno"
        });

        result.Should().BeOfType<BadRequestObjectResult>();
        userManager.Verify(x => x.CreateAsync(It.IsAny<EduOnlineUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact(DisplayName = "Login deve retornar BadRequest quando usuário estiver bloqueado")]
    public async Task Login_UsuarioBloqueado_DeveRetornarBadRequest()
    {
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);
        var context = CreateContext();
        var options = Options.Create(CreateTokenSettings());

        var email = "admin@eduonline.com";
        var user = new EduOnlineUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            Email = email,
            StatusId = Status.Cadastrado.Id,
            StatusNome = Status.Cadastrado.Nome
        };

        signInManager
            .Setup(x => x.PasswordSignInAsync(email, "Teste@123", false, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

        userManager
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        var service = new AuthenticationService(
            signInManager.Object,
            userManager.Object,
            options,
            context,
            new Mock<IAspNetUser>().Object);

        var roleManager = CreateRoleManagerMock();
        var notificador = new InMemoryNotificador();
        var appUser = Mock.Of<IAspNetUser>(x => x.IsAuthenticated() == false);
        var logger = new Mock<ILogger<AuthController>>();

        var controller = new AuthController(notificador, service, appUser, roleManager.Object, logger.Object);

        var result = await controller.Login(new UsuarioLoginModel { Email = email, Senha = "Teste@123" });

        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)result;
        var payload = badRequest.Value.Should().BeOfType<ResponseResult>().Subject;
        payload.Success.Should().BeFalse();
    }

    [Fact(DisplayName = "Excluir deve retornar NotFound quando usuário não existir")]
    public async Task Excluir_UsuarioNaoExiste_DeveRetornarNotFound()
    {
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);
        var context = CreateContext();
        var options = Options.Create(CreateTokenSettings());

        userManager
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((EduOnlineUser?)null);

        var service = new AuthenticationService(
            signInManager.Object,
            userManager.Object,
            options,
            context,
            new Mock<IAspNetUser>().Object);

        var roleManager = CreateRoleManagerMock();
        var notificador = new InMemoryNotificador();
        var appUser = Mock.Of<IAspNetUser>(x => x.IsAuthenticated() == true);
        var logger = new Mock<ILogger<AuthController>>();

        var controller = new AuthController(notificador, service, appUser, roleManager.Object, logger.Object);

        var result = await controller.Excluir(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact(DisplayName = "Excluir deve retornar BadRequest quando usuário não estiver pendente")]
    public async Task Excluir_UsuarioNaoPendente_DeveRetornarBadRequest()
    {
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);
        var context = CreateContext();
        var options = Options.Create(CreateTokenSettings());

        var id = Guid.NewGuid().ToString();
        var user = new EduOnlineUser
        {
            Id = id,
            UserName = "admin@eduonline.com",
            Email = "admin@eduonline.com",
            StatusId = Status.Cadastrado.Id,
            StatusNome = Status.Cadastrado.Nome
        };

        userManager
            .Setup(x => x.FindByIdAsync(id))
            .ReturnsAsync(user);

        var service = new AuthenticationService(
            signInManager.Object,
            userManager.Object,
            options,
            context,
            new Mock<IAspNetUser>().Object);

        var roleManager = CreateRoleManagerMock();
        var notificador = new InMemoryNotificador();
        var appUser = Mock.Of<IAspNetUser>(x => x.IsAuthenticated() == true);
        var logger = new Mock<ILogger<AuthController>>();

        var controller = new AuthController(notificador, service, appUser, roleManager.Object, logger.Object);

        var result = await controller.Excluir(Guid.Parse(id));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    private sealed class InMemoryNotificador : INotificador
    {
        private readonly List<Notificacao> _notificacoes = [];

        public bool TemNotificacao() => _notificacoes.Count != 0;

        public List<Notificacao> ObterNotificacoes() => _notificacoes;

        public void Handle(Notificacao notificacao) => _notificacoes.Add(notificacao);
    }

    private static AppTokenSettings CreateTokenSettings()
        => new()
        {
            Issuer = "https://localhost:7020",
            Audience = "EduOnline-Dev",
            Segredo = "360a1429-8cdb-45ec-ab2d-fccf2eb3521e",
            RefreshTokenExpiration = 8
        };

    private static ApplicationDbContext CreateContext()
    {
        var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(dbOptions);
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

    private static Mock<SignInManager<EduOnlineUser>> CreateSignInManagerMock(UserManager<EduOnlineUser> userManager)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<EduOnlineUser>>();
        var options = new Mock<IOptions<IdentityOptions>>();
        var logger = new Mock<ILogger<SignInManager<EduOnlineUser>>>();
        var schemes = new Mock<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
        var confirmation = new Mock<IUserConfirmation<EduOnlineUser>>();

        return new Mock<SignInManager<EduOnlineUser>>(
            userManager,
            contextAccessor.Object,
            claimsFactory.Object,
            options.Object,
            logger.Object,
            schemes.Object,
            confirmation.Object);
    }

    private static Mock<RoleManager<IdentityRole>> CreateRoleManagerMock()
    {
        var roleStore = new Mock<IRoleStore<IdentityRole>>();

        return new Mock<RoleManager<IdentityRole>>(
            roleStore.Object,
            Array.Empty<IRoleValidator<IdentityRole>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new Mock<ILogger<RoleManager<IdentityRole>>>().Object);
    }
}
