using EduOnline.Core.Api.Controllers;
using EduOnline.Core.Mensagens.Notifications;

namespace EduOnline.WebApps.UnitTest;

internal static class ResponseResultHelper
{
    public static ResponseResult Ok(object? data = null)
        => new(data, []);

    public static ResponseResult Erro(string mensagem)
        => new(null, [new DomainNotification(string.Empty, mensagem)]);
}
