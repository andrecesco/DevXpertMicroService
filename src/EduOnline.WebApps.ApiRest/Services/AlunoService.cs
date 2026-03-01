using EduOnline.Bff.ApiRest.Options;
using EduOnline.Bff.ApiRest.Requests;
using EduOnline.Core.Api.Controllers;
using Microsoft.Extensions.Options;

namespace EduOnline.Bff.ApiRest.Services;

public class AlunoService : BaseService, IAlunoService
{
    private readonly HttpClient _httpClient;

    public AlunoService(HttpClient httpClient, IOptions<ServiceUrlOptions> options)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(options.Value.AlunoUrl);
    }

    public async Task<ResponseResult> CriarAluno(Guid id, CriarAlunoRequest request)
    {
        var itemContent = ObterConteudo(request);
        var response = await _httpClient.PostAsync($"{id}", itemContent);

        return !TratarErrosResponse(response)
            ? await DeserializarObjetoResponse<ResponseResult>(response)
            : RetornoOk();
    }
}
