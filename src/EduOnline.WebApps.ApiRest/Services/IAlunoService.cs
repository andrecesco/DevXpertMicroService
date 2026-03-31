using EduOnline.Bff.ApiRest.Requests;
using EduOnline.Core.Api.Controllers;
using EduOnline.WebApps.ApiRest.Models;

namespace EduOnline.Bff.ApiRest.Services;

public interface IAlunoService
{
    Task<ResponseResult> CriarAluno(Guid id, CriarAlunoRequest request);
    Task<ResponseResult> ObterTodos();
    Task<ResponseResult> ObterPorId(Guid id);
    Task<ResponseResult> ObterMatriculasPorAlunoId(Guid id);
    Task<ResponseResult> ObterMatriculaPorId(Guid id, Guid matriculaId);
    Task<ResponseResult> ObterCertificadoPorMatriculaId(Guid id, Guid matriculaId);
    Task<ResponseResult> AtualizarAluno(Guid id, AtualizarAlunoRequest request);
    Task<ResponseResult> MatricularAluno(Guid id, AdicionarMatriculaRequest request);
    Task<ResponseResult> AtualizarProgressoCurso(Guid id, Guid matriculaId, Guid aulaId);
    Task<ResponseResult> FinalizarCurso(Guid id, Guid matriculaId);
}
