using Bogus;
using EduOnline.Alunos.Application.Queries.Dtos;
using EduOnline.Alunos.Domain.Enumeradores;
using EduOnline.Alunos.Domain.Models;
using EduOnline.IntegrationTest;
using EduOnline.WebApps.ApiRest.Models;
using Shouldly;
using System.Net.Http.Json;

namespace EduOnline.Alunos.IntegrationTest;

[TestCaseOrderer("EduOnline.IntegrationTest.PriorityOrderer", "EduOnline.IntegrationTest")]
public class AlunoIntegrationTest(AlunoIntegrationTestFixture fixture) : IClassFixture<AlunoIntegrationTestFixture>
{
    [Fact(DisplayName = "001 - Criar usuário e adicionar aluno"), TestPriority(1)]
    [Trait("Categoria", "Integração API - Auth")]
    public async Task CriarUsuario()
    {
        var usuarioRegistro = fixture.ObterRequestRegistroUsuario();

        var response = await fixture.Client.PostAsJsonAsync("api/auth/nova/conta", usuarioRegistro);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        var usuarioRepostaModel = System.Text.Json.JsonSerializer.Deserialize<ResponseApi<UsuarioRepostaModel>>(json,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        var token = usuarioRepostaModel?.Data?.AccessToken ?? string.Empty;
        var userId = Guid.Parse(usuarioRepostaModel?.Data?.UsuarioToken?.Id ?? Guid.Empty.ToString());

        token.ShouldNotBeNullOrEmpty();
        userId.ShouldNotBe(Guid.Empty);
    }

    [Fact(DisplayName = "002 - Alterar aluno"), TestPriority(2)]
    [Trait("Categoria", "Integração API - Aluno")]
    public async Task AlterarAluno()
    {
        var request = AlunoIntegrationTestFixture.ObterAtualizarAlunoRequest();

        await fixture.RealizarLoginApi();
        fixture.Client.AtribuirToken(fixture.UsuarioToken);

        var response = await fixture.Client.PatchAsJsonAsync($"{AlunoIntegrationTestFixture.Uri}/{fixture.UsuarioId}", request);
        var json = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        json.ShouldBe(string.Empty);
    }

    [Fact(DisplayName = "003 - Obter Todos Alunos"), TestPriority(3)]
    [Trait("Categoria", "Integração API - Aluno")]
    public async Task ObterTodosAlunos()
    {
        await fixture.RealizarLoginAdminApi();
        fixture.Client.AtribuirToken(fixture.UsuarioToken);

        var response = await fixture.Client.GetFromJsonAsync<ResponseApi<List<AlunoDto>>>($"{AlunoIntegrationTestFixture.Uri}");

        response?.Data?.Count.ShouldNotBe(0);
        fixture.AlunoId = response?.Data?.LastOrDefault()?.Id ?? Guid.Empty;
    }

    [Fact(DisplayName = "004 - Obter Aluno por Id"), TestPriority(4)]
    [Trait("Categoria", "Integração API - Aluno")]
    public async Task ObterAlunoPorId()
    {
        await fixture.RealizarLoginAdminApi();
        fixture.Client.AtribuirToken(fixture.UsuarioToken);

        var response = await fixture.Client.GetFromJsonAsync<ResponseApi<AlunoDto>>($"{AlunoIntegrationTestFixture.Uri}/{fixture.AlunoId}");

        response?.Data?.ShouldNotBeNull();
    }

    [Fact(DisplayName = "005 - Matricular Aluno"), TestPriority(5)]
    [Trait("Categoria", "Integração API - Aluno")]
    public async Task MatricularAluno()
    {
        await fixture.RealizarLoginApi();
        fixture.Client.AtribuirToken(fixture.UsuarioToken);

        await fixture.ObterCursoId();

        var adicionarMatriculaRequest = new Faker<AdicionarMatriculaRequest>()
            .RuleFor(a => a.CursoId, f => fixture.CursoId)
            .RuleFor(a => a.NomeCartao, f => f.Person.FullName)
            .RuleFor(a => a.NumeroCartao, f => f.Finance.CreditCardNumber())
            .RuleFor(a => a.ExpiracaoCartao, f => f.Date.FutureDateOnly().ToString("MM") + "/" + f.Date.FutureDateOnly().ToString("yy"))
            .RuleFor(a => a.CvvCartao, f => f.Finance.CreditCardCvv())
            .Generate();

        var contador = 0;

        var matricula = new MatriculaDto();

        do
        {
            var response = await fixture.Client.PostAsJsonAsync($"{AlunoIntegrationTestFixture.Uri}/{fixture.UsuarioId}/matriculas", adicionarMatriculaRequest);
            var json = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            fixture.CapturarGuidInserido(response);

            json.ShouldBe(string.Empty);

            matricula = await fixture.ObterMatricularPorId(fixture.Id);

            if (matricula is null)
                break;

            fixture.MatriculaId = fixture.Id;

            contador++;

            if (contador > 3)
                break;

        } while (matricula.PagamentoStatusId != PagamentoStatus.Pago.Id);

        matricula.ShouldNotBeNull();
        matricula.PagamentoStatusId.ShouldBe(PagamentoStatus.Pago.Id);
    }

    [Fact(DisplayName = "006 - Avançar progresso da matrícula"), TestPriority(6)]
    [Trait("Categoria", "Integração API - Aluno")]
    public async Task AdicionarProgressoDaMatricula()
    {
        await fixture.RealizarLoginApi();
        fixture.Client.AtribuirToken(fixture.UsuarioToken);

        foreach (var aula in fixture.Aulas)
        {
            var response = await fixture.Client.PatchAsync($"{AlunoIntegrationTestFixture.Uri}/{fixture.UsuarioId}/matriculas/{fixture.MatriculaId}/progresso/{aula.Id}", null);
            
            var json = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            json.ShouldBe(string.Empty);
        }
    }

    [Fact(DisplayName = "007 - Obter o Certificado"), TestPriority(7)]
    [Trait("Categoria", "Integração API - Aluno")]
    public async Task ObterCertificadoDaMatricula()
    {
        await fixture.RealizarLoginApi();
        fixture.Client.AtribuirToken(fixture.UsuarioToken);

        var response = await fixture.Client.GetFromJsonAsync<MatriculaDto>($"{AlunoIntegrationTestFixture.Uri}/{fixture.UsuarioId}/matriculas/{fixture.MatriculaId}/certificado");

        response.ShouldNotBeNull();
    }
}
