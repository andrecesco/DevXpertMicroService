using EduOnline.Bff.ApiRest.Controllers;
using EduOnline.Bff.ApiRest.Services;
using EduOnline.Core.Api.Controllers;
using EduOnline.WebApps.ApiRest.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EduOnline.WebApps.UnitTest.Controllers;

public class PagamentoControllerTest
{
    private static PagamentoController CriarController(IPagamentoBffService pagamentoService)
    {
        var controller = new PagamentoController(pagamentoService);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [Fact(DisplayName = "RealizarPagamento deve retornar Ok quando pagamento processado com sucesso")]
    public async Task RealizarPagamento_Sucesso_DeveRetornarOk()
    {
        var service = new Mock<IPagamentoBffService>();
        service.Setup(s => s.RealizarPagamento(It.IsAny<RealizarPagamentoRequest>()))
            .ReturnsAsync(ResponseResultHelper.Ok());

        var result = await CriarController(service.Object).RealizarPagamento(new RealizarPagamentoRequest());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "RealizarPagamento deve retornar BadRequest quando pagamento rejeitado")]
    public async Task RealizarPagamento_Rejeitado_DeveRetornarBadRequest()
    {
        var service = new Mock<IPagamentoBffService>();
        service.Setup(s => s.RealizarPagamento(It.IsAny<RealizarPagamentoRequest>()))
            .ReturnsAsync(ResponseResultHelper.Erro("Pagamento recusado"));

        var result = await CriarController(service.Object).RealizarPagamento(new RealizarPagamentoRequest());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "ObterTodos deve retornar Ok com lista de pagamentos")]
    public async Task ObterTodos_DeveRetornarOk()
    {
        var service = new Mock<IPagamentoBffService>();
        service.Setup(s => s.ObterTodos()).ReturnsAsync(ResponseResultHelper.Ok());

        var result = await CriarController(service.Object).ObterTodos();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "ObterTodos deve retornar BadRequest quando serviço falha")]
    public async Task ObterTodos_ServicoErro_DeveRetornarBadRequest()
    {
        var service = new Mock<IPagamentoBffService>();
        service.Setup(s => s.ObterTodos()).ReturnsAsync(ResponseResultHelper.Erro("Falha no serviço"));

        var result = await CriarController(service.Object).ObterTodos();

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "ObterPorId deve retornar Ok quando pagamento encontrado")]
    public async Task ObterPorId_Encontrado_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var service = new Mock<IPagamentoBffService>();
        service.Setup(s => s.ObterPorId(id)).ReturnsAsync(ResponseResultHelper.Ok());

        var result = await CriarController(service.Object).ObterPorId(id);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "ObterPorId deve retornar NotFound quando pagamento não existe")]
    public async Task ObterPorId_NaoEncontrado_DeveRetornarNotFound()
    {
        var id = Guid.NewGuid();
        var service = new Mock<IPagamentoBffService>();
        service.Setup(s => s.ObterPorId(id))
            .ReturnsAsync(ResponseResultHelper.Erro("Pagamento não encontrado"));

        var result = await CriarController(service.Object).ObterPorId(id);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
