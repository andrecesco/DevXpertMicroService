using EduOnline.Pagamentos.ApiRest.Models;
using EduOnline.Pagamentos.Domain;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EduOnline.Pagamentos.IntegrationTest;

[Collection(PagamentosIntegrationCollection.Name)]
public class PagamentosControllerIntegrationTest
{
    private readonly HttpClient _client;
    private readonly PagamentosApiTestFactory _factory;

    public PagamentosControllerIntegrationTest(PagamentosApiTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "GET /api/pagamentos deve retornar 401 sem autenticação")]
    public async Task ObterTodos_SemAutenticacao_DeveRetornar401()
    {
        var response = await _client.GetAsync("/api/pagamentos", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "GET /api/pagamentos deve retornar 403 com token de Aluno")]
    public async Task ObterTodos_ComTokenAluno_DeveRetornar403()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.GerarTokenAluno(_factory.Aluno1Id));

        var response = await _client.GetAsync("/api/pagamentos", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "GET /api/pagamentos deve retornar 200 com token de Administrador")]
    public async Task ObterTodos_ComTokenAdministrador_DeveRetornar200()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.GerarTokenAdmin(Guid.NewGuid()));

        var response = await _client.GetAsync("/api/pagamentos", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await ParseResponseAsync(response);
        envelope.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact(DisplayName = "GET /api/pagamentos/{id} deve retornar 200 para dono do pagamento")]
    public async Task ObterPorId_DonoPagamento_DeveRetornar200()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.GerarTokenAluno(_factory.Aluno1Id));

        var response = await _client.GetAsync($"/api/pagamentos/{_factory.PagamentoAluno1Id}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "GET /api/pagamentos/{id} deve retornar 403 para aluno que não é dono")]
    public async Task ObterPorId_AlunoNaoDono_DeveRetornar403()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.GerarTokenAluno(_factory.Aluno1Id));

        var response = await _client.GetAsync($"/api/pagamentos/{_factory.PagamentoAluno2Id}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "GET /api/pagamentos/{id} deve retornar 200 para administrador")]
    public async Task ObterPorId_Administrador_DeveRetornar200()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.GerarTokenAdmin(Guid.NewGuid()));

        var response = await _client.GetAsync($"/api/pagamentos/{_factory.PagamentoAluno2Id}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "GET /api/pagamentos/{id} deve retornar 404 para pagamento inexistente")]
    public async Task ObterPorId_Inexistente_DeveRetornar404()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.GerarTokenAdmin(Guid.NewGuid()));

        var response = await _client.GetAsync($"/api/pagamentos/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "POST /api/pagamentos deve retornar 401 sem autenticação")]
    public async Task RealizarPagamento_SemAutenticacao_DeveRetornar401()
    {
        var request = new RealizarPagamentoRequest
        {
            MatriculaId = Guid.NewGuid(),
            CursoId = Guid.NewGuid(),
            Total = 99.90m,
            NomeCartao = "Sem Auth",
            NumeroCartao = "4111111111111111",
            ExpiracaoCartao = "12/30",
            CvvCartao = "123"
        };

        var response = await _client.PostAsJsonAsync("/api/pagamentos", request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "POST /api/pagamentos deve retornar 400 quando payload inválido")]
    public async Task RealizarPagamento_PayloadInvalido_DeveRetornar400()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.GerarTokenAluno(_factory.Aluno1Id));

        var response = await _client.PostAsJsonAsync("/api/pagamentos", new RealizarPagamentoRequest(), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "POST /api/pagamentos deve retornar 200 e persistir pagamento quando payload válido")]
    public async Task RealizarPagamento_PayloadValido_DeveRetornar200EPersistirPagamento()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.GerarTokenAluno(_factory.Aluno1Id));

        var total = 199.91m;
        var quantidadeAntes = await _factory.ContarPagamentosAsync();

        var request = new RealizarPagamentoRequest
        {
            MatriculaId = Guid.NewGuid(),
            CursoId = Guid.NewGuid(),
            Total = total,
            NomeCartao = "Aluno Teste",
            NumeroCartao = "4111111111111111",
            ExpiracaoCartao = "12/30",
            CvvCartao = "123"
        };

        var response = await _client.PostAsJsonAsync("/api/pagamentos", request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await ParseResponseAsync(response);
        envelope.GetProperty("success").GetBoolean().Should().BeTrue();
        envelope.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Object);

        var quantidadeDepois = await _factory.ContarPagamentosAsync();
        quantidadeDepois.Should().Be(quantidadeAntes + 1);

        var pagamentoPersistido = await _factory.ObterPagamentoPorAlunoETotalAsync(_factory.Aluno1Id, total);
        pagamentoPersistido.Should().NotBeNull();
        pagamentoPersistido!.Transacao.Should().NotBeNull();
        pagamentoPersistido.Transacao!.StatusTransacaoId.Should().Be(StatusTransacao.Aprovado.Id);
    }

    private static async Task<JsonElement> ParseResponseAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
