using EduOnline.Bff.ApiRest.Options;
using EduOnline.Core.Api.Controllers;
using EduOnline.WebApps.ApiRest.Models;
using Microsoft.Extensions.Options;

namespace EduOnline.Bff.ApiRest.Services;

public class PagamentoBffService : BaseService, IPagamentoBffService
{
    private readonly HttpClient _httpClient;

    public PagamentoBffService(HttpClient httpClient, IOptions<ServiceUrlOptions> options)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(options.Value.PagamentoUrl);
    }

    public async Task<ResponseResult> RealizarPagamento(RealizarPagamentoRequest request)
        => await ProcessarResposta(await _httpClient.PostAsync(string.Empty, ObterConteudo(request)), "Não foi possível realizar pagamento");

    public async Task<ResponseResult> ObterTodos()
        => await ProcessarResposta(await _httpClient.GetAsync(string.Empty), "Pagamentos não encontrados");

    public async Task<ResponseResult> ObterPorId(Guid id)
        => await ProcessarResposta(await _httpClient.GetAsync(id.ToString()), "Pagamento não encontrado");
}
