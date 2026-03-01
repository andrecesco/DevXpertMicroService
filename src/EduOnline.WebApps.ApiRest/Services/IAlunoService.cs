using EduOnline.Bff.ApiRest.Requests;
using EduOnline.Core.Api.Controllers;
using System.Net.Http;

namespace EduOnline.Bff.ApiRest.Services;

public interface IAlunoService
{
    Task<ResponseResult> CriarAluno(Guid id, CriarAlunoRequest request);
}
