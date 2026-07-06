using EduOnline.Bff.ApiRest.Options;
using EduOnline.Core.Api.Controllers;
using EduOnline.WebApps.ApiRest.Models;
using Microsoft.Extensions.Options;

namespace EduOnline.Bff.ApiRest.Services;

public class ConteudoService : BaseService, IConteudoService
{
    private readonly HttpClient _httpClient;

    public ConteudoService(HttpClient httpClient, IOptions<ServiceUrlOptions> options)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(options.Value.ConteudoUrl);
    }

    public async Task<ResponseResult> ObterTodosCursos()
        => await ProcessarResposta(await _httpClient.GetAsync("cursos"), "Cursos não encontrados");

    public async Task<ResponseResult> ObterCursoPorId(Guid id)
        => await ProcessarResposta(await _httpClient.GetAsync($"cursos/{id}"), "Curso não encontrado");

    public async Task<ResponseResult> ObterAulasPorCursoId(Guid id)
        => await ProcessarResposta(await _httpClient.GetAsync($"cursos/{id}/aulas"), "Curso não encontrado");

    public async Task<ResponseResult> CriarCurso(CursoRequest request)
        => await ProcessarResposta(await _httpClient.PostAsync("cursos", ObterConteudo(request)), "Curso não encontrado");

    public async Task<ResponseResult> AtualizarCurso(Guid id, CursoRequest request)
        => await ProcessarResposta(await _httpClient.PutAsync($"cursos/{id}", ObterConteudo(request)), "Curso não encontrado");

    public async Task<ResponseResult> InativarCurso(Guid id)
        => await ProcessarResposta(await _httpClient.PatchAsync($"cursos/{id}/inativar", null), "Curso não encontrado");

    public async Task<ResponseResult> AdicionarAula(Guid id, AulaRequest request)
        => await ProcessarResposta(await _httpClient.PostAsync($"cursos/{id}/aulas", ObterConteudo(request)), "Curso não encontrado");

    public async Task<ResponseResult> AtualizarAula(Guid id, Guid aulaId, AulaRequest request)
        => await ProcessarResposta(await _httpClient.PutAsync($"cursos/{id}/aulas/{aulaId}", ObterConteudo(request)), "Aula não encontrada");

    public async Task<ResponseResult> RegistrarConsumoAula(Guid cursoId, Guid aulaId, Guid alunoId, Guid matriculaId)
        => await ProcessarResposta(
            await _httpClient.PatchAsync($"cursos/{cursoId}/aulas/{aulaId}/consumo/alunos/{alunoId}/matriculas/{matriculaId}", null),
            "Não foi possível registrar o consumo da aula");
}
