using EduOnline.Bff.ApiRest.Options;
using EduOnline.Bff.ApiRest.Requests;
using EduOnline.Core.Api.Controllers;
using EduOnline.WebApps.ApiRest.Models;
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
        => await ProcessarResposta(await _httpClient.PostAsync($"{id}", ObterConteudo(request)), "Aluno não encontrado");

    public async Task<ResponseResult> ObterTodos()
        => await ProcessarResposta(await _httpClient.GetAsync(string.Empty), "Alunos não encontrados");

    public async Task<ResponseResult> ObterPorId(Guid id)
        => await ProcessarResposta(await _httpClient.GetAsync(id.ToString()), "Aluno não encontrado");

    public async Task<ResponseResult> ObterMatriculasPorAlunoId(Guid id)
        => await ProcessarResposta(await _httpClient.GetAsync($"{id}/matriculas"), "Matrículas não encontradas");

    public async Task<ResponseResult> ObterMatriculaPorId(Guid id, Guid matriculaId)
        => await ProcessarResposta(await _httpClient.GetAsync($"{id}/matriculas/{matriculaId}"), "Matrícula não encontrada");

    public async Task<ResponseResult> ObterCertificadoPorMatriculaId(Guid id, Guid matriculaId)
        => await ProcessarResposta(await _httpClient.GetAsync($"{id}/matriculas/{matriculaId}/certificado"), "Certificado não encontrado");

    public async Task<ResponseResult> AtualizarAluno(Guid id, AtualizarAlunoRequest request)
        => await ProcessarResposta(await _httpClient.PatchAsync(id.ToString(), ObterConteudo(request)), "Aluno não encontrado");

    public async Task<ResponseResult> MatricularAluno(Guid id, AdicionarMatriculaRequest request)
        => await ProcessarResposta(await _httpClient.PostAsync($"{id}/matriculas", ObterConteudo(request)), "Não foi possível matricular o aluno");

    public async Task<ResponseResult> AtualizarProgressoCurso(Guid id, Guid matriculaId, Guid aulaId)
        => await ProcessarResposta(await _httpClient.PatchAsync($"{id}/matriculas/{matriculaId}/progresso/{aulaId}", null), "Não foi possível atualizar o progresso");

    public async Task<ResponseResult> FinalizarCurso(Guid id, Guid matriculaId)
        => await ProcessarResposta(await _httpClient.PatchAsync($"{id}/matriculas/{matriculaId}/finalizar", null), "Não foi possível finalizar o curso");
}
