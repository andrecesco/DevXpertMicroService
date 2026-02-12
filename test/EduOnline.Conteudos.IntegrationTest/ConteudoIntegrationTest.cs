using Bogus;
using EduOnline.Conteudos.Domain;
using EduOnline.IntegrationTest;
using EduOnline.WebApps.ApiRest.Models;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Shouldly;
using System.Net.Http.Json;

namespace EduOnline.Conteudos.IntegrationTest;

[TestCaseOrderer("EduOnline.IntegrationTest.PriorityOrderer", "EduOnline.IntegrationTest")]
public class ConteudoIntegrationTest(AlunoIntegrationTestFixture fixture) : IClassFixture<AlunoIntegrationTestFixture>
{
    [Fact(DisplayName = "001 - Criar curso"), TestPriority(1)]
    [Trait("Categoria", "Integração API - Curso")]
    public async Task CriarCurso()
    {
        await fixture.RealizarLoginAdminApi();
        fixture.Client.AtribuirToken(fixture.UsuarioToken);

        var cursoRequest = AlunoIntegrationTestFixture.ObterCursoRequest();

        var response = await fixture.Client.PostAsJsonAsync($"{AlunoIntegrationTestFixture.Uri}", cursoRequest);
        
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        json.ShouldBe(string.Empty);
    }

    [Fact(DisplayName = "002 - Alterar curso"), TestPriority(2)]
    [Trait("Categoria", "Integração API - Curso")]
    public async Task AlterarCurso()
    {
        await fixture.RealizarLoginAdminApi();
        fixture.Client.AtribuirToken(fixture.UsuarioToken);

        await fixture.ObterCursoId();

        var cursoRequest = AlunoIntegrationTestFixture.ObterCursoRequest();

        var response = await fixture.Client.PutAsJsonAsync($"{AlunoIntegrationTestFixture.Uri}/{fixture.CursoId}", cursoRequest);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        json.ShouldBe(string.Empty);
    }

    [Fact(DisplayName = "003 - Inativar curso"), TestPriority(3)]
    [Trait("Categoria", "Integração API - Curso")]
    public async Task InativarCurso()
    {
        await fixture.RealizarLoginAdminApi();
        fixture.Client.AtribuirToken(fixture.UsuarioToken);

        await fixture.ObterCursoId();

        var response = await fixture.Client.PatchAsync($"{AlunoIntegrationTestFixture.Uri}/{fixture.CursoId}/inativar", null);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        json.ShouldBe(string.Empty);
    }

    [Fact(DisplayName = "004 - Adicionar aula"), TestPriority(4)]
    [Trait("Categoria", "Integração API - Curso")]
    public async Task AdicionarAula()
    {
        await fixture.RealizarLoginAdminApi();
        fixture.Client.AtribuirToken(fixture.UsuarioToken);

        await fixture.ObterCursoId();

        var aulaRequest = AlunoIntegrationTestFixture.ObterAulaRequest();

        var response = await fixture.Client.PostAsJsonAsync($"{AlunoIntegrationTestFixture.Uri}/{fixture.CursoId}/aulas", aulaRequest);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        json.ShouldBe(string.Empty);
    }

    [Fact(DisplayName = "005 - Atualizar aula"), TestPriority(5)]
    [Trait("Categoria", "Integração API - Curso")]
    public async Task AtualizarAula()
    {
        await fixture.RealizarLoginAdminApi();
        fixture.Client.AtribuirToken(fixture.UsuarioToken);

        await fixture.ObterCursoId();
        await fixture.ObterAulaId();

        var aulaRequest = AlunoIntegrationTestFixture.ObterAulaRequest();

        var response = await fixture.Client.PutAsJsonAsync($"{AlunoIntegrationTestFixture.Uri}/{fixture.CursoId}/aulas/{fixture.AulaId}", aulaRequest);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        json.ShouldBe(string.Empty);
    }
}

public class AlunoIntegrationTestFixture : WebApiIntegrationTestFixture<Program>
{
    public static string Uri => "api/cursos";

    public string UsuarioToken { get; private set; } = string.Empty;
    public Guid UsuarioId { get; private set; } = Guid.Empty;
    public Guid CursoId { get; set; } = Guid.Empty;
    public Guid AulaId { get; set; } = Guid.Empty;

    public AlunoIntegrationTestFixture()
    {
    }

    public static CursoRequest ObterCursoRequest()
    {
        var conteudoProgramatico = new Faker<ConteudoProgramaticoRequest>("pt_BR")
            .RuleFor(c => c.Tema, f => f.Lorem.Word())
            .RuleFor(c => c.NivelId, f => f.Random.Int(1, 3))
            .RuleFor(c => c.CargaHoraria, f => f.Random.Int(10, 100))
            .Generate();

        return new Faker<CursoRequest>("pt_BR")
            .RuleFor(a => a.Nome, f => f.Lorem.Word())
            .RuleFor(a => a.Autor, f => f.Person.FullName)
            .RuleFor(a => a.Validade, f => f.Date.FutureDateOnly())
            .RuleFor(a => a.Valor, f => f.Finance.Amount(1, 1000, 2))
            .RuleFor(a => a.ConteudoProgramatico, f => conteudoProgramatico);
    }

    public static AulaRequest ObterAulaRequest()
    {
        return new Faker<AulaRequest>("pt_BR")
            .RuleFor(a => a.Titulo, f => f.Lorem.Sentence(3))
            .RuleFor(a => a.Descricao, f => f.Lorem.Sentence(3))
            .RuleFor(a => a.LinkMaterial, f => f.Internet.Url())
            .RuleFor(a => a.DuracaoEmMinutos, f => f.Random.Int(5, 60));
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
    }

    public async Task ObterAulaId()
    {
        var responseAulas = await Client.GetFromJsonAsync<ResponseApi<List<Aula>>>($"api/cursos/{CursoId}/aulas");
        var aula = responseAulas?.Data?.LastOrDefault();
        AulaId = aula?.Id ?? Guid.Empty;
    }
}
