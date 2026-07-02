using EduOnline.Core.Api.Extensions;
using FluentAssertions;
using System.Net;

namespace EduOnline.Auth.ApiRest.IntegrationTest;

public class HealthChecksIntegrationTest : IClassFixture<AuthApiTestFactory>
{
    private readonly HttpClient _client;

    public HealthChecksIntegrationTest(AuthApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "GET /health deve retornar 200 na Auth API")]
    public async Task Health_DeveRetornar200()
    {
        var response = await _client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Contains(CorrelationIdMiddleware.HeaderName).Should().BeTrue();
    }

    [Fact(DisplayName = "GET /health/ready deve retornar 200 na Auth API")]
    public async Task HealthReady_DeveRetornar200()
    {
        var response = await _client.GetAsync("/health/ready", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Contains(CorrelationIdMiddleware.HeaderName).Should().BeTrue();
    }

    [Fact(DisplayName = "GET /metrics deve retornar métricas na Auth API")]
    public async Task Metrics_DeveRetornar200()
    {
        var response = await _client.GetAsync("/metrics", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Contains(CorrelationIdMiddleware.HeaderName).Should().BeTrue();
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/plain");

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().Contain("# HELP");
        content.Should().Contain("# TYPE");
    }
}
