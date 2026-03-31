using EduOnline.Bff.ApiRest.Requests;
using EduOnline.Core.Api.Controllers;
using EduOnline.WebApps.ApiRest.Models;

namespace EduOnline.Bff.ApiRest.Services;

public interface IAuthService
{
    Guid AggregateId { get; set; }
    Task<ResponseResult> CriarUsuarioIdentity(CriarUsuarioRequest request);
    Task<ResponseResult> RemoverUsuarioIdentity(Guid id);
    Task<ResponseResult> ObterUsuarioPorId(Guid id);
    Task<ResponseResult> Login(UsuarioLoginModel request);
    Task<ResponseResult> RefreshToken(string refreshToken);
    Task<ResponseResult> Logout();
}

