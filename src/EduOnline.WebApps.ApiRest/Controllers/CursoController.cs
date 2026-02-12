using EduOnline.Conteudos.Domain;
using EduOnline.Conteudos.Domain.Services;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Mensagens;
using EduOnline.WebApps.ApiRest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduOnline.WebApps.ApiRest.Controllers;

[Authorize(Roles = "Administrador")]
[Route("api/cursos")]
public class CursoController(ICursoRepository cursoRepository, ICursoService cursoService, INotificador notificador, IUser user) : MainController(notificador, user)
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ObterTodos()
    {
        var cursos = await cursoRepository.ObterTodosAsync();
        return CustomResponse(cursos);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var curso = await cursoRepository.ObterPorIdAsync(id);

        if (curso is null)
            return NotFound();

        return CustomResponse(curso);
    }

    [AllowAnonymous]
    [HttpGet("{id}/aulas")]
    public async Task<IActionResult> ObterAulasPorCursoId(Guid id)
    {
        var aulas = await cursoRepository.ObterAulasPorCursoIdAsync(id);
        return CustomResponse(aulas);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CursoRequest request)
    {
        if (!ModelState.IsValid)
            return CustomResponse(ModelState);

        var curso = new Curso
        {
            Nome = request.Nome,
            Autor = request.Autor,
            Validade = request.Validade,
            Valor = request.Valor,
            ConteudoProgramatico = new ConteudoProgramatico
            {
                Tema = request.ConteudoProgramatico.Tema,
                NivelId = request.ConteudoProgramatico.NivelId,
                CargaHoraria = request.ConteudoProgramatico.CargaHoraria
            }
        };

        await cursoService.Adicionar(curso);

        if (notificador.TemNotificacao())
            return CustomResponse(notificador.ObterNotificacoes());

        return Created();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, CursoRequest request)
    {
        if (!ModelState.IsValid)
            return CustomResponse(ModelState);

        var curso = new Curso
        {
            Id = id,
            Nome = request.Nome,
            Autor = request.Autor,
            Validade = request.Validade,
            Valor = request.Valor,
            ConteudoProgramatico = new ConteudoProgramatico
            {
                Tema = request.ConteudoProgramatico.Tema,
                NivelId = request.ConteudoProgramatico.NivelId,
                CargaHoraria = request.ConteudoProgramatico.CargaHoraria
            }
        };

        await cursoService.Atualizar(curso);

        if (notificador.TemNotificacao())
            return CustomResponse(notificador.ObterNotificacoes());

        return NoContent();
    }

    [HttpPatch("{id}/inativar")]
    public async Task<IActionResult> InativarCurso(Guid id)
    {
        await cursoService.Inativar(id);
        if (notificador.TemNotificacao())
            return CustomResponse(notificador.ObterNotificacoes());
        return NoContent();
    }

    [HttpPost("{id}/aulas")]
    public async Task<IActionResult> AdicionarAula(Guid id, AulaRequest request)
    {
        if (!ModelState.IsValid)
            return CustomResponse(ModelState);

        var aula = new Aula
        {
            CursoId = id,
            Titulo = request.Titulo,
            Descricao = request.Descricao,
            LinkMaterial = request.LinkMaterial,
            DuracaoEmMinutos = request.DuracaoEmMinutos
        };

        await cursoService.AdicionarAula(id, aula);

        if (notificador.TemNotificacao())
            return CustomResponse(notificador.ObterNotificacoes());

        return Created();
    }

    [HttpPut("{id}/aulas/{aulaId}")]
    public async Task<IActionResult> AtualizarAula(Guid id, Guid aulaId, AulaRequest request)
    {
        if (!ModelState.IsValid)
            return CustomResponse(ModelState);

        var aula = await cursoRepository.ObterAulaPorIdAsync(aulaId);

        if (aula is null)
            return NotFound();

        aula.Titulo = request.Titulo;
        aula.Descricao = request.Descricao;
        aula.LinkMaterial = request.LinkMaterial;
        aula.DuracaoEmMinutos = request.DuracaoEmMinutos;

        await cursoService.AtualizarAula(id, aula);

        if (notificador.TemNotificacao())
            return CustomResponse(notificador.ObterNotificacoes());

        return NoContent();
    }
}
