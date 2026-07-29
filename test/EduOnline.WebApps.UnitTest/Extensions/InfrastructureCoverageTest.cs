using EduOnline.Core.Api.Extensions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Net;

namespace EduOnline.WebApps.UnitTest.Extensions;

public class InfrastructureCoverageTest
{
    [Fact]
    public void ValidateRabbitMqWhenEnabled_SemSecao_DeveRetornarBuilder()
    {
        var builder = CriarBuilder();

        var resultado = builder.ValidateRabbitMqWhenEnabled();

        Assert.Same(builder, resultado);
    }

    [Fact]
    public void ValidateRabbitMqWhenEnabled_Desabilitado_DeveRetornarBuilder()
    {
        var builder = CriarBuilder(new Dictionary<string, string?>
        {
            ["RabbitMq:Enabled"] = "false"
        });

        var resultado = builder.ValidateRabbitMqWhenEnabled();

        Assert.Same(builder, resultado);
    }

    [Theory]
    [InlineData(null, "5672", "guest", "guest", "eduonline", "Configuração RabbitMq:HostName é obrigatória quando RabbitMq:Enabled=true.")]
    [InlineData("rabbitmq", "0", "guest", "guest", "eduonline", "Configuração RabbitMq:Port inválida quando RabbitMq:Enabled=true.")]
    [InlineData("rabbitmq", "5672", null, "guest", "eduonline", "Configuração RabbitMq:UserName é obrigatória quando RabbitMq:Enabled=true.")]
    [InlineData("rabbitmq", "5672", "guest", null, "eduonline", "Configuração RabbitMq:Password é obrigatória quando RabbitMq:Enabled=true.")]
    [InlineData("rabbitmq", "5672", "guest", "guest", null, "Configuração RabbitMq:ExchangeName é obrigatória quando RabbitMq:Enabled=true.")]
    public void ValidateRabbitMqWhenEnabled_ComConfiguracaoInvalida_DeveLancarExcecao(
        string? host,
        string? port,
        string? user,
        string? password,
        string? exchange,
        string mensagemEsperada)
    {
        var builder = CriarBuilder(new Dictionary<string, string?>
        {
            ["RabbitMq:Enabled"] = "true",
            ["RabbitMq:HostName"] = host,
            ["RabbitMq:Port"] = port,
            ["RabbitMq:UserName"] = user,
            ["RabbitMq:Password"] = password,
            ["RabbitMq:ExchangeName"] = exchange
        });

        var ex = Assert.Throws<InvalidOperationException>(() => builder.ValidateRabbitMqWhenEnabled());

        Assert.Equal(mensagemEsperada, ex.Message);
    }

    [Fact]
    public void ValidateRabbitMqWhenEnabled_ComConfiguracaoValida_DeveRetornarBuilder()
    {
        var builder = CriarBuilder(new Dictionary<string, string?>
        {
            ["RabbitMq:Enabled"] = "true",
            ["RabbitMq:HostName"] = "rabbitmq",
            ["RabbitMq:Port"] = "5672",
            ["RabbitMq:UserName"] = "guest",
            ["RabbitMq:Password"] = "guest",
            ["RabbitMq:ExchangeName"] = "eduonline"
        });

        var resultado = builder.ValidateRabbitMqWhenEnabled();

        Assert.Same(builder, resultado);
    }

    [Fact]
    public async Task HttpClientAuthorizationDelegatingHandler_DeveCopiarHeadersPermitidos()
    {
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        accessor.HttpContext.Request.Headers.Authorization = "Bearer token";
        accessor.HttpContext.Request.Headers["X-Correlation-Id"] = "123";
        accessor.HttpContext.Request.Headers.Host = "host-local";

        HttpRequestMessage? requestCapturada = null;
        var handler = new HttpClientAuthorizationDelegatingHandler(accessor)
        {
            InnerHandler = new StubHttpMessageHandler(request =>
            {
                requestCapturada = request;
                return new HttpResponseMessage(HttpStatusCode.OK);
            })
        };

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/teste");
        request.Headers.Add("X-Existing", "manter");

        var response = await new HttpMessageInvoker(handler).SendAsync(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(requestCapturada);
        Assert.Equal("Bearer token", requestCapturada.Headers.GetValues("Authorization").Single());
        Assert.Equal("123", requestCapturada.Headers.GetValues("X-Correlation-Id").Single());
        Assert.Equal("manter", requestCapturada.Headers.GetValues("X-Existing").Single());
        Assert.False(requestCapturada.Headers.Contains("Host"));
    }

    [Fact]
    public async Task HttpClientAuthorizationDelegatingHandler_SemHttpContext_DeveSeguirFluxo()
    {
        var handler = new HttpClientAuthorizationDelegatingHandler(new HttpContextAccessor())
        {
            InnerHandler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Accepted))
        };

        var response = await new HttpMessageInvoker(handler).SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost/teste"), CancellationToken.None);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public void AddDefaultCorsByEnvironment_Development_DevePermitirQualquerOrigem()
    {
        var services = new ServiceCollection();
        var env = new FakeHostEnvironment { EnvironmentName = Environments.Development };
        var config = new ConfigurationBuilder().Build();

        services.AddDefaultCorsByEnvironment(env, config);
        var policy = ObterPolicy(services);

        Assert.Contains("*", policy.Origins);
    }

    [Fact]
    public void AddDefaultCorsByEnvironment_ProductionComOrigins_DeveUsarListaConfigurada()
    {
        var services = new ServiceCollection();
        var env = new FakeHostEnvironment { EnvironmentName = Environments.Production };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "https://app.eduonline.com",
                ["Cors:AllowedOrigins:1"] = "https://admin.eduonline.com"
            })
            .Build();

        services.AddDefaultCorsByEnvironment(env, config);
        var policy = ObterPolicy(services);

        Assert.Contains("https://app.eduonline.com", policy.Origins);
        Assert.Contains("https://admin.eduonline.com", policy.Origins);
        Assert.DoesNotContain("*", policy.Origins);
    }

    [Fact]
    public void AddDefaultCorsByEnvironment_ProductionSemOrigins_DevePermitirQualquerOrigem()
    {
        var services = new ServiceCollection();
        var env = new FakeHostEnvironment { EnvironmentName = Environments.Production };
        var config = new ConfigurationBuilder().Build();

        services.AddDefaultCorsByEnvironment(env, config);
        var policy = ObterPolicy(services);

        Assert.Contains("*", policy.Origins);
    }

    private static WebApplicationBuilder CriarBuilder(IDictionary<string, string?>? valores = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });

        if (valores is not null)
        {
            builder.Configuration.AddInMemoryCollection(valores);
        }

        return builder;
    }

    private static CorsPolicy ObterPolicy(IServiceCollection services)
    {
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<CorsOptions>>().Value;
        return options.GetPolicy("Total")!;
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responder(request));
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "EduOnline.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
