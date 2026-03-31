using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EduOnline.Core.Api.Identidade;

public static class JwtConfig
{
    private const string SegredoPadraoDesenvolvimento = "360a1429-8cdb-45ec-ab2d-fccf2eb3521e";
    private const string SegredoPadraoAplicacao = "ChaveSuperSecretaParaJWT_2024_EduOnline_MinhaChaveDeve_TerMaisde32Caracteres!@#$%^&*";

    public static WebApplicationBuilder AddJwtConfiguration(this WebApplicationBuilder builder)
    {
        var appTokenSettings = builder.Configuration.GetSection("AppTokenSettings").Get<AppTokenSettings>();
        var legacyTokenSettings = builder.Configuration.GetSection("AppSettings").Get<AppTokenSettings>();

        var issuer = appTokenSettings?.Issuer ?? legacyTokenSettings?.Issuer;
        var audience = appTokenSettings?.Audience ?? legacyTokenSettings?.Audience;

        if (string.IsNullOrWhiteSpace(issuer))
            throw new InvalidOperationException("Configuração de issuer JWT não encontrada (AppTokenSettings/AppSettings).");

        if (string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("Configuração de audience JWT não encontrada (AppTokenSettings/AppSettings).");

        var segredos = new[]
        {
            appTokenSettings?.Segredo,
            legacyTokenSettings?.Segredo,
            SegredoPadraoDesenvolvimento,
            SegredoPadraoAplicacao
        }
        .Where(segredo => !string.IsNullOrWhiteSpace(segredo))
        .Distinct(StringComparer.Ordinal)
        .ToArray();

        if (segredos.Length == 0)
            throw new InvalidOperationException("Configuração de segredo JWT não encontrada (AppTokenSettings/AppSettings).");

        var signingKeys = segredos
            .Select(segredo => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(segredo!)))
            .Cast<SecurityKey>()
            .ToArray();

        var validIssuers = new[] { appTokenSettings?.Issuer, legacyTokenSettings?.Issuer, "https://localhost:7020" }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var validAudiences = new[] { appTokenSettings?.Audience, legacyTokenSettings?.Audience, "EduOnline-Dev" }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        builder.Services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
            x.SaveToken = true;

            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                ValidIssuers = validIssuers,
                ValidAudiences = validAudiences,
                ClockSkew = TimeSpan.Zero
            };
        });

        return builder;
    }

    public static void UseAuthConfiguration(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }
}
