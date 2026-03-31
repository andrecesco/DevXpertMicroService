using EduOnline.Bff.ApiRest.Services;
using EduOnline.Core.Api.Controllers;
using EduOnline.WebApps.ApiRest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduOnline.Bff.ApiRest.Controllers;

/// <summary>
/// Endpoints de pagamentos expostos pela BFF.
/// </summary>
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[Authorize(Roles = "Aluno,Administrador")]
[Route("api/bff/pagamentos")]
public class PagamentoController(IPagamentoBffService pagamentoService) : ControllerBase
{
    /// <summary>
    /// Realiza pagamento para uma matrícula.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> RealizarPagamento(RealizarPagamentoRequest request)
    {
        var response = await pagamentoService.RealizarPagamento(request);
        return response.Success ? Ok(response) : RespostaErro(response);
    }

    /// <summary>
    /// Lista todos os pagamentos (somente administrador).
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpGet]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterTodos()
    {
        var response = await pagamentoService.ObterTodos();
        return response.Success ? Ok(response) : RespostaErro(response);
    }

    /// <summary>
    /// Obtém um pagamento pelo identificador.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var response = await pagamentoService.ObterPorId(id);
        return response.Success ? Ok(response) : RespostaErro(response);
    }

    private IActionResult RespostaErro(ResponseResult response)
    {
        var mensagens = response.Errors?.Select(e => e.Value).Where(m => !string.IsNullOrWhiteSpace(m)).ToList() ?? [];

        if (mensagens.Any(m => m.Contains("não encontrado", StringComparison.OrdinalIgnoreCase)))
            return NotFound(response);

        if (mensagens.Any(m => m.Contains("não autenticado", StringComparison.OrdinalIgnoreCase)))
            return Unauthorized(response);

        if (mensagens.Any(m => m.Contains("acesso negado", StringComparison.OrdinalIgnoreCase)))
            return StatusCode(StatusCodes.Status403Forbidden, response);

        return BadRequest(response);
    }
}
