using Bogus;
using EduOnline.Alunos.ApiRest.Models;
using EduOnline.Alunos.Application.Queries.Dtos;
using EduOnline.Alunos.Domain.Enumeradores;
using EduOnline.Conteudos.Domain;
using EduOnline.IntegrationTest;
using Shouldly;
using System.Net.Http.Json;

namespace EduOnline.Alunos.IntegrationTest;

[TestCaseOrderer(typeof(PriorityOrderer))]
public class AlunoIntegrationTest(AlunoIntegrationTestFixture fixture) : IClassFixture<AlunoIntegrationTestFixture>
{
    [Fact(DisplayName = "001 - Criar usuário e adicionar aluno"), TestPriority(1)]
    [Trait("Categoria", "Integração API - Auth")]
    public async Task CriarUsuario()
    {
        var usuarioRegistro = fixture.ObterRequestRegistroUsuario();

        var response = await fixture.Client.PostAsJsonAsync("api/auth/nova-conta", usuarioRegistro, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();

        await fixture.RealizarLoginApi(usuarioRegistro.Email, usuarioRegistro.Senha);

        fixture.UsuarioToken.ShouldNotBeNullOrEmpty();
        fixture.UsuarioId.ShouldNotBe(Guid.Empty);
    }

    [Fact(DisplayName = "002 - Alterar aluno"), TestPriority(2)]
    [Trait("Categoria", "Integração API - Aluno")]
    public async Task AlterarAluno()
    {
        var request = AlunoIntegrationTestFixture.ObterAtualizarAlunoRequest();

        await fixture.RealizarLoginApi();
        fixture.Client.AtribuirToken(fixture.UsuarioToken);

        var response = await fixture.Client.PatchAsJsonAsync($"{AlunoIntegrationTestFixture.Uri}/{fixture.UsuarioId}", request, TestContext.Current.CancellationToken);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        json.ShouldBe(string.Empty);
    }

    [Fact(DisplayName = "003 - Obter Todos Alunos"), TestPriority(3)]
    [Trait("Categoria", "Integração API - Aluno")]
    public async Task ObterTodosAlunos()
    {
        await fixture.RealizarLoginAdminApi();
        fixture.Client.AtribuirToken(fixture.UsuarioToken);

        var response = await fixture.Client.GetFromJsonAsync<ResponseApi<List<AlunoDto>>>($"{AlunoIntegrationTestFixture.Uri}", TestContext.Current.CancellationToken);

        response?.Data?.Count.ShouldNotBe(0);
        fixture.AlunoId = response?.Data?.LastOrDefault()?.Id ?? Guid.Empty;
    }

    [Fact(DisplayName = "004 - Obter Aluno por Id"), TestPriority(4)]
    [Trait("Categoria", "Integração API - Aluno")]
    public async Task ObterAlunoPorId()
    {
        await fixture.RealizarLoginAdminApi();
        fixture.Client.AtribuirToken(fixture.UsuarioToken);

        var response = await fixture.Client.GetFromJsonAsync<ResponseApi<AlunoDto>>($"{AlunoIntegrationTestFixture.Uri}/{fixture.AlunoId}", TestContext.Current.CancellationToken);

        response?.Data?.ShouldNotBeNull();
    }

    [Fact(DisplayName = "005 - Matricular Aluno"), TestPriority(5)]
    [Trait("Categoria", "Integração API - Aluno")]
    public async Task MatricularAluno()
    {
        await fixture.RealizarLoginApi();
        fixture.Client.AtribuirToken(fixture.UsuarioToken);

        fixture.CursoId = Guid.NewGuid();
        fixture.Aulas = [
            new Aula { Id = Guid.NewGuid(), CursoId = fixture.CursoId },
            new Aula { Id = Guid.NewGuid(), CursoId = fixture.CursoId },
            new Aula { Id = Guid.NewGuid(), CursoId = fixture.CursoId }
        ];

        var adicionarMatriculaRequest = new Faker<AdicionarMatriculaRequest>()
            .RuleFor(a => a.CursoId, f => fixture.CursoId)
            .RuleFor(a => a.CursoNome, f => f.Company.CatchPhrase())
            .RuleFor(a => a.Valor, f => f.Random.Decimal(100, 5000))
            .RuleFor(a => a.TotalAulas, f => fixture.Aulas.Count)
            .RuleFor(a => a.NomeCartao, f => f.Person.FullName)
            .RuleFor(a => a.NumeroCartao, f => f.Finance.CreditCardNumber())
            .RuleFor(a => a.ExpiracaoCartao, f => f.Date.FutureDateOnly().ToString("MM") + "/" + f.Date.FutureDateOnly().ToString("yy"))
            .RuleFor(a => a.CvvCartao, f => f.Finance.CreditCardCvv())
            .Generate();

        var response = await fixture.Client.PostAsJsonAsync($"{AlunoIntegrationTestFixture.Uri}/{fixture.UsuarioId}/matriculas", adicionarMatriculaRequest, TestContext.Current.CancellationToken);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        fixture.CapturarGuidInserido(response);
        fixture.MatriculaId = fixture.Id;

        await fixture.AprovarPagamentoMatricula(fixture.MatriculaId);

        json.ShouldBe(string.Empty);

        var matricula = await fixture.ObterMatricularPorId(fixture.MatriculaId);

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
            var response = await fixture.Client.PatchAsync($"{AlunoIntegrationTestFixture.Uri}/{fixture.UsuarioId}/matriculas/{fixture.MatriculaId}/progresso/{aula.Id}", null, TestContext.Current.CancellationToken);

            var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

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

        fixture.CursoId = Guid.NewGuid();
        fixture.Aulas = [
            new Aula { Id = Guid.NewGuid(), CursoId = fixture.CursoId },
            new Aula { Id = Guid.NewGuid(), CursoId = fixture.CursoId }
        ];

        var adicionarMatriculaRequest = new Faker<AdicionarMatriculaRequest>()
            .RuleFor(a => a.CursoId, f => fixture.CursoId)
            .RuleFor(a => a.CursoNome, f => f.Company.CatchPhrase())
            .RuleFor(a => a.Valor, f => f.Random.Decimal(100, 5000))
            .RuleFor(a => a.TotalAulas, f => fixture.Aulas.Count)
            .RuleFor(a => a.NomeCartao, f => f.Person.FullName)
            .RuleFor(a => a.NumeroCartao, f => f.Finance.CreditCardNumber())
            .RuleFor(a => a.ExpiracaoCartao, f => f.Date.FutureDateOnly().ToString("MM") + "/" + f.Date.FutureDateOnly().ToString("yy"))
            .RuleFor(a => a.CvvCartao, f => f.Finance.CreditCardCvv())
            .Generate();

        var matriculaResponse = await fixture.Client.PostAsJsonAsync($"{AlunoIntegrationTestFixture.Uri}/{fixture.UsuarioId}/matriculas", adicionarMatriculaRequest, TestContext.Current.CancellationToken);
        matriculaResponse.EnsureSuccessStatusCode();
        fixture.CapturarGuidInserido(matriculaResponse);
        fixture.MatriculaId = fixture.Id;

        await fixture.AprovarPagamentoMatricula(fixture.MatriculaId);

        foreach (var aula in fixture.Aulas)
        {
            var progressoResponse = await fixture.Client.PatchAsync($"{AlunoIntegrationTestFixture.Uri}/{fixture.UsuarioId}/matriculas/{fixture.MatriculaId}/progresso/{aula.Id}", null, TestContext.Current.CancellationToken);
            progressoResponse.EnsureSuccessStatusCode();
        }

        var finalizarResponse = await fixture.Client.PatchAsync($"{AlunoIntegrationTestFixture.Uri}/{fixture.UsuarioId}/matriculas/{fixture.MatriculaId}/finalizar", null, TestContext.Current.CancellationToken);
        finalizarResponse.EnsureSuccessStatusCode();

        var response = await fixture.Client.GetFromJsonAsync<ResponseApi<CertificadoDto>>($"{AlunoIntegrationTestFixture.Uri}/{fixture.UsuarioId}/matriculas/{fixture.MatriculaId}/certificado", TestContext.Current.CancellationToken);

        response.ShouldNotBeNull();
        response?.Data.ShouldNotBeNull();
        response?.Data?.MatriculaId.ShouldBe(fixture.MatriculaId);
    }
}
