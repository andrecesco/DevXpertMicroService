using EduOnline.Core.Api.Controllers;
using EduOnline.WebApps.ApiRest.Models;

namespace EduOnline.Bff.ApiRest.Services;

public interface IPagamentoBffService
{
    Task<ResponseResult> RealizarPagamento(RealizarPagamentoRequest request);
    Task<ResponseResult> ObterTodos();
    Task<ResponseResult> ObterPorId(Guid id);
}
