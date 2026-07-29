using EduOnline.Bff.ApiRest.Services;
using EduOnline.Core.Api.Controllers;
using EduOnline.Core.Api.Extensions;
using Polly;
using System.Net;
using System.Reflection;

namespace EduOnline.WebApps.UnitTest.Services;

public class BaseServiceAndPollyCoverageTest
{
    [Fact]
    public async Task ProcessarResposta_Unauthorized_DeveRetornarErroDeAutenticacao()
    {
        var sut = new BaseServiceHarness();

        var result = await sut.Processar(new HttpResponseMessage(HttpStatusCode.Unauthorized), "x");

        Assert.False(result.Success);
        Assert.Equal("Não autenticado", result.Errors?.First().Value);
    }

    [Fact]
    public async Task ProcessarResposta_Forbidden_DeveRetornarErroDeAcessoNegado()
    {
        var sut = new BaseServiceHarness();

        var result = await sut.Processar(new HttpResponseMessage(HttpStatusCode.Forbidden), "x");

        Assert.False(result.Success);
        Assert.Equal("Acesso negado", result.Errors?.First().Value);
    }

    [Fact]
    public async Task ProcessarResposta_OkComBodyEmBranco_DeveRetornarSucesso()
    {
        var sut = new BaseServiceHarness();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("   ")
        };

        var result = await sut.Processar(response, "x");

        Assert.True(result.Success);
    }

    [Fact]
    public void GetRetryPolicy_ParaPost_DeveRetornarNoOp()
    {
        var metodo = typeof(PollyExtensions).GetMethod("GetRetryPolicy", BindingFlags.NonPublic | BindingFlags.Static)!;

        var resultado = metodo.Invoke(null, [new HttpRequestMessage(HttpMethod.Post, "http://localhost")]);

        Assert.NotNull(resultado);
        Assert.Contains("NoOp", resultado!.GetType().Name, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetRetryPolicy_ParaGet_DeveRetornarPolicyDeRetry()
    {
        var metodo = typeof(PollyExtensions).GetMethod("GetRetryPolicy", BindingFlags.NonPublic | BindingFlags.Static)!;

        var resultado = metodo.Invoke(null, [new HttpRequestMessage(HttpMethod.Get, "http://localhost")]);

        Assert.NotNull(resultado);
        Assert.DoesNotContain("NoOp", resultado!.GetType().Name, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class BaseServiceHarness : BaseService
    {
        public Task<ResponseResult> Processar(HttpResponseMessage response, string mensagemNotFound)
            => ProcessarResposta(response, mensagemNotFound);
    }
}
