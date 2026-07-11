using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;

namespace EduOnline.Core.Api.Extensions;

public static class HttpExtensions
{
    public static IHttpClientBuilder AllowSelfSignedCertificate(this IHttpClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.ConfigurePrimaryHttpMessageHandler(_ => ConfigureClientHandler());
    }

    public static HttpClientHandler ConfigureClientHandler()
    {
        var path = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path");
        var certPass = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password");

        if (!string.IsNullOrWhiteSpace(path))
            return new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (_, cert, chain, _) =>
                {
                    chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                    var certificate = X509CertificateLoader.LoadPkcs12(File.ReadAllBytes(path!), certPass);
                    chain.ChainPolicy.CustomTrustStore.Add(certificate);

                    return chain.Build(cert);
                }
            };

        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    }
}
