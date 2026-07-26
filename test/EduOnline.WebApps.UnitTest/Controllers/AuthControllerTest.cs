using EduOnline.Bff.ApiRest.Controllers;
using EduOnline.Bff.ApiRest.Requests;
using EduOnline.Bff.ApiRest.Services;
using EduOnline.Core.Api.Controllers;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Mensagens;
using EduOnline.WebApps.ApiRest.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace EduOnline.WebApps.UnitTest.Controllers;

public class AuthControllerTest
{
    private static AuthController CriarController(
        IAuthService authService,
        IAspNetUser user,
        INotificador? notificador = null)
    {
        notificador ??= new FakeNotificador();
        var controller = new AuthController(authService, notificador, user);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [Fact(DisplayName = "CriarUsuario deve retornar Created quando auth responde com sucesso")]
    public async Task CriarUsuario_AuthSucesso_DeveRetornarCreated()
    {
        var authService = new Mock<IAuthService>();
        authService
            .Setup(s => s.CriarUsuarioIdentity(It.IsAny<CriarUsuarioRequest>()))
            .ReturnsAsync(ResponseResultHelper.Ok());

        var user = new FakeAspNetUser(Guid.NewGuid());
        var controller = CriarController(authService.Object, user);

        var result = await controller.CriarUsuario(new CriarUsuarioRequest());

        result.Should().BeOfType<CreatedResult>();
    }

    [Fact(DisplayName = "CriarUsuario deve retornar BadRequest quando auth retorna erro")]
    public async Task CriarUsuario_AuthErro_DeveRetornarBadRequest()
    {
        var authService = new Mock<IAuthService>();
        authService
            .Setup(s => s.CriarUsuarioIdentity(It.IsAny<CriarUsuarioRequest>()))
            .ReturnsAsync(ResponseResultHelper.Erro("E-mail já cadastrado"));

        var user = new FakeAspNetUser(Guid.NewGuid());
        var controller = CriarController(authService.Object, user);

        var result = await controller.CriarUsuario(new CriarUsuarioRequest());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "Entrar deve retornar Ok quando login bem-sucedido")]
    public async Task Entrar_LoginValido_DeveRetornarOk()
    {
        var authService = new Mock<IAuthService>();
        authService
            .Setup(s => s.Login(It.IsAny<UsuarioLoginModel>()))
            .ReturnsAsync(ResponseResultHelper.Ok("token-jwt"));

        var user = new FakeAspNetUser(Guid.NewGuid());
        var controller = CriarController(authService.Object, user);

        var result = await controller.Entrar(new UsuarioLoginModel());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "Entrar deve retornar BadRequest quando credenciais inválidas")]
    public async Task Entrar_CredenciaisInvalidas_DeveRetornarBadRequest()
    {
        var authService = new Mock<IAuthService>();
        authService
            .Setup(s => s.Login(It.IsAny<UsuarioLoginModel>()))
            .ReturnsAsync(ResponseResultHelper.Erro("Usuário ou senha inválidos"));

        var user = new FakeAspNetUser(Guid.NewGuid());
        var controller = CriarController(authService.Object, user);

        var result = await controller.Entrar(new UsuarioLoginModel());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "RefreshToken deve retornar Ok quando token válido")]
    public async Task RefreshToken_TokenValido_DeveRetornarOk()
    {
        var authService = new Mock<IAuthService>();
        authService
            .Setup(s => s.RefreshToken(It.IsAny<string>()))
            .ReturnsAsync(ResponseResultHelper.Ok("novo-token"));

        var user = new FakeAspNetUser(Guid.NewGuid());
        var controller = CriarController(authService.Object, user);

        var result = await controller.RefreshToken("token-antigo");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "Sair deve retornar NoContent quando logout bem-sucedido")]
    public async Task Sair_Sucesso_DeveRetornarNoContent()
    {
        var authService = new Mock<IAuthService>();
        authService
            .Setup(s => s.Logout())
            .ReturnsAsync(ResponseResultHelper.Ok());

        var user = new FakeAspNetUser(Guid.NewGuid());
        var controller = CriarController(authService.Object, user);

        var result = await controller.Sair();

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact(DisplayName = "ObterPorId deve retornar Ok quando usuário é o próprio")]
    public async Task ObterPorId_ProprioUsuario_DeveRetornarOk()
    {
        var userId = Guid.NewGuid();
        var authService = new Mock<IAuthService>();
        authService
            .Setup(s => s.ObterUsuarioPorId(userId))
            .ReturnsAsync(ResponseResultHelper.Ok());

        var user = new FakeAspNetUser(userId);
        var controller = CriarController(authService.Object, user);

        var result = await controller.ObterPorId(userId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "ObterPorId deve retornar Forbid quando usuário não é o próprio nem admin")]
    public async Task ObterPorId_OutroUsuario_DeveRetornarForbid()
    {
        var authService = new Mock<IAuthService>();
        var user = new FakeAspNetUser(Guid.NewGuid(), isAdmin: false);
        var controller = CriarController(authService.Object, user);

        var result = await controller.ObterPorId(Guid.NewGuid());

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact(DisplayName = "ObterPorId deve retornar Ok quando administrador consulta outro usuário")]
    public async Task ObterPorId_Admin_DeveRetornarOk()
    {
        var targetId = Guid.NewGuid();
        var authService = new Mock<IAuthService>();
        authService
            .Setup(s => s.ObterUsuarioPorId(targetId))
            .ReturnsAsync(ResponseResultHelper.Ok());

        var user = new FakeAspNetUser(Guid.NewGuid(), isAdmin: true);
        var controller = CriarController(authService.Object, user);

        var result = await controller.ObterPorId(targetId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "Excluir deve retornar NoContent quando remoção bem-sucedida")]
    public async Task Excluir_Sucesso_DeveRetornarNoContent()
    {
        var authService = new Mock<IAuthService>();
        authService
            .Setup(s => s.RemoverUsuarioIdentity(It.IsAny<Guid>()))
            .ReturnsAsync(ResponseResultHelper.Ok());

        var user = new FakeAspNetUser(Guid.NewGuid(), isAdmin: true);
        var controller = CriarController(authService.Object, user);

        var result = await controller.Excluir(Guid.NewGuid());

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact(DisplayName = "Excluir deve retornar NotFound quando usuário não encontrado")]
    public async Task Excluir_NaoEncontrado_DeveRetornarNotFound()
    {
        var authService = new Mock<IAuthService>();
        authService
            .Setup(s => s.RemoverUsuarioIdentity(It.IsAny<Guid>()))
            .ReturnsAsync(ResponseResultHelper.Erro("Usuário não encontrado"));

        var user = new FakeAspNetUser(Guid.NewGuid(), isAdmin: true);
        var controller = CriarController(authService.Object, user);

        var result = await controller.Excluir(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    private sealed class FakeNotificador : INotificador
    {
        public void Handle(Notificacao notificacao) { }
        public List<Notificacao> ObterNotificacoes() => [];
        public bool TemNotificacao() => false;
    }

    private sealed class FakeAspNetUser(Guid userId, bool isAdmin = false) : IAspNetUser
    {
        public string Name => "user";
        public Guid GetUserId() => userId;
        public string GetUserEmail() => "user@eduonline.com";
        public bool IsAuthenticated() => true;
        public bool IsInRole(string role) => isAdmin && role == "Administrador";
        public IEnumerable<Claim> GetClaimsIdentity() => [];
        public HttpContext ObterHttpContext() => new DefaultHttpContext();
    }
}
