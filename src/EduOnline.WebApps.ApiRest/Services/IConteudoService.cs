using EduOnline.Core.Api.Controllers;
using EduOnline.WebApps.ApiRest.Models;

namespace EduOnline.Bff.ApiRest.Services;

public interface IConteudoService
{
    Task<ResponseResult> ObterTodosCursos();
    Task<ResponseResult> ObterCursoPorId(Guid id);
    Task<ResponseResult> ObterAulasPorCursoId(Guid id);
    Task<ResponseResult> CriarCurso(CursoRequest request);
    Task<ResponseResult> AtualizarCurso(Guid id, CursoRequest request);
    Task<ResponseResult> InativarCurso(Guid id);
    Task<ResponseResult> AdicionarAula(Guid id, AulaRequest request);
    Task<ResponseResult> AtualizarAula(Guid id, Guid aulaId, AulaRequest request);
}
