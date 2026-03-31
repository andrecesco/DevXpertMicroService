using EduOnline.Core.Api.Controllers;
using EduOnline.Core.Mensagens.Notifications;
using System.Net;
using System.Text;
using System.Text.Json;

namespace EduOnline.Bff.ApiRest.Services;

public abstract class BaseService
{
    private static readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };


    protected static StringContent ObterConteudo(object dado)
    {
        var json = JsonSerializer.Serialize(dado);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    protected static async Task<T> DeserializarObjetoResponse<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();

        if (json == string.Empty) throw new Exception("O response está com o body vazio!");

        return JsonSerializer.Deserialize<T>(json, _options);
    }

    protected static async Task<ResponseResult> ProcessarResposta(HttpResponseMessage response, string mensagemNotFound)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
            return RetornoErro(mensagemNotFound);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return RetornoErro("Não autenticado");

        if (response.StatusCode == HttpStatusCode.Forbidden)
            return RetornoErro("Acesso negado");

        if (VerificarHttpStatusInvalido(response))
            return await TentarDeserializarOuRetornarErroPadrao(response);

        if (RespostaSemCorpo(response))
            return RetornoOk();

        var json = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(json))
            return RetornoOk();

        return JsonSerializer.Deserialize<ResponseResult>(json, _options) ?? RetornoOk();
    }

    private static async Task<ResponseResult> TentarDeserializarOuRetornarErroPadrao(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();

        if (!string.IsNullOrWhiteSpace(json))
        {
            var resultado = JsonSerializer.Deserialize<ResponseResult>(json, _options);
            if (resultado is not null)
                return resultado;
        }

        return RetornoErro($"Falha na chamada do serviço ({(int)response.StatusCode})");
    }

    protected static ResponseResult RetornoOk(object data = null)
    {
        return new ResponseResult(data, []);
    }

    protected static ResponseResult RetornoErro(string mensagem)
    {
        return new ResponseResult(null, [new DomainNotification(string.Empty, mensagem)]);
    }

    protected static bool RespostaSemCorpo(HttpResponseMessage response)
        => response.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.Created;

    private static bool VerificarHttpStatusInvalido(HttpResponseMessage response)
        => (response.StatusCode >= HttpStatusCode.BadRequest &&
            response.StatusCode <= HttpStatusCode.InternalServerError);

    protected static Guid CapturarGuidInserido(HttpResponseMessage response)
    {
        if (response.StatusCode != HttpStatusCode.Created) return Guid.Empty;

        var temId = Guid.TryParse(response.Headers.Location.Segments.LastOrDefault(), out var id);

        if (!temId) throw new Exception("O id de cadastro não foi retornado");

        return id;
    }
}
