using Microsoft.AspNetCore.Http;

namespace EduOnline.Core.Api.Extensions;

public class HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var currentRequestHeaders = _httpContextAccessor.HttpContext?.Request.Headers;
        var headersToIgnore = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Host",
            "Content-Length",
            "Content-Type",
            "Transfer-Encoding",
            "Connection",
            "Authorization"
        };

        if (currentRequestHeaders != null)
        {
            if (currentRequestHeaders.TryGetValue("Authorization", out var authorizationValues))
            {
                request.Headers.Remove("Authorization");
                request.Headers.TryAddWithoutValidation("Authorization", authorizationValues.ToArray());
            }

            foreach (var header in currentRequestHeaders)
            {
                if (!string.IsNullOrEmpty(header.Key) &&
                    !headersToIgnore.Contains(header.Key) &&
                    !request.Headers.Contains(header.Key))
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToString());
                }
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
