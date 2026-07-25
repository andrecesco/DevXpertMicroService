using EduOnline.Bff.ApiRest.Services;
using EduOnline.Bff.ApiRest.Controllers;
using EduOnline.Core.Api.Controllers;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.WebApps.ApiRest.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace EduOnline.WebApps.UnitTest.Controllers;

public class CursoControllerTest
{
    private static CursoController CriarController(IConteudoService conteudoService)
    {
        var controller = new CursoController(conteudoService);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [Fact(DisplayName = "ObterTodos deve retornar Ok com lista de cursos")]
    public async Task ObterTodos_DeveRetornarOk()
    {
        var service = new Mock<IConteudoService>();
        service.Setup(s => s.ObterTodosCursos()).ReturnsAsync(ResponseResultHelper.Ok());

        var result = await CriarController(service.Object).ObterTodos();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "ObterTodos deve retornar BadRequest quando serviço falha")]
    public async Task ObterTodos_ServicoErro_DeveRetornarBadRequest()
    {
        var service = new Mock<IConteudoService>();
        service.Setup(s => s.ObterTodosCursos()).ReturnsAsync(ResponseResultHelper.Erro("Falha"));

        var result = await CriarController(service.Object).ObterTodos();

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "ObterPorId deve retornar Ok quando curso encontrado")]
    public async Task ObterPorId_CursoEncontrado_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var service = new Mock<IConteudoService>();
        service.Setup(s => s.ObterCursoPorId(id)).ReturnsAsync(ResponseResultHelper.Ok());

        var result = await CriarController(service.Object).ObterPorId(id);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "ObterPorId deve retornar NotFound quando curso não existe")]
    public async Task ObterPorId_NaoEncontrado_DeveRetornarNotFound()
    {
        var id = Guid.NewGuid();
        var service = new Mock<IConteudoService>();
        service.Setup(s => s.ObterCursoPorId(id))
            .ReturnsAsync(ResponseResultHelper.Erro("Curso não encontrado"));

        var result = await CriarController(service.Object).ObterPorId(id);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "ObterAulasPorCursoId deve retornar Ok com lista de aulas")]
    public async Task ObterAulas_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var service = new Mock<IConteudoService>();
        service.Setup(s => s.ObterAulasPorCursoId(id)).ReturnsAsync(ResponseResultHelper.Ok());

        var result = await CriarController(service.Object).ObterAulasPorCursoId(id);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "Criar deve retornar Created quando cadastro bem-sucedido")]
    public async Task CriarCurso_Sucesso_DeveRetornarCreated()
    {
        var service = new Mock<IConteudoService>();
        service.Setup(s => s.CriarCurso(It.IsAny<CursoRequest>())).ReturnsAsync(ResponseResultHelper.Ok());

        var result = await CriarController(service.Object).Criar(new CursoRequest());

        result.Should().BeOfType<CreatedResult>();
    }

    [Fact(DisplayName = "Criar deve retornar BadRequest quando serviço retorna erro")]
    public async Task CriarCurso_ServicoErro_DeveRetornarBadRequest()
    {
        var service = new Mock<IConteudoService>();
        service.Setup(s => s.CriarCurso(It.IsAny<CursoRequest>()))
            .ReturnsAsync(ResponseResultHelper.Erro("Erro ao criar curso"));

        var result = await CriarController(service.Object).Criar(new CursoRequest());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "Atualizar deve retornar NoContent quando atualização bem-sucedida")]
    public async Task AtualizarCurso_Sucesso_DeveRetornarNoContent()
    {
        var id = Guid.NewGuid();
        var service = new Mock<IConteudoService>();
        service.Setup(s => s.AtualizarCurso(id, It.IsAny<CursoRequest>()))
            .ReturnsAsync(ResponseResultHelper.Ok());

        var result = await CriarController(service.Object).Atualizar(id, new CursoRequest());

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact(DisplayName = "InativarCurso deve retornar NoContent quando inativação bem-sucedida")]
    public async Task InativarCurso_Sucesso_DeveRetornarNoContent()
    {
        var id = Guid.NewGuid();
        var service = new Mock<IConteudoService>();
        service.Setup(s => s.InativarCurso(id)).ReturnsAsync(ResponseResultHelper.Ok());

        var result = await CriarController(service.Object).InativarCurso(id);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact(DisplayName = "AdicionarAula deve retornar NoContent quando inclusão bem-sucedida")]
    public async Task AdicionarAula_Sucesso_DeveRetornarNoContent()
    {
        var id = Guid.NewGuid();
        var service = new Mock<IConteudoService>();
        service.Setup(s => s.AdicionarAula(id, It.IsAny<AulaRequest>()))
            .ReturnsAsync(ResponseResultHelper.Ok());

        var result = await CriarController(service.Object).AdicionarAula(id, new AulaRequest());

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact(DisplayName = "AtualizarAula deve retornar NoContent quando atualização bem-sucedida")]
    public async Task AtualizarAula_Sucesso_DeveRetornarNoContent()
    {
        var cursoId = Guid.NewGuid();
        var aulaId = Guid.NewGuid();
        var service = new Mock<IConteudoService>();
        service.Setup(s => s.AtualizarAula(cursoId, aulaId, It.IsAny<AulaRequest>()))
            .ReturnsAsync(ResponseResultHelper.Ok());

        var result = await CriarController(service.Object).AtualizarAula(cursoId, aulaId, new AulaRequest());

        result.Should().BeOfType<NoContentResult>();
    }
}
