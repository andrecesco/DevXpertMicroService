using EduOnline.Auth.ApiRest.Data;
using EduOnline.Auth.ApiRest.Extensions;
using EduOnline.Auth.ApiRest.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using AuthenticationService = EduOnline.Auth.ApiRest.Services.AuthenticationService;

namespace EduOnline.Auth.ApiRest.UnitTest;

public class AuthenticationServiceJwtGenerationTest
{
    [Fact(DisplayName = "GerarJwt deve lançar exceção quando Segredo estiver vazio")]
    public async Task GerarJwt_SegredoVazio_DeveLancarExcecao()
    {
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);
        var context = CreateContext();

        var email = "admin@eduonline.com";
        var user = new EduOnlineUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            Email = email
        };

        userManager.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
        userManager.Setup(x => x.GetClaimsAsync(user)).ReturnsAsync(new List<System.Security.Claims.Claim>());
        userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>());

        var tokenSettings = CreateTokenSettings();
        tokenSettings.Segredo = string.Empty;

        var options = Options.Create(tokenSettings);
        var service = new AuthenticationService(signInManager.Object, userManager.Object, options, context, new Mock<EduOnline.Core.ControleDeAcesso.IAspNetUser>().Object);

        var action = async () => await service.GerarJwt(email);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Segredo*");
    }

    [Fact(DisplayName = "GerarJwt deve gerar token com claims corretos")]
    public async Task GerarJwt_DeveGerarTokenComClaimsCorretos()
    {
        // Arrange
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);
        var context = CreateContext();

        var userId = Guid.NewGuid().ToString();
        var email = "admin@eduonline.com";
        var user = new EduOnlineUser
        {
            Id = userId,
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            StatusId = Status.Cadastrado.Id,
            StatusNome = Status.Cadastrado.Nome
        };

        userManager.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
        userManager.Setup(x => x.GetClaimsAsync(user)).ReturnsAsync(new List<System.Security.Claims.Claim>());
        userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Administrador" });

        var options = Options.Create(CreateTokenSettings());
        var service = new AuthenticationService(signInManager.Object, userManager.Object, options, context, new Mock<EduOnline.Core.ControleDeAcesso.IAspNetUser>().Object);

        // Act
        var resultado = await service.GerarJwt(email);

        // Assert
        resultado.Should().NotBeNull();
        resultado.AccessToken.Should().NotBeNullOrEmpty();
        resultado.ExpiraEm.Should().BeGreaterThan(0);
        resultado.UsuarioToken.Should().NotBeNull();
        resultado.UsuarioToken.Email.Should().Be(email);
        resultado.UsuarioToken.Id.Should().Be(userId);

        // Decodificar e validar token
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(resultado.AccessToken);
        token.Issuer.Should().Be("https://localhost:7020");
        token.Audiences.Should().Contain("EduOnline-Dev");
        token.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Administrador");
    }

    [Fact(DisplayName = "GerarJwt deve incluir RefreshToken na resposta")]
    public async Task GerarJwt_DeveIncluirRefreshTokenNaResposta()
    {
        // Arrange
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);
        var context = CreateContext();

        var email = "admin@eduonline.com";
        var user = new EduOnlineUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        userManager.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
        userManager.Setup(x => x.GetClaimsAsync(user)).ReturnsAsync(new List<System.Security.Claims.Claim>());
        userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>());

        var options = Options.Create(CreateTokenSettings());
        var service = new AuthenticationService(signInManager.Object, userManager.Object, options, context, new Mock<EduOnline.Core.ControleDeAcesso.IAspNetUser>().Object);

        // Act
        var resultado = await service.GerarJwt(email);

        // Assert
        resultado.RefreshToken.Should().NotBe(Guid.Empty);

        // Validar que refresh token foi persistido
        var refreshToken = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == resultado.RefreshToken);
        refreshToken.Should().NotBeNull();
        refreshToken!.Username.Should().Be(email);
        refreshToken.ExpirationDate.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact(DisplayName = "GerarJwt deve limpar RefreshTokens antigos do mesmo usuário")]
    public async Task GerarJwt_DeveLimparRefreshTokensAntigos()
    {
        // Arrange
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);
        var context = CreateContext();

        var email = "admin@eduonline.com";
        var user = new EduOnlineUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            Email = email
        };

        // Adicionar refresh token antigo
        var tokenAntigo = new RefreshToken
        {
            Username = email,
            ExpirationDate = DateTime.UtcNow.AddMinutes(30)
        };
        context.RefreshTokens.Add(tokenAntigo);
        await context.SaveChangesAsync();

        userManager.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
        userManager.Setup(x => x.GetClaimsAsync(user)).ReturnsAsync(new List<System.Security.Claims.Claim>());
        userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>());

        var options = Options.Create(CreateTokenSettings());
        var service = new AuthenticationService(signInManager.Object, userManager.Object, options, context, new Mock<EduOnline.Core.ControleDeAcesso.IAspNetUser>().Object);

        // Act
        await service.GerarJwt(email);

        // Assert
        var refreshTokensDoUsuario = context.RefreshTokens.Where(rt => rt.Username == email).ToList();
        refreshTokensDoUsuario.Should().HaveCount(1);
        refreshTokensDoUsuario[0].Token.Should().NotBe(tokenAntigo.Token);
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
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
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
