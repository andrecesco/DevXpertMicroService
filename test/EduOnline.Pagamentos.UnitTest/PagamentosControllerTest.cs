using EduOnline.Core.Api.Controllers;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Mensagens.Notifications;
using EduOnline.Pagamentos.ApiRest.Controllers;
using EduOnline.Pagamentos.ApiRest.Models;
using EduOnline.Pagamentos.Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace EduOnline.Pagamentos.UnitTest;

public class PagamentosControllerTest
{
    [Fact(DisplayName = "RealizarPagamento deve retornar BadRequest quando ModelState inválido")]
    public async Task RealizarPagamento_ModelStateInvalido_DeveRetornarBadRequest()
    {
        var pagamentoService = new Mock<IPagamentoService>();
        var pagamentoRepository = new Mock<IPagamentoRepository>();
        var user = new FakeAspNetUser(Guid.NewGuid());
        var notifications = new DomainNotificationHandler();

        var controller = new PagamentosController(pagamentoService.Object, pagamentoRepository.Object, notifications, user);
        controller.ModelState.AddModelError("Total", "Total é obrigatório");

        var result = await controller.RealizarPagamento(new RealizarPagamentoRequest());

        result.Should().BeOfType<BadRequestObjectResult>();
        pagamentoService.Verify(x => x.RealizarPagamentoCurso(It.IsAny<EduOnline.Core.DomainObjects.Dtos.PagamentoCurso>()), Times.Never);
    }

    [Fact(DisplayName = "RealizarPagamento deve retornar Ok quando operação válida")]
    public async Task RealizarPagamento_OperacaoValida_DeveRetornarOk()
    {
        var pagamentoService = new Mock<IPagamentoService>();
        var pagamentoRepository = new Mock<IPagamentoRepository>();
        var alunoId = Guid.NewGuid();
        var user = new FakeAspNetUser(alunoId);
        var notifications = new DomainNotificationHandler();

        var transacao = new Transacao
        {
            Id = Guid.NewGuid(),
            Total = 199.90m,
            StatusTransacaoId = StatusTransacao.Aprovado.Id
        };

        EduOnline.Core.DomainObjects.Dtos.PagamentoCurso? pagamentoCursoCapturado = null;

        pagamentoService
            .Setup(x => x.RealizarPagamentoCurso(It.IsAny<EduOnline.Core.DomainObjects.Dtos.PagamentoCurso>()))
            .Callback<EduOnline.Core.DomainObjects.Dtos.PagamentoCurso>(p => pagamentoCursoCapturado = p)
            .ReturnsAsync(transacao);

        var controller = new PagamentosController(pagamentoService.Object, pagamentoRepository.Object, notifications, user);

        var request = new RealizarPagamentoRequest
        {
            MatriculaId = Guid.NewGuid(),
            CursoId = Guid.NewGuid(),
            Total = 199.90m,
            NomeCartao = "Aluno Teste",
            NumeroCartao = "4111111111111111",
            ExpiracaoCartao = "12/30",
            CvvCartao = "123"
        };

        var result = await controller.RealizarPagamento(request);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<ResponseResult>().Subject;
        payload.Success.Should().BeTrue();

        pagamentoCursoCapturado.Should().NotBeNull();
        pagamentoCursoCapturado!.AlunoId.Should().Be(alunoId);
        pagamentoCursoCapturado.CursoId.Should().Be(request.CursoId);
        pagamentoCursoCapturado.MatriculaId.Should().Be(request.MatriculaId);
    }

    [Fact(DisplayName = "ObterPorId deve retornar NotFound quando pagamento não encontrado")]
    public async Task ObterPorId_PagamentoNaoEncontrado_DeveRetornarNotFound()
    {
        var pagamentoService = new Mock<IPagamentoService>();
        var pagamentoRepository = new Mock<IPagamentoRepository>();
        var user = new FakeAspNetUser(Guid.NewGuid());
        var notifications = new DomainNotificationHandler();

        pagamentoRepository.Setup(x => x.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Pagamento?)null);

        var controller = new PagamentosController(pagamentoService.Object, pagamentoRepository.Object, notifications, user);

        var result = await controller.ObterPorId(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact(DisplayName = "ObterPorId deve retornar Forbid quando usuário não é dono e não é admin")]
    public async Task ObterPorId_UsuarioNaoDono_DeveRetornarForbid()
    {
        var pagamentoService = new Mock<IPagamentoService>();
        var pagamentoRepository = new Mock<IPagamentoRepository>();
        var user = new FakeAspNetUser(Guid.NewGuid(), isAdmin: false);
        var notifications = new DomainNotificationHandler();

        var pagamento = new Pagamento
        {
            Id = Guid.NewGuid(),
            AlunoId = Guid.NewGuid(),
            Total = 100m
        };

        pagamentoRepository.Setup(x => x.ObterPorIdAsync(pagamento.Id)).ReturnsAsync(pagamento);

        var controller = new PagamentosController(pagamentoService.Object, pagamentoRepository.Object, notifications, user);

        var result = await controller.ObterPorId(pagamento.Id);

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact(DisplayName = "ObterTodos deve retornar Ok com lista de pagamentos")]
    public async Task ObterTodos_DeveRetornarOk()
    {
        var pagamentoService = new Mock<IPagamentoService>();
        var pagamentoRepository = new Mock<IPagamentoRepository>();
        var user = new FakeAspNetUser(Guid.NewGuid(), isAdmin: true);
        var notifications = new DomainNotificationHandler();

        pagamentoRepository.Setup(x => x.ObterTodosAsync()).ReturnsAsync(new List<Pagamento>
        {
            new() { Id = Guid.NewGuid(), AlunoId = Guid.NewGuid(), Total = 50m },
            new() { Id = Guid.NewGuid(), AlunoId = Guid.NewGuid(), Total = 80m }
        });

        var controller = new PagamentosController(pagamentoService.Object, pagamentoRepository.Object, notifications, user);

        var result = await controller.ObterTodos();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<ResponseResult>().Subject;
        payload.Success.Should().BeTrue();
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
