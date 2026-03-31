using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace EduOnline.Core.Api.Extensions;

public static class ConfigurationValidationExtensions
{
    public static WebApplicationBuilder ValidateRabbitMqWhenEnabled(this WebApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection("RabbitMq");

        if (!section.Exists())
            return builder;

        var enabled = section.GetValue<bool?>("Enabled") ?? false;
        if (!enabled)
            return builder;

        var host = section["HostName"];
        var user = section["UserName"];
        var password = section["Password"];
        var exchange = section["ExchangeName"];
        var port = section.GetValue<int?>("Port");

        if (string.IsNullOrWhiteSpace(host))
            throw new InvalidOperationException("Configuração RabbitMq:HostName é obrigatória quando RabbitMq:Enabled=true.");

        if (port is null || port <= 0)
            throw new InvalidOperationException("Configuração RabbitMq:Port inválida quando RabbitMq:Enabled=true.");

        if (string.IsNullOrWhiteSpace(user))
            throw new InvalidOperationException("Configuração RabbitMq:UserName é obrigatória quando RabbitMq:Enabled=true.");

        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Configuração RabbitMq:Password é obrigatória quando RabbitMq:Enabled=true.");

        if (string.IsNullOrWhiteSpace(exchange))
            throw new InvalidOperationException("Configuração RabbitMq:ExchangeName é obrigatória quando RabbitMq:Enabled=true.");

        return builder;
    }
}
