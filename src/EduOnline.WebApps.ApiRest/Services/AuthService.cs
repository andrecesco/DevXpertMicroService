using EduOnline.Bff.ApiRest.Options;
using EduOnline.Bff.ApiRest.Requests;
using EduOnline.Core.Api.Controllers;
using EduOnline.WebApps.ApiRest.Models;
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
        var response = await _httpClient.PostAsync("nova-conta", itemContent);

        AggregateId = CapturarGuidInserido(response);

        return await ProcessarResposta(response, "Não foi possível criar usuário");
    }

    public async Task<ResponseResult> RemoverUsuarioIdentity(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"{id}");
        return await ProcessarResposta(response, "Não foi possível remover usuário");
    }

    public async Task<ResponseResult> ObterUsuarioPorId(Guid id)
        => await ProcessarResposta(await _httpClient.GetAsync($"{id}"), "Usuário não encontrado");

    public async Task<ResponseResult> Login(UsuarioLoginModel request)
        => await ProcessarResposta(await _httpClient.PostAsync("entrar", ObterConteudo(request)), "Não foi possível autenticar o usuário");

    public async Task<ResponseResult> RefreshToken(string refreshToken)
        => await ProcessarResposta(await _httpClient.PostAsync("refresh-token", ObterConteudo(refreshToken)), "Não foi possível renovar o token");

    public async Task<ResponseResult> Logout()
        => await ProcessarResposta(await _httpClient.PostAsync("sair", null), "Não foi possível finalizar a sessão");
}

