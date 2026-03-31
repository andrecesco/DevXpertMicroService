using EduOnline.Core.Api.Extensions;
using FluentAssertions;
using System.Net;

namespace EduOnline.Pagamentos.IntegrationTest;

[Collection(PagamentosIntegrationCollection.Name)]
public class HealthChecksIntegrationTest
{
    private readonly HttpClient _client;

    public HealthChecksIntegrationTest(PagamentosApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "GET /health deve retornar 200 na Pagamentos API")]
    public async Task Health_DeveRetornar200()
    {
        var response = await _client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Contains(CorrelationIdMiddleware.HeaderName).Should().BeTrue();
    }

    [Fact(DisplayName = "GET /health/ready deve retornar 200 na Pagamentos API")]
    public async Task HealthReady_DeveRetornar200()
    {
        var response = await _client.GetAsync("/health/ready", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Contains(CorrelationIdMiddleware.HeaderName).Should().BeTrue();
    }
}
