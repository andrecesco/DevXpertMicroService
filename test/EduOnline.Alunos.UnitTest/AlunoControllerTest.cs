#nullable enable

using EduOnline.Alunos.ApiRest.Controllers;
using EduOnline.Alunos.ApiRest.Models;
using EduOnline.Alunos.Application.Queries;
using EduOnline.Alunos.Application.Queries.Dtos;
using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.DomainObjects;
using EduOnline.Core.Mensagens;
using EduOnline.Core.Mensagens.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace EduOnline.Alunos.UnitTest;

public class AlunoControllerTest
{
    private static AlunoController CriarController(
        IMediatorHandler? mediatorHandler = null,
        IAlunoQuery? alunoQuery = null,
        IAspNetUser? user = null)
    {
        var notifications = new DomainNotificationHandler();
        var controller = new AlunoController(
            mediatorHandler ?? new Mock<IMediatorHandler>().Object,
            notifications,
            alunoQuery ?? new Mock<IAlunoQuery>().Object,
            user ?? new FakeAspNetUser(Guid.NewGuid(), isAdmin: true));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [Fact]
    public async Task ObterTodos_DeveRetornarOk()
    {
        var query = new Mock<IAlunoQuery>();
        query.Setup(q => q.ObterTodos()).ReturnsAsync([new AlunoDto { Id = Guid.NewGuid() }]);
        var controller = CriarController(alunoQuery: query.Object);

        var result = await controller.ObterTodos();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ObterPorId_AlunoEncontrado_DeveRetornarOk()
    {
        var userId = Guid.NewGuid();
        var query = new Mock<IAlunoQuery>();
        query.Setup(q => q.ObterPorId(userId)).ReturnsAsync(new AlunoDto { Id = userId });
        var controller = CriarController(alunoQuery: query.Object, user: new FakeAspNetUser(userId));

        var result = await controller.ObterPorId(userId);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ObterPorId_NaoEncontrado_DeveRetornarNotFound()
    {
        var userId = Guid.NewGuid();
        var query = new Mock<IAlunoQuery>();
        query.Setup(q => q.ObterPorId(userId)).ReturnsAsync((AlunoDto)null!);
        var controller = CriarController(alunoQuery: query.Object, user: new FakeAspNetUser(userId));

        var result = await controller.ObterPorId(userId);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ObterPorId_OutroUsuario_DeveRetornarUnauthorized()
    {
        var query = new Mock<IAlunoQuery>();
        var controller = CriarController(alunoQuery: query.Object, user: new FakeAspNetUser(Guid.NewGuid(), isAdmin: false));

        var result = await controller.ObterPorId(Guid.NewGuid());

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task ObterPorId_Admin_DeveRetornarOk()
    {
        var targetId = Guid.NewGuid();
        var query = new Mock<IAlunoQuery>();
        query.Setup(q => q.ObterPorId(targetId)).ReturnsAsync(new AlunoDto { Id = targetId });
        var controller = CriarController(alunoQuery: query.Object, user: new FakeAspNetUser(Guid.NewGuid(), isAdmin: true));

        var result = await controller.ObterPorId(targetId);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ObterMatriculasPorAlunoId_ProprioAluno_DeveRetornarOk()
    {
        var userId = Guid.NewGuid();
        var query = new Mock<IAlunoQuery>();
        query.Setup(q => q.ObterMatriculasPorAlunoId(userId)).ReturnsAsync([]);
        var controller = CriarController(alunoQuery: query.Object, user: new FakeAspNetUser(userId));

        var result = await controller.ObterMatriculasPorAlunoId(userId);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ObterMatriculasPorAlunoId_OutroAluno_DeveRetornarUnauthorized()
    {
        var query = new Mock<IAlunoQuery>();
        var controller = CriarController(alunoQuery: query.Object, user: new FakeAspNetUser(Guid.NewGuid(), isAdmin: false));

        var result = await controller.ObterMatriculasPorAlunoId(Guid.NewGuid());

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task ObterMatriculaPorId_ProprioAluno_DeveRetornarOk()
    {
        var userId = Guid.NewGuid();
        var query = new Mock<IAlunoQuery>();
        query.Setup(q => q.ObterMatriculaPorId(It.IsAny<Guid>())).ReturnsAsync(new MatriculaDto());
        var controller = CriarController(alunoQuery: query.Object, user: new FakeAspNetUser(userId));

        var result = await controller.ObterMatriculaPorId(userId, Guid.NewGuid());

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ObterMatriculaPorId_OutroAluno_DeveRetornarUnauthorized()
    {
        var query = new Mock<IAlunoQuery>();
        var controller = CriarController(alunoQuery: query.Object, user: new FakeAspNetUser(Guid.NewGuid(), isAdmin: false));

        var result = await controller.ObterMatriculaPorId(Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task ObterCertificadoPorMatriculaId_Encontrado_DeveRetornarOk()
    {
        var userId = Guid.NewGuid();
        var query = new Mock<IAlunoQuery>();
        query.Setup(q => q.ObterCertificadoPorMatriculaId(It.IsAny<Guid>())).ReturnsAsync(new CertificadoDto());
        var controller = CriarController(alunoQuery: query.Object, user: new FakeAspNetUser(userId));

        var result = await controller.ObterCertificadoPorMatriculaId(userId, Guid.NewGuid());

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ObterCertificadoPorMatriculaId_OutroAluno_DeveRetornarUnauthorized()
    {
        var query = new Mock<IAlunoQuery>();
        var controller = CriarController(alunoQuery: query.Object, user: new FakeAspNetUser(Guid.NewGuid(), isAdmin: false));

        var result = await controller.ObterCertificadoPorMatriculaId(Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Cadastrar_Sucesso_DeveRetornarNoContent()
    {
        var mediator = new Mock<IMediatorHandler>();
        mediator.Setup(m => m.EnviarComando(It.IsAny<Command>())).ReturnsAsync(true);
        var controller = CriarController(mediatorHandler: mediator.Object);

        var result = await controller.Cadastrar(Guid.NewGuid(),
            new AdicionarAlunoRequest { Nome = "Aluno Teste", Email = "aluno@teste.com" });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task AtualizarAluno_Sucesso_DeveRetornarNoContent()
    {
        var userId = Guid.NewGuid();
        var mediator = new Mock<IMediatorHandler>();
        mediator.Setup(m => m.EnviarComando(It.IsAny<Command>())).ReturnsAsync(true);
        var controller = CriarController(mediatorHandler: mediator.Object, user: new FakeAspNetUser(userId));

        var result = await controller.AtualizarAluno(userId,
            new AtualizarAlunoRequest { Nome = "Novo Nome", DataNascimento = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)) });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task AtualizarAluno_OutroAluno_DeveRetornarUnauthorized()
    {
        var controller = CriarController(user: new FakeAspNetUser(Guid.NewGuid(), isAdmin: false));

        var result = await controller.AtualizarAluno(Guid.NewGuid(), new AtualizarAlunoRequest());

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task MatricularAluno_OutroAluno_DeveRetornarForbid()
    {
        var controller = CriarController(user: new FakeAspNetUser(Guid.NewGuid(), isAdmin: false));

        var result = await controller.MatricularAluno(Guid.NewGuid(), new AdicionarMatriculaRequest());

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task MatricularAluno_Sucesso_DeveRetornarCreatedAtAction()
    {
        var userId = Guid.NewGuid();
        var mediator = new Mock<IMediatorHandler>();
        mediator.Setup(m => m.EnviarComando(It.IsAny<Command>())).ReturnsAsync(true);
        var controller = CriarController(mediatorHandler: mediator.Object, user: new FakeAspNetUser(userId));

        var result = await controller.MatricularAluno(userId, new AdicionarMatriculaRequest());

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task AtualizarProgressoCurso_OutroAluno_DeveRetornarUnauthorized()
    {
        var controller = CriarController(user: new FakeAspNetUser(Guid.NewGuid(), isAdmin: false));

        var result = await controller.AtualizarProgressoCurso(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task AtualizarProgressoCurso_Sucesso_DeveRetornarNoContent()
    {
        var userId = Guid.NewGuid();
        var mediator = new Mock<IMediatorHandler>();
        mediator.Setup(m => m.EnviarComando(It.IsAny<Command>())).ReturnsAsync(true);
        var controller = CriarController(mediatorHandler: mediator.Object, user: new FakeAspNetUser(userId));

        var result = await controller.AtualizarProgressoCurso(userId, Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task FinalizarCurso_OutroAluno_DeveRetornarUnauthorized()
    {
        var controller = CriarController(user: new FakeAspNetUser(Guid.NewGuid(), isAdmin: false));

        var result = await controller.FinalizarCurso(Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task FinalizarCurso_Sucesso_DeveRetornarNoContent()
    {
        var userId = Guid.NewGuid();
        var mediator = new Mock<IMediatorHandler>();
        mediator.Setup(m => m.EnviarComando(It.IsAny<Command>())).ReturnsAsync(true);
        var controller = CriarController(mediatorHandler: mediator.Object, user: new FakeAspNetUser(userId));

        var result = await controller.FinalizarCurso(userId, Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ObterTodos_QuandoLancaDomainException_DeveRetornarBadRequest()
    {
        var query = new Mock<IAlunoQuery>();
        query.Setup(q => q.ObterTodos()).ThrowsAsync(new DomainException("falha de domínio"));
        var controller = CriarController(alunoQuery: query.Object);

        var result = await controller.ObterTodos();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Cadastrar_QuandoMediatorRetornaFalse_DeveRetornarOk()
    {
        var mediator = new Mock<IMediatorHandler>();
        mediator.Setup(m => m.EnviarComando(It.IsAny<Command>())).ReturnsAsync(false);
        var controller = CriarController(mediatorHandler: mediator.Object);

        var result = await controller.Cadastrar(Guid.NewGuid(),
            new AdicionarAlunoRequest { Nome = "Aluno Teste", Email = "aluno@teste.com" });

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Cadastrar_QuandoLancaDomainException_DeveRetornarBadRequest()
    {
        var mediator = new Mock<IMediatorHandler>();
        mediator.Setup(m => m.EnviarComando(It.IsAny<Command>())).ThrowsAsync(new DomainException("erro de domínio"));
        var controller = CriarController(mediatorHandler: mediator.Object);

        var result = await controller.Cadastrar(Guid.NewGuid(),
            new AdicionarAlunoRequest { Nome = "Aluno Teste", Email = "aluno@teste.com" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AtualizarAluno_QuandoMediatorRetornaFalse_DeveRetornarOk()
    {
        var userId = Guid.NewGuid();
        var mediator = new Mock<IMediatorHandler>();
        mediator.Setup(m => m.EnviarComando(It.IsAny<Command>())).ReturnsAsync(false);
        var controller = CriarController(mediatorHandler: mediator.Object, user: new FakeAspNetUser(userId));

        var result = await controller.AtualizarAluno(userId,
            new AtualizarAlunoRequest { Nome = "Novo Nome", DataNascimento = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)) });

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task MatricularAluno_QuandoMediatorRetornaFalse_DeveRetornarOk()
    {
        var userId = Guid.NewGuid();
        var mediator = new Mock<IMediatorHandler>();
        mediator.Setup(m => m.EnviarComando(It.IsAny<Command>())).ReturnsAsync(false);
        var controller = CriarController(mediatorHandler: mediator.Object, user: new FakeAspNetUser(userId));

        var result = await controller.MatricularAluno(userId, new AdicionarMatriculaRequest());

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task AtualizarProgressoCurso_QuandoMediatorRetornaFalse_DeveRetornarOk()
    {
        var userId = Guid.NewGuid();
        var mediator = new Mock<IMediatorHandler>();
        mediator.Setup(m => m.EnviarComando(It.IsAny<Command>())).ReturnsAsync(false);
        var controller = CriarController(mediatorHandler: mediator.Object, user: new FakeAspNetUser(userId));

        var result = await controller.AtualizarProgressoCurso(userId, Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task FinalizarCurso_QuandoMediatorRetornaFalse_DeveRetornarOk()
    {
        var userId = Guid.NewGuid();
        var mediator = new Mock<IMediatorHandler>();
        mediator.Setup(m => m.EnviarComando(It.IsAny<Command>())).ReturnsAsync(false);
        var controller = CriarController(mediatorHandler: mediator.Object, user: new FakeAspNetUser(userId));

        var result = await controller.FinalizarCurso(userId, Guid.NewGuid());

        Assert.IsType<OkObjectResult>(result);
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
