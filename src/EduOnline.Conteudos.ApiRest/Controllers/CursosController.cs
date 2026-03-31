using EduOnline.Conteudos.ApiRest.Models;
using EduOnline.Conteudos.Domain;
using EduOnline.Conteudos.Domain.Services;
using EduOnline.Core.Api.Controllers;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Mensagens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduOnline.Conteudos.ApiRest.Controllers;

/// <summary>
/// Endpoints para gestão de cursos, conteúdo programático e aulas.
/// </summary>
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[Authorize(Roles = "Aluno,Administrador")]
[Route("api/conteudos/cursos")]
public class CursosController(ICursoRepository cursoRepository, ICursoService cursoService, INotificador notificador, IAspNetUser user)
    : MainController(notificador, user)
{
    /// <summary>
    /// Lista todos os cursos disponíveis.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterTodos()
    {
        var cursos = await cursoRepository.ObterTodosAsync();
        return CustomResponse(cursos);
    }

    /// <summary>
    /// Obtém um curso específico pelo identificador.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var curso = await cursoRepository.ObterPorIdAsync(id);

        if (curso is null)
            return NotFound();

        return CustomResponse(curso);
    }

    /// <summary>
    /// Lista as aulas de um curso.
    /// </summary>
    [HttpGet("{id:guid}/aulas")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterAulasPorCursoId(Guid id)
    {
        var aulas = await cursoRepository.ObterAulasPorCursoIdAsync(id);
        return CustomResponse(aulas);
    }

    /// <summary>
    /// Cria um novo curso.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
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

        if (!OperacaoValida())
            return CustomResponse();

        return CreatedAtAction(nameof(ObterPorId), new { id = curso.Id }, null);
    }

    /// <summary>
    /// Atualiza os dados de um curso existente.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, CursoRequest request)
    {
        if (!ModelState.IsValid)
            return CustomResponse(ModelState);

        var cursoExistente = await cursoRepository.ObterPorIdAsync(id);
        if (cursoExistente is null)
            return NotFound();

        cursoExistente.Nome = request.Nome;
        cursoExistente.Autor = request.Autor;
        cursoExistente.Validade = request.Validade;
        cursoExistente.Valor = request.Valor;
        cursoExistente.ConteudoProgramatico = new ConteudoProgramatico
        {
            Tema = request.ConteudoProgramatico.Tema,
            NivelId = request.ConteudoProgramatico.NivelId,
            CargaHoraria = request.ConteudoProgramatico.CargaHoraria
        };

        await cursoService.Atualizar(cursoExistente);

        if (!OperacaoValida())
            return CustomResponse();

        return NoContent();
    }

    /// <summary>
    /// Inativa um curso sem removê-lo da base de dados.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpPatch("{id:guid}/inativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> InativarCurso(Guid id)
    {
        await cursoService.Inativar(id);

        if (!OperacaoValida())
            return CustomResponse();

        return NoContent();
    }

    /// <summary>
    /// Adiciona uma nova aula a um curso.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpPost("{id:guid}/aulas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
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

        if (!OperacaoValida())
            return CustomResponse();

        return CreatedAtAction(nameof(ObterAulasPorCursoId), new { id }, null);
    }

    /// <summary>
    /// Atualiza os dados de uma aula vinculada a um curso.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpPut("{id:guid}/aulas/{aulaId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        if (!OperacaoValida())
            return CustomResponse();

        return NoContent();
    }
}
