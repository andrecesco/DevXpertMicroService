using EduOnline.Bff.ApiRest.Options;
using EduOnline.Bff.ApiRest.Requests;
using EduOnline.Core.Api.Controllers;
using Microsoft.Extensions.Options;

namespace EduOnline.Bff.ApiRest.Services;

public class AuthService : BaseService, IAuthService
{
    private readonly HttpClient _httpClient;
    public Guid AggregateId { get; set; }

    public AuthService(HttpClient httpClient, IOptions<ServiceUrlOptions> options)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(options.Value.AuthUrl);
    }


    public async Task<ResponseResult> CriarUsuarioIdentity(CriarUsuarioRequest request)
    {
        var itemContent = ObterConteudo(request);
        var response = await _httpClient.PostAsync("nova/conta", itemContent);

        AggregateId = CapturarGuidInserido(response);

        return !TratarErrosResponse(response)
            ? await DeserializarObjetoResponse<ResponseResult>(response)
            : RetornoOk();
    }
}

