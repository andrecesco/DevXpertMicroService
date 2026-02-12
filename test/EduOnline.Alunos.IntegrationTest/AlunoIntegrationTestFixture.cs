using Bogus;
using EduOnline.Alunos.Application.Queries.Dtos;
using EduOnline.Alunos.Domain.Enumeradores;
using EduOnline.Alunos.Domain.Models;
using EduOnline.Conteudos.Domain;
using EduOnline.IntegrationTest;
using EduOnline.WebApps.ApiRest.Models;
using System.Net.Http.Json;

namespace EduOnline.Alunos.IntegrationTest;

public class AlunoIntegrationTestFixture : WebApiIntegrationTestFixture<Program>
{
    public static string Uri => "api/alunos";

    public string UsuarioToken { get; private set; } = string.Empty;
    public Guid UsuarioId { get; private set; } = Guid.Empty;
    public Guid AlunoId { get; set; } = Guid.Empty;
    public Guid CursoId { get; set; } = Guid.Empty;
    public List<Aula> Aulas { get; set; } = [];
    public Guid MatriculaId { get; set; } = Guid.Empty;
    public int MatriculaStatusPagamentoId { get; private set; }
    public int MatriculaStatusId { get; private set; }
    public AtualizarAlunoRequest? AlterarAlunoRequest { get; private set; }
    public UsuarioRegistroModel? RegistroUsuarioRequest { get; private set; }

    public AlunoIntegrationTestFixture() 
    {
    }

    public static AtualizarAlunoRequest ObterAtualizarAlunoRequest()
    {
        return new Faker<AtualizarAlunoRequest>()
            .RuleFor(a => a.Nome, f => f.Person.FullName)
            .RuleFor(a => a.DataNascimento, f => f.Date.PastDateOnly());
    }

    public UsuarioRegistroModel ObterRequestRegistroUsuario()
    {
        RegistroUsuarioRequest = new Faker<UsuarioRegistroModel>()
            .RuleFor(u => u.Nome, f => f.Person.FullName)
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Senha, f => "Teste@123")
            .RuleFor(u => u.ConfirmaSenha, f => "Teste@123");

        return RegistroUsuarioRequest;
    }

    public async Task RealizarLoginApi()
    {
        var usuarioLogin = new UsuarioLoginModel
        {
            Email = RegistroUsuarioRequest?.Email ?? "aluno@aluno.com",
            Senha = RegistroUsuarioRequest?.Senha ?? "Teste@123"
        };

        var response = await Client.PostAsJsonAsync("api/auth/entrar", usuarioLogin);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        var usuarioRepostaModel = System.Text.Json.JsonSerializer.Deserialize<ResponseApi<UsuarioRepostaModel>>(json,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        UsuarioToken = usuarioRepostaModel?.Data?.AccessToken ?? string.Empty;
        UsuarioId = Guid.Parse(usuarioRepostaModel?.Data?.UsuarioToken?.Id ?? Guid.Empty.ToString());
    }

    public async Task RealizarLoginAdminApi()
    {
        var usuarioLogin = new UsuarioLoginModel
        {
            Email = "admin@admin.com",
            Senha = "Teste@123"
        };

        var response = await Client.PostAsJsonAsync("api/auth/entrar", usuarioLogin);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        var usuarioRepostaModel = System.Text.Json.JsonSerializer.Deserialize<ResponseApi<UsuarioRepostaModel>>(json,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        UsuarioToken = usuarioRepostaModel?.Data?.AccessToken ?? string.Empty;
        UsuarioId = Guid.Parse(usuarioRepostaModel?.Data?.UsuarioToken?.Id ?? Guid.Empty.ToString());
    }

    public async Task ObterCursoId()
    {
        var responseCurso = await Client.GetFromJsonAsync<ResponseApi<List<Curso>>>("api/cursos");
        var curso = responseCurso?.Data?.LastOrDefault();
        CursoId = curso?.Id ?? Guid.Empty;

        var responseAulas = await Client.GetFromJsonAsync<ResponseApi<List<Aula>>>($"api/cursos/{CursoId}/aulas");
        Aulas = responseAulas?.Data ?? [];
    }

    public async Task<MatriculaDto?> ObterMatricularPorId(Guid id)
    {
        Client.AtribuirToken(UsuarioToken);
        var response = await Client.GetFromJsonAsync<ResponseApi<MatriculaDto>>($"api/alunos/{UsuarioId}/matriculas/{id}");
        return response?.Data;
    }
}
