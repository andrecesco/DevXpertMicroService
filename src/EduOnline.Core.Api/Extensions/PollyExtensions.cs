using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

namespace EduOnline.Core.Api.Extensions;

public static class PollyExtensions
{
    private static AsyncRetryPolicy<HttpResponseMessage> TempoDeEsperaIdempotente
        => HttpPolicyExtensions.HandleTransientHttpError()
            .WaitAndRetryAsync(new[] {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            });

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(HttpRequestMessage request)
    {
        // Não faz retry para POST
        if (request.Method == HttpMethod.Post)
        {
            return Policy.NoOpAsync<HttpResponseMessage>();
        }

        return TempoDeEsperaIdempotente;
    }

    public static void AddHttpClientService<TClient, TImplementation>(this IServiceCollection services)
        where TClient : class
        where TImplementation : class, TClient
    {
        services.AddHttpContextAccessor();
        services.AddTransient<HttpClientAuthorizationDelegatingHandler>();

        services.AddHttpClient<TClient, TImplementation>()
            .AddPolicyHandler(GetRetryPolicy)
            .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)))
            .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
    }
}
