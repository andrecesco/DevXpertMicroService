#nullable enable

using EduOnline.Alunos.ApiRest.Controllers;
using EduOnline.Alunos.ApiRest.Models;
using EduOnline.Alunos.Application.Queries;
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

public class AlunoControllerErrorCoverageTest
{
    [Fact]
    public async Task ObterPorId_QuandoLancaDomainException_DeveRetornarBadRequest()
    {
        var userId = Guid.NewGuid();
        var query = new Mock<IAlunoQuery>();
        query.Setup(q => q.ObterPorId(userId)).ThrowsAsync(new DomainException("erro de domínio"));
        var controller = CriarController(alunoQuery: query.Object, user: new FakeAspNetUser(userId));

        var result = await controller.ObterPorId(userId);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ObterMatriculasPorAlunoId_QuandoLancaException_DeveRetornarBadRequest()
    {
        var userId = Guid.NewGuid();
        var query = new Mock<IAlunoQuery>();
        query.Setup(q => q.ObterMatriculasPorAlunoId(userId)).ThrowsAsync(new InvalidOperationException("falha"));
        var controller = CriarController(alunoQuery: query.Object, user: new FakeAspNetUser(userId));

        var result = await controller.ObterMatriculasPorAlunoId(userId);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ObterMatriculaPorId_QuandoLancaDomainException_DeveRetornarBadRequest()
    {
        var userId = Guid.NewGuid();
        var query = new Mock<IAlunoQuery>();
        query.Setup(q => q.ObterMatriculaPorId(It.IsAny<Guid>())).ThrowsAsync(new DomainException("erro de domínio"));
        var controller = CriarController(alunoQuery: query.Object, user: new FakeAspNetUser(userId));

        var result = await controller.ObterMatriculaPorId(userId, Guid.NewGuid());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ObterCertificadoPorMatriculaId_QuandoCertificadoNaoExiste_DeveRetornarBadRequest()
    {
        var userId = Guid.NewGuid();
        var query = new Mock<IAlunoQuery>();
        query.Setup(q => q.ObterCertificadoPorMatriculaId(It.IsAny<Guid>())).ReturnsAsync((Application.Queries.Dtos.CertificadoDto?)null);
        var controller = CriarController(alunoQuery: query.Object, user: new FakeAspNetUser(userId));

        var result = await controller.ObterCertificadoPorMatriculaId(userId, Guid.NewGuid());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AtualizarAluno_QuandoLancaException_DeveRetornarBadRequest()
    {
        var userId = Guid.NewGuid();
        var mediator = new Mock<IMediatorHandler>();
        mediator.Setup(m => m.EnviarComando(It.IsAny<Command>())).ThrowsAsync(new InvalidOperationException("falha"));
        var controller = CriarController(mediatorHandler: mediator.Object, user: new FakeAspNetUser(userId));

        var result = await controller.AtualizarAluno(userId, new AtualizarAlunoRequest { Nome = "Teste", DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-20)) });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task MatricularAluno_QuandoLancaDomainException_DeveRetornarBadRequest()
    {
        var userId = Guid.NewGuid();
        var mediator = new Mock<IMediatorHandler>();
        mediator.Setup(m => m.EnviarComando(It.IsAny<Command>())).ThrowsAsync(new DomainException("erro"));
        var controller = CriarController(mediatorHandler: mediator.Object, user: new FakeAspNetUser(userId));

        var result = await controller.MatricularAluno(userId, new AdicionarMatriculaRequest());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AtualizarProgressoCurso_QuandoLancaException_DeveRetornarBadRequest()
    {
        var userId = Guid.NewGuid();
        var mediator = new Mock<IMediatorHandler>();
        mediator.Setup(m => m.EnviarComando(It.IsAny<Command>())).ThrowsAsync(new InvalidOperationException("falha"));
        var controller = CriarController(mediatorHandler: mediator.Object, user: new FakeAspNetUser(userId));

        var result = await controller.AtualizarProgressoCurso(userId, Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task FinalizarCurso_QuandoLancaDomainException_DeveRetornarBadRequest()
    {
        var userId = Guid.NewGuid();
        var mediator = new Mock<IMediatorHandler>();
        mediator.Setup(m => m.EnviarComando(It.IsAny<Command>())).ThrowsAsync(new DomainException("erro"));
        var controller = CriarController(mediatorHandler: mediator.Object, user: new FakeAspNetUser(userId));

        var result = await controller.FinalizarCurso(userId, Guid.NewGuid());

        Assert.IsType<BadRequestObjectResult>(result);
    }

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
