using EduOnline.Bff.ApiRest.Options;
using EduOnline.Bff.ApiRest.Requests;
using EduOnline.Bff.ApiRest.Services;
using EduOnline.Core.Api.Controllers;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.Text.Json;

namespace EduOnline.WebApps.UnitTest.Services;

public class BaseServiceAndAuthServiceTest
{
    [Fact]
    public async Task ProcessarResposta_NotFound_DeveRetornarErro()
    {
        var sut = new BaseServiceHarness();
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        var result = await sut.Processar(response, "não encontrado");

        Assert.False(result.Success);
        Assert.Equal("não encontrado", result.Errors?.First().Value);
    }

    [Fact]
    public async Task ProcessarResposta_BadRequestSemBody_DeveRetornarErroPadrao()
    {
        var sut = new BaseServiceHarness();
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(string.Empty)
        };

        var result = await sut.Processar(response, "x");

        Assert.False(result.Success);
        Assert.Equal("Falha na chamada do serviço (400)", result.Errors?.First().Value);
    }

    [Fact]
    public async Task ProcessarResposta_BadRequestComBody_DeveDesserializarResponseResult()
    {
        var sut = new BaseServiceHarness();
        var payload = new ResponseResult(null, [])
        {
            Success = false,
            Errors = [new("", "erro customizado")]
        };

        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        var result = await sut.Processar(response, "x");

        Assert.False(result.Success);
        Assert.Equal("erro customizado", result.Errors?.First().Value);
    }

    [Fact]
    public async Task ProcessarResposta_NoContent_DeveRetornarOk()
    {
        var sut = new BaseServiceHarness();
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);

        var result = await sut.Processar(response, "x");

        Assert.True(result.Success);
    }

    [Fact]
    public async Task DeserializarObjetoResponse_ComBodyVazio_DeveLancarExcecao()
    {
        var sut = new BaseServiceHarness();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty)
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Deserializar<ResponseResult>(response));

        Assert.Equal("O response está com o body vazio!", exception.Message);
    }

    [Fact]
    public void CapturarGuidInserido_StatusNaoCreated_DeveRetornarGuidVazio()
    {
        var sut = new BaseServiceHarness();
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        var result = sut.CapturarGuid(response);

        Assert.Equal(Guid.Empty, result);
    }

    [Fact]
    public void CapturarGuidInserido_CreatedComLocationInvalida_DeveLancarExcecao()
    {
        var sut = new BaseServiceHarness();
        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Headers = { Location = new Uri("http://localhost/nova-conta/invalido") }
        };

        Assert.Throws<InvalidOperationException>(() => sut.CapturarGuid(response));
    }

    [Fact]
    public async Task CriarUsuarioIdentity_ComCreatedDeveDefinirAggregateId()
    {
        var id = Guid.NewGuid();
        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Headers = { Location = new Uri($"http://localhost/nova-conta/{id}") }
        };

        var handler = new StubHttpMessageHandler(_ => response);
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new ServiceUrlOptions { AuthUrl = "http://localhost/" });
        var service = new AuthService(httpClient, options);

        var result = await service.CriarUsuarioIdentity(new CriarUsuarioRequest());

        Assert.True(result.Success);
        Assert.Equal(id, service.AggregateId);
        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.EndsWith("nova-conta", handler.LastRequest?.RequestUri?.ToString());
    }

    [Fact]
    public async Task ObterUsuarioPorId_NotFound_DeveRetornarMensagemEsperada()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new ServiceUrlOptions { AuthUrl = "http://localhost/" });
        var service = new AuthService(httpClient, options);

        var result = await service.ObterUsuarioPorId(Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("Usuário não encontrado", result.Errors?.First().Value);
    }

    private sealed class BaseServiceHarness : BaseService
    {
        public Task<ResponseResult> Processar(HttpResponseMessage response, string mensagemNotFound)
            => ProcessarResposta(response, mensagemNotFound);

        public Task<T> Deserializar<T>(HttpResponseMessage response)
            => DeserializarObjetoResponse<T>(response);

        public Guid CapturarGuid(HttpResponseMessage response)
            => CapturarGuidInserido(response);
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler = handler;

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_handler(request));
        }
    }
}
