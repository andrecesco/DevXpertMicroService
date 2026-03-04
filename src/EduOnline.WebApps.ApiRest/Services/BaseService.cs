using EduOnline.Core.Api.Controllers;
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
        if (VerificarHttpBodyVazio(response)) return default;
        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(json, _options);
    }

    protected static bool TratarErrosResponse(HttpResponseMessage response)
    {
        if (VerificarHttpStatusInvalido(response)) return false;

        response.EnsureSuccessStatusCode();

        return true;
    }

    protected static ResponseResult RetornoOk()
    {
        return new ResponseResult(null, []);
    }

    private static bool VerificarHttpStatusInvalido(HttpResponseMessage response)
        => (response.StatusCode >= HttpStatusCode.BadRequest &&
            response.StatusCode < HttpStatusCode.InternalServerError)
        || VerificarHttpBodyVazio(response);

    private static bool VerificarHttpBodyVazio(HttpResponseMessage responseMessage)
    {
        IEnumerable<HttpStatusCode> respostasVazias = [HttpStatusCode.NoContent, HttpStatusCode.NotFound];
        return respostasVazias.Contains(responseMessage.StatusCode);
    }

    protected static Guid CapturarGuidInserido(HttpResponseMessage response)
    {
        if (response.StatusCode != HttpStatusCode.Created) return Guid.Empty;

        var temId = Guid.TryParse(response.Headers.Location.Segments.LastOrDefault(), out var id);

        if (!temId) throw new Exception("O id de cadastro não foi retornado");

        return id;
    }
}
