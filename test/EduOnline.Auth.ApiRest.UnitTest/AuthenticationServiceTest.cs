using EduOnline.Auth.ApiRest.Data;
using EduOnline.Auth.ApiRest.Extensions;
using EduOnline.Auth.ApiRest.Models;
using EduOnline.Core.ControleDeAcesso;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using AuthenticationService = EduOnline.Auth.ApiRest.Services.AuthenticationService;

namespace EduOnline.Auth.ApiRest.UnitTest;

public class AuthenticationServiceTest
{
    [Fact(DisplayName = "ObterUserId deve retornar usuário quando existir")]
    public async Task ObterUserId_QuandoUsuarioExiste_DeveRetornarUsuario()
    {
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);
        var context = CreateContext();
        var aspNetUser = new Mock<IAspNetUser>();
        var options = Options.Create(CreateTokenSettings());

        var userId = Guid.NewGuid().ToString();
        var expectedUser = new EduOnlineUser { Id = userId, Email = "aluno@eduonline.com", UserName = "aluno@eduonline.com" };

        userManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(expectedUser);

        var service = new AuthenticationService(signInManager.Object, userManager.Object, options, context, aspNetUser.Object);

        var result = await service.ObterUserId(Guid.Parse(userId));

        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
    }

    [Fact(DisplayName = "RemoverUser deve falhar para usuário não pendente")]
    public async Task RemoverUser_QuandoNaoPendente_DeveFalhar()
    {
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);
        var context = CreateContext();
        var aspNetUser = new Mock<IAspNetUser>();
        var options = Options.Create(CreateTokenSettings());

        var usuario = new EduOnlineUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@eduonline.com",
            UserName = "admin@eduonline.com",
            StatusId = Status.Cadastrado.Id,
            StatusNome = Status.Cadastrado.Nome
        };

        var service = new AuthenticationService(signInManager.Object, userManager.Object, options, context, aspNetUser.Object);

        var result = await service.RemoverUser(usuario);

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "UserNotPending");
        userManager.Verify(x => x.DeleteAsync(It.IsAny<EduOnlineUser>()), Times.Never);
    }

    [Fact(DisplayName = "RemoverUser deve remover usuário pendente")]
    public async Task RemoverUser_QuandoPendente_DeveRemover()
    {
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);
        var context = CreateContext();
        var aspNetUser = new Mock<IAspNetUser>();
        var options = Options.Create(CreateTokenSettings());

        var usuario = new EduOnlineUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "aluno@eduonline.com",
            UserName = "aluno@eduonline.com",
            StatusId = Status.Pendente.Id,
            StatusNome = Status.Pendente.Nome
        };

        userManager.Setup(x => x.DeleteAsync(usuario)).ReturnsAsync(IdentityResult.Success);

        var service = new AuthenticationService(signInManager.Object, userManager.Object, options, context, aspNetUser.Object);

        var result = await service.RemoverUser(usuario);

        result.Succeeded.Should().BeTrue();
        userManager.Verify(x => x.DeleteAsync(usuario), Times.Once);
    }

    [Fact(DisplayName = "GerarJwt deve lançar exceção quando usuário não existir")]
    public async Task GerarJwt_QuandoUsuarioNaoExiste_DeveLancarExcecao()
    {
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);
        var context = CreateContext();
        var aspNetUser = new Mock<IAspNetUser>();
        var options = Options.Create(CreateTokenSettings());

        userManager.Setup(x => x.FindByEmailAsync("inexistente@eduonline.com")).ReturnsAsync((EduOnlineUser?)null);

        var service = new AuthenticationService(signInManager.Object, userManager.Object, options, context, aspNetUser.Object);

        var action = async () => await service.GerarJwt("inexistente@eduonline.com");

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*não encontrado*");
    }

    [Fact(DisplayName = "ObterRefreshToken deve retornar token quando válido")]
    public async Task ObterRefreshToken_QuandoValido_DeveRetornarToken()
    {
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);
        var context = CreateContext();
        var aspNetUser = new Mock<IAspNetUser>();
        var options = Options.Create(CreateTokenSettings());

        var token = new RefreshToken
        {
            Username = "aluno@eduonline.com",
            ExpirationDate = DateTime.UtcNow.AddMinutes(30)
        };

        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync();

        var service = new AuthenticationService(signInManager.Object, userManager.Object, options, context, aspNetUser.Object);

        var result = await service.ObterRefreshToken(token.Token);

        result.Should().NotBeNull();
        result!.Token.Should().Be(token.Token);
    }

    [Fact(DisplayName = "ObterRefreshToken deve retornar null quando expirado")]
    public async Task ObterRefreshToken_QuandoExpirado_DeveRetornarNull()
    {
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);
        var context = CreateContext();
        var aspNetUser = new Mock<IAspNetUser>();
        var options = Options.Create(CreateTokenSettings());

        var token = new RefreshToken
        {
            Username = "aluno@eduonline.com",
            ExpirationDate = DateTime.UtcNow.AddMinutes(-30)
        };

        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync();

        var service = new AuthenticationService(signInManager.Object, userManager.Object, options, context, aspNetUser.Object);

        var result = await service.ObterRefreshToken(token.Token);

        result.Should().BeNull();
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
}
