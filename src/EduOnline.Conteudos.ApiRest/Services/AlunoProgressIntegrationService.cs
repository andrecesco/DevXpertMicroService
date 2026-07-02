using Microsoft.Extensions.Configuration;

namespace EduOnline.Conteudos.ApiRest.Services;

public class AlunoProgressIntegrationService(HttpClient httpClient, IConfiguration configuration) : IAlunoProgressIntegrationService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<bool> AtualizarProgressoAsync(Guid alunoId, Guid matriculaId, Guid aulaId, CancellationToken cancellationToken = default)
    {
        _httpClient.BaseAddress ??= new Uri(configuration["AlunoUrl"] ?? throw new InvalidOperationException("Configuração 'AlunoUrl' não encontrada."));

        var response = await _httpClient.PatchAsync($"{alunoId}/matriculas/{matriculaId}/progresso/{aulaId}", null, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
