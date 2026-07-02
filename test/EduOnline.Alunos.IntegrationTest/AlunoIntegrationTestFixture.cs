extern alias alunosApi;

using Bogus;
using EduOnline.Alunos.ApiRest.Models;
using EduOnline.Alunos.Application.Commands;
using EduOnline.Alunos.Application.Queries.Dtos;
using EduOnline.Auth.ApiRest.Models;
using EduOnline.Conteudos.Domain;
using EduOnline.Core.Communication.Mediator;
using EduOnline.IntegrationTest;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace EduOnline.Alunos.IntegrationTest;

public class AlunoIntegrationTestFixture : IDisposable
{
    private readonly AlunosApiTestFactory _factory;
    private bool _disposedValue;

    public static string Uri => "api/alunos";
    public HttpClient Client { get; }
    public Guid Id { get; set; }

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
        var options = new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
            BaseAddress = new Uri("http://localhost"),
            HandleCookies = true,
            MaxAutomaticRedirections = 7
        };

        _factory = new AlunosApiTestFactory();
        Client = _factory.CreateClient(options);

        // Inicializar banco de dados de Auth
        _factory.InitializeAuthDatabaseAsync().GetAwaiter().GetResult();
    }

    public bool CapturarGuidInserido(HttpResponseMessage response)
    {
        var recursoFoiInserido = Guid.TryParse(response.Headers.Location?.Segments.LastOrDefault(), out var newId);
        Id = newId;

        return recursoFoiInserido;
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

    public async Task RealizarLoginApi(string? email = null, string? senha = null)
    {
        var usuarioLogin = new UsuarioLoginModel
        {
            Email = email ?? AlunosApiTestFactory.AlunoEmail,
            Senha = senha ?? AlunosApiTestFactory.Password
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
            Email = "admin@eduonline.com",
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

    public async Task<MatriculaDto?> ObterMatricularPorId(Guid id)
    {
        Client.AtribuirToken(UsuarioToken);
        var response = await Client.GetFromJsonAsync<ResponseApi<MatriculaDto>>($"api/alunos/{UsuarioId}/matriculas/{id}");
        return response?.Data;
    }

    public async Task AprovarPagamentoMatricula(Guid matriculaId)
    {
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediatorHandler>();
        await mediator.EnviarComando(new MatriculaPagaCommand(matriculaId));
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Client.Dispose();
                _factory.Dispose();
            }

            _disposedValue = true;
        }
    }
}
