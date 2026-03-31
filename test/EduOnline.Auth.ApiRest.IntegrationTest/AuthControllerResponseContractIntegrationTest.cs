using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EduOnline.Auth.ApiRest.IntegrationTest;

public class AuthControllerResponseContractIntegrationTest : IClassFixture<AuthApiTestFactory>
{
    private readonly HttpClient _client;

    public AuthControllerResponseContractIntegrationTest(AuthApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "Contrato de sucesso do login deve conter success/data/errors")]
    public async Task Login_Sucesso_DeveRespeitarContratoDeResposta()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/entrar", new
        {
            email = AuthApiTestFactory.AdminEmail,
            senha = AuthApiTestFactory.Password
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = await ParseResponseAsync(response);
        var root = doc.RootElement;

        root.TryGetProperty("success", out var successProp).Should().BeTrue();
        root.TryGetProperty("data", out var dataProp).Should().BeTrue();
        root.TryGetProperty("errors", out var errorsProp).Should().BeTrue();

        successProp.GetBoolean().Should().BeTrue();
        dataProp.ValueKind.Should().Be(JsonValueKind.Object);
        errorsProp.ValueKind.Should().Be(JsonValueKind.Array);

        dataProp.TryGetProperty("accessToken", out var accessToken).Should().BeTrue();
        dataProp.TryGetProperty("refreshToken", out var refreshToken).Should().BeTrue();
        dataProp.TryGetProperty("usuarioToken", out var usuarioToken).Should().BeTrue();

        accessToken.GetString().Should().NotBeNullOrWhiteSpace();
        refreshToken.GetString().Should().NotBeNullOrWhiteSpace();
        usuarioToken.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact(DisplayName = "Contrato de erro no login inválido deve conter success=false e errors")]
    public async Task Login_Erro_DeveRespeitarContratoDeResposta()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/entrar", new
        {
            email = AuthApiTestFactory.AdminEmail,
            senha = "SenhaInvalida@123"
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var doc = await ParseResponseAsync(response);
        var root = doc.RootElement;

        root.TryGetProperty("success", out var successProp).Should().BeTrue();
        root.TryGetProperty("data", out var dataProp).Should().BeTrue();
        root.TryGetProperty("errors", out var errorsProp).Should().BeTrue();

        successProp.GetBoolean().Should().BeFalse();
        dataProp.ValueKind.Should().Be(JsonValueKind.Null);
        errorsProp.ValueKind.Should().Be(JsonValueKind.Array);
        errorsProp.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "Contrato de erro no refresh token inválido deve conter errors")]
    public async Task RefreshToken_Invalido_DeveRespeitarContratoDeResposta()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh-token", "token-invalido", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var doc = await ParseResponseAsync(response);
        var root = doc.RootElement;

        root.GetProperty("success").GetBoolean().Should().BeFalse();
        root.GetProperty("errors").ValueKind.Should().Be(JsonValueKind.Array);
        root.GetProperty("errors").GetArrayLength().Should().BeGreaterThan(0);
    }

    private static async Task<JsonDocument> ParseResponseAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        return JsonDocument.Parse(json);
    }
}
