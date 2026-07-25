using EduOnline.Bff.ApiRest.Services;
using EduOnline.Bff.ApiRest.Controllers;
using EduOnline.Core.Api.Controllers;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Mensagens.Notifications;
using EduOnline.WebApps.ApiRest.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace EduOnline.WebApps.UnitTest.Controllers;

public class AlunoControllerTest
{
    private static AlunoController CriarController(
        IAlunoService alunoService,
        IConteudoService conteudoService,
        IAspNetUser user)
    {
        var controller = new AlunoController(alunoService, conteudoService, user);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [Fact(DisplayName = "ObterTodos deve retornar Ok com lista de alunos")]
    public async Task ObterTodos_DeveRetornarOk()
    {
        var alunoService = new Mock<IAlunoService>();
        alunoService.Setup(s => s.ObterTodos()).ReturnsAsync(ResponseResultHelper.Ok());
        var conteudoService = new Mock<IConteudoService>();
        var user = new FakeAspNetUser(Guid.NewGuid(), isAdmin: true);

        var result = await CriarController(alunoService.Object, conteudoService.Object, user).ObterTodos();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "ObterTodos deve retornar BadRequest quando serviço retorna erro")]
    public async Task ObterTodos_ServicoErro_DeveRetornarBadRequest()
    {
        var alunoService = new Mock<IAlunoService>();
        alunoService.Setup(s => s.ObterTodos()).ReturnsAsync(ResponseResultHelper.Erro("Falha"));
        var conteudoService = new Mock<IConteudoService>();
        var user = new FakeAspNetUser(Guid.NewGuid(), isAdmin: true);

        var result = await CriarController(alunoService.Object, conteudoService.Object, user).ObterTodos();

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "ObterPorId deve retornar Ok quando usuário consulta a si mesmo")]
    public async Task ObterPorId_ProprioAluno_DeveRetornarOk()
    {
        var userId = Guid.NewGuid();
        var alunoService = new Mock<IAlunoService>();
        alunoService.Setup(s => s.ObterPorId(userId)).ReturnsAsync(ResponseResultHelper.Ok());
        var conteudoService = new Mock<IConteudoService>();
        var user = new FakeAspNetUser(userId);

        var result = await CriarController(alunoService.Object, conteudoService.Object, user).ObterPorId(userId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "ObterPorId deve retornar Forbid quando usuário consulta outro aluno")]
    public async Task ObterPorId_OutroAluno_DeveRetornarForbid()
    {
        var alunoService = new Mock<IAlunoService>();
        var conteudoService = new Mock<IConteudoService>();
        var user = new FakeAspNetUser(Guid.NewGuid(), isAdmin: false);

        var result = await CriarController(alunoService.Object, conteudoService.Object, user).ObterPorId(Guid.NewGuid());

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact(DisplayName = "ObterPorId deve retornar Ok quando administrador consulta outro aluno")]
    public async Task ObterPorId_Admin_DeveRetornarOk()
    {
        var targetId = Guid.NewGuid();
        var alunoService = new Mock<IAlunoService>();
        alunoService.Setup(s => s.ObterPorId(targetId)).ReturnsAsync(ResponseResultHelper.Ok());
        var conteudoService = new Mock<IConteudoService>();
        var user = new FakeAspNetUser(Guid.NewGuid(), isAdmin: true);

        var result = await CriarController(alunoService.Object, conteudoService.Object, user).ObterPorId(targetId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "ObterPorId deve retornar NotFound quando aluno não encontrado")]
    public async Task ObterPorId_NaoEncontrado_DeveRetornarNotFound()
    {
        var userId = Guid.NewGuid();
        var alunoService = new Mock<IAlunoService>();
        alunoService.Setup(s => s.ObterPorId(userId))
            .ReturnsAsync(ResponseResultHelper.Erro("Aluno não encontrado"));
        var conteudoService = new Mock<IConteudoService>();
        var user = new FakeAspNetUser(userId);

        var result = await CriarController(alunoService.Object, conteudoService.Object, user).ObterPorId(userId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "ObterMatriculasPorAlunoId deve retornar Ok para o próprio aluno")]
    public async Task ObterMatriculasPorAlunoId_ProprioAluno_DeveRetornarOk()
    {
        var userId = Guid.NewGuid();
        var alunoService = new Mock<IAlunoService>();
        alunoService.Setup(s => s.ObterMatriculasPorAlunoId(userId)).ReturnsAsync(ResponseResultHelper.Ok());
        var conteudoService = new Mock<IConteudoService>();
        var user = new FakeAspNetUser(userId);

        var result = await CriarController(alunoService.Object, conteudoService.Object, user).ObterMatriculasPorAlunoId(userId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "ObterMatriculasPorAlunoId deve retornar Forbid para outro aluno")]
    public async Task ObterMatriculasPorAlunoId_OutroAluno_DeveRetornarForbid()
    {
        var alunoService = new Mock<IAlunoService>();
        var conteudoService = new Mock<IConteudoService>();
        var user = new FakeAspNetUser(Guid.NewGuid(), isAdmin: false);

        var result = await CriarController(alunoService.Object, conteudoService.Object, user).ObterMatriculasPorAlunoId(Guid.NewGuid());

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact(DisplayName = "ObterMatriculaPorId deve retornar Ok para o próprio aluno")]
    public async Task ObterMatriculaPorId_ProprioAluno_DeveRetornarOk()
    {
        var userId = Guid.NewGuid();
        var matriculaId = Guid.NewGuid();
        var alunoService = new Mock<IAlunoService>();
        alunoService.Setup(s => s.ObterMatriculaPorId(userId, matriculaId)).ReturnsAsync(ResponseResultHelper.Ok());
        var conteudoService = new Mock<IConteudoService>();
        var user = new FakeAspNetUser(userId);

        var result = await CriarController(alunoService.Object, conteudoService.Object, user).ObterMatriculaPorId(userId, matriculaId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "ObterMatriculaPorId deve retornar Forbid para outro aluno")]
    public async Task ObterMatriculaPorId_OutroAluno_DeveRetornarForbid()
    {
        var alunoService = new Mock<IAlunoService>();
        var conteudoService = new Mock<IConteudoService>();
        var user = new FakeAspNetUser(Guid.NewGuid(), isAdmin: false);

        var result = await CriarController(alunoService.Object, conteudoService.Object, user).ObterMatriculaPorId(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact(DisplayName = "ObterCertificadoPorMatriculaId deve retornar Forbid para outro aluno")]
    public async Task ObterCertificadoPorMatriculaId_OutroAluno_DeveRetornarForbid()
    {
        var alunoService = new Mock<IAlunoService>();
        var conteudoService = new Mock<IConteudoService>();
        var user = new FakeAspNetUser(Guid.NewGuid(), isAdmin: false);

        var result = await CriarController(alunoService.Object, conteudoService.Object, user).ObterCertificadoPorMatriculaId(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact(DisplayName = "ObterCertificadoPorMatriculaId deve retornar Ok para o próprio aluno")]
    public async Task ObterCertificadoPorMatriculaId_ProprioAluno_DeveRetornarOk()
    {
        var userId = Guid.NewGuid();
        var matriculaId = Guid.NewGuid();
        var alunoService = new Mock<IAlunoService>();
        alunoService.Setup(s => s.ObterCertificadoPorMatriculaId(userId, matriculaId)).ReturnsAsync(ResponseResultHelper.Ok());
        var conteudoService = new Mock<IConteudoService>();
        var user = new FakeAspNetUser(userId);

        var result = await CriarController(alunoService.Object, conteudoService.Object, user).ObterCertificadoPorMatriculaId(userId, matriculaId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "AtualizarAluno deve retornar Forbid para outro aluno")]
    public async Task AtualizarAluno_OutroAluno_DeveRetornarForbid()
    {
        var alunoService = new Mock<IAlunoService>();
        var conteudoService = new Mock<IConteudoService>();
        var user = new FakeAspNetUser(Guid.NewGuid(), isAdmin: false);

        var result = await CriarController(alunoService.Object, conteudoService.Object, user)
            .AtualizarAluno(Guid.NewGuid(), new AtualizarAlunoRequest());

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact(DisplayName = "AtualizarAluno deve retornar NoContent quando atualização bem-sucedida")]
    public async Task AtualizarAluno_Sucesso_DeveRetornarNoContent()
    {
        var userId = Guid.NewGuid();
        var alunoService = new Mock<IAlunoService>();
        alunoService.Setup(s => s.AtualizarAluno(userId, It.IsAny<AtualizarAlunoRequest>()))
            .ReturnsAsync(ResponseResultHelper.Ok());
        var conteudoService = new Mock<IConteudoService>();
        var user = new FakeAspNetUser(userId);

        var result = await CriarController(alunoService.Object, conteudoService.Object, user)
            .AtualizarAluno(userId, new AtualizarAlunoRequest());

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact(DisplayName = "AtualizarAluno deve retornar BadRequest quando serviço retorna erro")]
    public async Task AtualizarAluno_ServicoErro_DeveRetornarBadRequest()
    {
        var userId = Guid.NewGuid();
        var alunoService = new Mock<IAlunoService>();
        alunoService.Setup(s => s.AtualizarAluno(userId, It.IsAny<AtualizarAlunoRequest>()))
            .ReturnsAsync(ResponseResultHelper.Erro("Erro ao atualizar"));
        var conteudoService = new Mock<IConteudoService>();
        var user = new FakeAspNetUser(userId);

        var result = await CriarController(alunoService.Object, conteudoService.Object, user)
            .AtualizarAluno(userId, new AtualizarAlunoRequest());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "FinalizarCurso deve retornar Forbid para outro aluno")]
    public async Task FinalizarCurso_OutroAluno_DeveRetornarForbid()
    {
        var alunoService = new Mock<IAlunoService>();
        var conteudoService = new Mock<IConteudoService>();
        var user = new FakeAspNetUser(Guid.NewGuid(), isAdmin: false);

        var result = await CriarController(alunoService.Object, conteudoService.Object, user)
            .FinalizarCurso(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact(DisplayName = "FinalizarCurso deve retornar NoContent quando sucesso")]
    public async Task FinalizarCurso_Sucesso_DeveRetornarNoContent()
    {
        var userId = Guid.NewGuid();
        var matriculaId = Guid.NewGuid();
        var alunoService = new Mock<IAlunoService>();
        alunoService.Setup(s => s.FinalizarCurso(userId, matriculaId))
            .ReturnsAsync(ResponseResultHelper.Ok());
        var conteudoService = new Mock<IConteudoService>();
        var user = new FakeAspNetUser(userId);

        var result = await CriarController(alunoService.Object, conteudoService.Object, user)
            .FinalizarCurso(userId, matriculaId);

        result.Should().BeOfType<NoContentResult>();
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
