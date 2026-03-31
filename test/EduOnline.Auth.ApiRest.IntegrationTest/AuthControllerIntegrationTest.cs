using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EduOnline.Auth.ApiRest.IntegrationTest;

public class AuthControllerIntegrationTest : IClassFixture<AuthApiTestFactory>
{
    private readonly HttpClient _client;
    private readonly AuthApiTestFactory _factory;

    public AuthControllerIntegrationTest(AuthApiTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "GET /api/auth/{id} deve retornar 401 sem autenticação")]
    public async Task ObterPorId_SemAutenticacao_DeveRetornar401()
    {
        var response = await _client.GetAsync($"/api/auth/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "DELETE /api/auth/{id} deve retornar 401 sem autenticação")]
    public async Task Excluir_SemAutenticacao_DeveRetornar401()
    {
        var response = await _client.DeleteAsync($"/api/auth/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "POST /api/auth/sair deve retornar 401 sem autenticação")]
    public async Task Logout_SemAutenticacao_DeveRetornar401()
    {
        var response = await _client.PostAsync("/api/auth/sair", null, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "POST /api/auth/refresh-token com payload inválido deve retornar 400")]
    public async Task RefreshToken_PayloadInvalido_DeveRetornar400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh-token", "token-invalido", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "POST /api/auth/entrar com credenciais válidas deve retornar token")]
    public async Task Login_ComCredenciaisValidas_DeveRetornarToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/entrar", new
        {
            email = AuthApiTestFactory.AdminEmail,
            senha = AuthApiTestFactory.Password
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc = await ParseResponseAsync(response);
        var root = doc.RootElement;

        root.GetProperty("success").GetBoolean().Should().BeTrue();
        var accessToken = root.GetProperty("data").GetProperty("accessToken").GetString();
        accessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "POST /api/auth/entrar com senha inválida deve retornar 400")]
    public async Task Login_ComSenhaInvalida_DeveRetornar400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/entrar", new
        {
            email = $"naoexiste_{Guid.NewGuid():N}@eduonline.com",
            senha = "SenhaInvalida@123"
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var doc = await ParseResponseAsync(response);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("errors").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "GET /api/auth/{id} com token inválido deve retornar 401")]
    public async Task ObterPorId_ComTokenInvalido_DeveRetornar401()
    {
        var adminId = await _factory.GetUserIdByEmailAsync(AuthApiTestFactory.AdminEmail);
        adminId.Should().NotBeNull();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token-invalido");

        var response = await _client.GetAsync($"/api/auth/{adminId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "GET /api/auth/{id} com token de Aluno para outro usuário deve retornar 401")]
    public async Task ObterPorId_ComTokenAlunoParaOutroUsuario_DeveRetornar401()
    {
        var (tokenAluno, _) = await LoginAsync(AuthApiTestFactory.AlunoEmail, AuthApiTestFactory.Password);
        var (_, adminId) = await LoginAsync(AuthApiTestFactory.AdminEmail, AuthApiTestFactory.Password);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAluno);

        var response = await _client.GetAsync($"/api/auth/{adminId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "GET /api/auth/{id} com token de Administrador deve retornar 200")]
    public async Task ObterPorId_ComTokenAdministrador_DeveRetornar200()
    {
        var (tokenAdmin, _) = await LoginAsync(AuthApiTestFactory.AdminEmail, AuthApiTestFactory.Password);
        var (_, alunoId) = await LoginAsync(AuthApiTestFactory.AlunoEmail, AuthApiTestFactory.Password);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAdmin);

        var response = await _client.GetAsync($"/api/auth/{alunoId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "DELETE /api/auth/{id} com token de Aluno deve retornar 403")]
    public async Task Excluir_ComTokenAluno_DeveRetornar403()
    {
        var (tokenAluno, _) = await LoginAsync(AuthApiTestFactory.AlunoEmail, AuthApiTestFactory.Password);
        var (_, adminId) = await LoginAsync(AuthApiTestFactory.AdminEmail, AuthApiTestFactory.Password);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAluno);

        var response = await _client.DeleteAsync($"/api/auth/{adminId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "DELETE /api/auth/{id} com token de Administrador para usuário pendente deve retornar 204")]
    public async Task Excluir_ComTokenAdministrador_UsuarioPendente_DeveRetornar204()
    {
        var novoEmail = $"del_{Guid.NewGuid():N}@eduonline.com";
        var criar = await _client.PostAsJsonAsync("/api/auth/nova-conta", new
        {
            nome = "Aluno Para Excluir",
            email = novoEmail,
            senha = "Teste@123",
            confirmaSenha = "Teste@123",
            perfil = "Aluno"
        }, TestContext.Current.CancellationToken);

        criar.StatusCode.Should().Be(HttpStatusCode.Created);
        var userId = criar.Headers.Location?.Segments.LastOrDefault();
        userId.Should().NotBeNullOrWhiteSpace();

        var (tokenAdmin, _) = await LoginAsync(AuthApiTestFactory.AdminEmail, AuthApiTestFactory.Password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAdmin);

        var response = await _client.DeleteAsync($"/api/auth/{userId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "POST /api/auth/nova-conta para perfil Aluno deve voltar 201")]
    public async Task NovaConta_PerfilAluno_DeveRetornar201()
    {
        var email = $"aluno_{Guid.NewGuid():N}@eduonline.com";
        var response = await _client.PostAsJsonAsync("/api/auth/nova-conta", new
        {
            nome = "Aluno Teste",
            email,
            senha = "Teste@123",
            confirmaSenha = "Teste@123",
            perfil = "Aluno"
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact(DisplayName = "POST /api/auth/nova-conta com e-mail existente deve retornar 400")]
    public async Task NovaConta_EmailExistente_DeveRetornar400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/nova-conta", new
        {
            nome = "Admin Duplicado",
            email = AuthApiTestFactory.AdminEmail,
            senha = "Teste@123",
            confirmaSenha = "Teste@123",
            perfil = "Aluno"
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var doc = await ParseResponseAsync(response);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
    }

    [Fact(DisplayName = "POST /api/auth/nova-conta para perfil Administrador sem autenticação deve retornar 400")]
    public async Task NovaConta_PerfilAdministradorSemAutenticacao_DeveRetornar400()
    {
        var email = $"admin_{Guid.NewGuid():N}@eduonline.com";
        var response = await _client.PostAsJsonAsync("/api/auth/nova-conta", new
        {
            nome = "Admin Teste",
            email,
            senha = "Teste@123",
            confirmaSenha = "Teste@123",
            perfil = "Administrador"
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var doc = await ParseResponseAsync(response);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("errors").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "POST /api/auth/refresh-token com token válido deve retornar 200")]
    public async Task RefreshToken_Valido_DeveRetornar200()
    {
        var (email, senha) = await CriarContaAlunoAsync();

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/entrar", new
        {
            email,
            senha
        }, TestContext.Current.CancellationToken);

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginJson = await loginResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var loginDoc = JsonDocument.Parse(loginJson);
        var refreshToken = loginDoc.RootElement.GetProperty("data").GetProperty("refreshToken").GetGuid();

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh-token", refreshToken.ToString(), TestContext.Current.CancellationToken);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshDoc = await ParseResponseAsync(refreshResponse);
        var newAccessToken = refreshDoc.RootElement.GetProperty("data").GetProperty("accessToken").GetString();
        newAccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "POST /api/auth/refresh-token com token expirado deve retornar 400")]
    public async Task RefreshToken_Expirado_DeveRetornar400()
    {
        var (email, senha) = await CriarContaAlunoAsync();

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/entrar", new
        {
            email,
            senha
        }, TestContext.Current.CancellationToken);

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginJson = await loginResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var loginDoc = JsonDocument.Parse(loginJson);
        var refreshToken = loginDoc.RootElement.GetProperty("data").GetProperty("refreshToken").GetGuid();

        await _factory.ExpireRefreshTokenAsync(refreshToken);

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh-token", refreshToken.ToString(), TestContext.Current.CancellationToken);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var doc = await ParseResponseAsync(refreshResponse);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
    }

    [Fact(DisplayName = "GET /api/auth/{id} com token válido e usuário inexistente deve retornar 404")]
    public async Task ObterPorId_ComTokenValidoEUsuarioInexistente_DeveRetornar404()
    {
        var (tokenAdmin, _) = await LoginAsync(AuthApiTestFactory.AdminEmail, AuthApiTestFactory.Password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAdmin);

        var response = await _client.GetAsync($"/api/auth/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "DELETE /api/auth/{id} com token de Administrador para usuário já cadastrado deve retornar 400")]
    public async Task Excluir_ComTokenAdministrador_UsuarioCadastrado_DeveRetornar400()
    {
        var (email, senha) = await CriarContaAlunoAsync();

        var loginAluno = await _client.PostAsJsonAsync("/api/auth/entrar", new { email, senha }, TestContext.Current.CancellationToken);
        loginAluno.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginAlunoDoc = await ParseResponseAsync(loginAluno);
        var alunoId = loginAlunoDoc.RootElement.GetProperty("data").GetProperty("usuarioToken").GetProperty("id").GetString();
        alunoId.Should().NotBeNullOrWhiteSpace();

        var (tokenAdmin, _) = await LoginAsync(AuthApiTestFactory.AdminEmail, AuthApiTestFactory.Password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAdmin);

        var response = await _client.DeleteAsync($"/api/auth/{alunoId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var doc = await ParseResponseAsync(response);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
    }

    [Fact(DisplayName = "POST /api/auth/nova-conta com perfil inválido deve retornar 400")]
    public async Task NovaConta_PerfilInvalido_DeveRetornar400()
    {
        var email = $"perfil_{Guid.NewGuid():N}@eduonline.com";
        var response = await _client.PostAsJsonAsync("/api/auth/nova-conta", new
        {
            nome = "Perfil Invalido",
            email,
            senha = "Teste@123",
            confirmaSenha = "Teste@123",
            perfil = "Instrutor"
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "POST /api/auth/nova-conta com confirmaSenha diferente deve retornar 400")]
    public async Task NovaConta_ConfirmaSenhaDiferente_DeveRetornar400()
    {
        var email = $"senha_{Guid.NewGuid():N}@eduonline.com";
        var response = await _client.PostAsJsonAsync("/api/auth/nova-conta", new
        {
            nome = "Senha Diferente",
            email,
            senha = "Teste@123",
            confirmaSenha = "OutraSenha@123",
            perfil = "Aluno"
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "POST /api/auth/refresh-token com GUID inexistente deve retornar 400")]
    public async Task RefreshToken_GuidInexistente_DeveRetornar400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh-token", Guid.NewGuid().ToString(), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var doc = await ParseResponseAsync(response);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
    }

    private async Task<(string token, string userId)> LoginAsync(string email, string senha)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/entrar", new { email, senha }, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var doc = JsonDocument.Parse(json);

        var data = doc.RootElement.GetProperty("data");
        var token = data.GetProperty("accessToken").GetString()!;
        var userId = data.GetProperty("usuarioToken").GetProperty("id").GetString()!;

        return (token, userId);
    }

    private async Task<(string email, string senha)> CriarContaAlunoAsync()
    {
        var email = $"refresh_{Guid.NewGuid():N}@eduonline.com";
        var senha = "Teste@123";

        var criar = await _client.PostAsJsonAsync("/api/auth/nova-conta", new
        {
            nome = "Aluno Refresh",
            email,
            senha,
            confirmaSenha = senha,
            perfil = "Aluno"
        }, TestContext.Current.CancellationToken);

        criar.StatusCode.Should().Be(HttpStatusCode.Created);

        return (email, senha);
    }

    private static async Task<JsonDocument> ParseResponseAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        return JsonDocument.Parse(json);
    }
}
