using System.Net.Http.Headers;

namespace EduOnline.Auth.ApiRest.Services;

public class AlunoProvisioningService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<bool> CadastrarAlunoAsync(Guid id, string nome, string email, string accessToken, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, id.ToString())
        {
            Content = JsonContent.Create(new { nome, email })
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
