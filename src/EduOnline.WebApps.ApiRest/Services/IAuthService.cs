using EduOnline.Bff.ApiRest.Requests;
using EduOnline.Core.Api.Controllers;

namespace EduOnline.Bff.ApiRest.Services;

public interface IAuthService
{
    Guid AggregateId { get; set; }
    Task<ResponseResult> CriarUsuarioIdentity(CriarUsuarioRequest request);
}

