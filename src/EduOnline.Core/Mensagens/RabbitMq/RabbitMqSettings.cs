using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Core.Mensagens.RabbitMq;

[ExcludeFromCodeCoverage]
public class RabbitMqSettings
{
    public bool Enabled { get; set; } = false;
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "eduonline.events";
}
