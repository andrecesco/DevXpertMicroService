using EduOnline.Bff.ApiRest.Requests;
using EduOnline.Bff.ApiRest.Services;
using EduOnline.Core.Api.Controllers;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Mensagens;
using Microsoft.AspNetCore.Mvc;

namespace EduOnline.Bff.ApiRest.Controllers;

public class AuthController(IAuthService authService, IAlunoService alunoService, INotificador notificador, IAspNetUser user) : MainController(notificador, user)
{
    [HttpPost]
    public async Task<IActionResult> CriarUsuario(CriarUsuarioRequest request)
    {
        var responseAuth = await authService.CriarUsuarioIdentity(request) ?? throw new Exception("Ocorreu um erro ao se comunicar com a api de autenticação");

        if (!responseAuth.Success) return BadRequest(responseAuth);

        var responseAluno = await alunoService.CriarAluno(Guid.NewGuid(), new CriarAlunoRequest { Email = request.Email, Nome = request.Nome }) ?? throw new Exception("Ocorreu um erro ao se comunicar com a api de Aluno");

        if (!responseAluno.Success) return BadRequest(responseAluno);


        return Created();
    }
}
