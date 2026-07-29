using EduOnline.Bff.ApiRest.Controllers;
using EduOnline.Bff.ApiRest.Requests;
using EduOnline.Bff.ApiRest.Services;
using EduOnline.Core.Api.Controllers;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.WebApps.ApiRest.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Text.Json;

namespace EduOnline.WebApps.UnitTest.Controllers;

public class AlunoControllerCoverageTest
{
    [Fact]
    public async Task AtualizarAluno_SemDataNascimento_DeveRetornarBadRequest()
    {
        var userId = Guid.NewGuid();
        var alunoService = new Mock<IAlunoService>();
        var controller = CriarController(alunoService.Object, new Mock<IConteudoService>().Object, new FakeAspNetUser(userId));

        var resultado = await controller.AtualizarAluno(userId, new AtualizarAlunoRequest { Nome = "Aluno" });

        resultado.Should().BeOfType<BadRequestObjectResult>();
        alunoService.Verify(s => s.AtualizarAluno(It.IsAny<Guid>(), It.IsAny<AtualizarAlunoRequest>()), Times.Never);
    }

    [Fact]
    public async Task MatricularAluno_SemCursoId_DeveRetornarBadRequest()
    {
        var userId = Guid.NewGuid();
        var controller = CriarController(new Mock<IAlunoService>().Object, new Mock<IConteudoService>().Object, new FakeAspNetUser(userId));

        var resultado = await controller.MatricularAluno(userId, new AdicionarMatriculaRequest());

        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task MatricularAluno_QuandoConteudoRetornaErroNaoEncontrado_DeveRetornarNotFound()
    {
        var userId = Guid.NewGuid();
        var conteudoService = new Mock<IConteudoService>();
        conteudoService.Setup(s => s.ObterCursoPorId(It.IsAny<Guid>())).ReturnsAsync(ResponseResultHelper.Erro("Curso não encontrado"));
        var controller = CriarController(new Mock<IAlunoService>().Object, conteudoService.Object, new FakeAspNetUser(userId));

        var resultado = await controller.MatricularAluno(userId, new AdicionarMatriculaRequest { CursoId = Guid.NewGuid() });

        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task MatricularAluno_QuandoDadosDoCursoNaoSaoJson_DeveRetornarBadRequest()
    {
        var userId = Guid.NewGuid();
        var conteudoService = new Mock<IConteudoService>();
        conteudoService.Setup(s => s.ObterCursoPorId(It.IsAny<Guid>())).ReturnsAsync(ResponseResultHelper.Ok(new { nome = "Curso" }));
        var controller = CriarController(new Mock<IAlunoService>().Object, conteudoService.Object, new FakeAspNetUser(userId));

        var resultado = await controller.MatricularAluno(userId, new AdicionarMatriculaRequest { CursoId = Guid.NewGuid() });

        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task MatricularAluno_QuandoJsonNaoTemCamposObrigatorios_DeveRetornarBadRequest()
    {
        var userId = Guid.NewGuid();
        var conteudoService = new Mock<IConteudoService>();
        conteudoService.Setup(s => s.ObterCursoPorId(It.IsAny<Guid>()))
            .ReturnsAsync(ResponseResultHelper.Ok(JsonSerializer.SerializeToElement(new { nome = "Curso sem valor" })));
        var controller = CriarController(new Mock<IAlunoService>().Object, conteudoService.Object, new FakeAspNetUser(userId));

        var resultado = await controller.MatricularAluno(userId, new AdicionarMatriculaRequest { CursoId = Guid.NewGuid() });

        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task MatricularAluno_Sucesso_DevePopularRequestEExecutarCreate()
    {
        var userId = Guid.NewGuid();
        var cursoId = Guid.NewGuid();
        AdicionarMatriculaRequest? requestCapturada = null;
        var alunoService = new Mock<IAlunoService>();
        alunoService.Setup(s => s.MatricularAluno(userId, It.IsAny<AdicionarMatriculaRequest>()))
            .Callback<Guid, AdicionarMatriculaRequest>((_, req) => requestCapturada = req)
            .ReturnsAsync(ResponseResultHelper.Ok());
        var conteudoService = new Mock<IConteudoService>();
        conteudoService.Setup(s => s.ObterCursoPorId(cursoId))
            .ReturnsAsync(ResponseResultHelper.Ok(JsonSerializer.SerializeToElement(new
            {
                nome = "Curso de Teste",
                valor = 199.90m,
                aulas = new[] { new { ordem = 1 }, new { ordem = 2 }, new { ordem = 3 } }
            })));
        var controller = CriarController(alunoService.Object, conteudoService.Object, new FakeAspNetUser(userId));

        var resultado = await controller.MatricularAluno(userId, new AdicionarMatriculaRequest { CursoId = cursoId });

        resultado.Should().BeOfType<CreatedResult>();
        requestCapturada.Should().NotBeNull();
        requestCapturada!.CursoNome.Should().Be("Curso de Teste");
        requestCapturada.Valor.Should().Be(199.90m);
        requestCapturada.TotalAulas.Should().Be(3);
    }

    [Fact]
    public async Task AtualizarProgressoCurso_QuandoMatriculaRetornaErroDeAutenticacao_DeveRetornarUnauthorized()
    {
        var userId = Guid.NewGuid();
        var matriculaId = Guid.NewGuid();
        var alunoService = new Mock<IAlunoService>();
        alunoService.Setup(s => s.ObterMatriculaPorId(userId, matriculaId)).ReturnsAsync(ResponseResultHelper.Erro("Não autenticado"));
        var controller = CriarController(alunoService.Object, new Mock<IConteudoService>().Object, new FakeAspNetUser(userId));

        var resultado = await controller.AtualizarProgressoCurso(userId, matriculaId, Guid.NewGuid());

        resultado.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task AtualizarProgressoCurso_QuandoMatriculaNaoTemCursoId_DeveRetornarBadRequest()
    {
        var userId = Guid.NewGuid();
        var matriculaId = Guid.NewGuid();
        var alunoService = new Mock<IAlunoService>();
        alunoService.Setup(s => s.ObterMatriculaPorId(userId, matriculaId))
            .ReturnsAsync(ResponseResultHelper.Ok(JsonSerializer.SerializeToElement(new { semCurso = true })));
        var controller = CriarController(alunoService.Object, new Mock<IConteudoService>().Object, new FakeAspNetUser(userId));

        var resultado = await controller.AtualizarProgressoCurso(userId, matriculaId, Guid.NewGuid());

        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AtualizarProgressoCurso_QuandoConteudoRetornaAcessoNegado_DeveRetornarForbidden()
    {
        var userId = Guid.NewGuid();
        var matriculaId = Guid.NewGuid();
        var cursoId = Guid.NewGuid();
        var alunoService = new Mock<IAlunoService>();
        alunoService.Setup(s => s.ObterMatriculaPorId(userId, matriculaId))
            .ReturnsAsync(ResponseResultHelper.Ok(JsonSerializer.SerializeToElement(new { cursoId })));
        var conteudoService = new Mock<IConteudoService>();
        conteudoService.Setup(s => s.RegistrarConsumoAula(cursoId, It.IsAny<Guid>(), userId, matriculaId))
            .ReturnsAsync(ResponseResultHelper.Erro("Acesso negado"));
        var controller = CriarController(alunoService.Object, conteudoService.Object, new FakeAspNetUser(userId));

        var resultado = await controller.AtualizarProgressoCurso(userId, matriculaId, Guid.NewGuid());

        resultado.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task FinalizarCurso_QuandoServicoRetornaErroGenerico_DeveRetornarBadRequest()
    {
        var userId = Guid.NewGuid();
        var matriculaId = Guid.NewGuid();
        var alunoService = new Mock<IAlunoService>();
        alunoService.Setup(s => s.FinalizarCurso(userId, matriculaId)).ReturnsAsync(ResponseResultHelper.Erro("Falha ao finalizar"));
        var controller = CriarController(alunoService.Object, new Mock<IConteudoService>().Object, new FakeAspNetUser(userId));

        var resultado = await controller.FinalizarCurso(userId, matriculaId);

        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    private static AlunoController CriarController(IAlunoService alunoService, IConteudoService conteudoService, IAspNetUser user)
    {
        var controller = new AlunoController(alunoService, conteudoService, user)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return controller;
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
