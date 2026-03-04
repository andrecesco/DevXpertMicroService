using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Mensagens;
using EduOnline.Core.Mensagens.Notifications;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EduOnline.Core.Api.Controllers;

[ApiController]
public abstract class MainController : ControllerBase
{
    private readonly DomainNotificationHandler _domainNotifications;
    private readonly INotificador _notificador;
    public readonly IAspNetUser AppUser;

    protected Guid UsuarioId { get; set; }
    protected bool UsuarioAutenticado { get; set; }

    protected MainController(INotificador notificador,
                             IAspNetUser appUser)
    {
        _notificador = notificador;
        AppUser = appUser;

        if (appUser.IsAuthenticated())
        {
            UsuarioId = appUser.GetUserId();
            UsuarioAutenticado = true;
        }
    }

    protected MainController(INotificationHandler<DomainNotification> notifications,
                                 IAspNetUser appUser)
    {
        _domainNotifications = (DomainNotificationHandler)notifications;
        AppUser = appUser;

        if (appUser.IsAuthenticated())
        {
            UsuarioId = appUser.GetUserId();
            UsuarioAutenticado = true;
        }
    }

    protected bool OperacaoValida()
    {
        return !_notificador?.TemNotificacao() ?? false || !_domainNotifications.TemNotificacao();
    }

    protected ActionResult CustomResponse(object? result = null)
    {
        if (OperacaoValida())
        {
            var okResult = new ResponseResult(result, []);

            return Ok(okResult);
        }

        var errorResponse = new ResponseResult(null, _notificador?.ObterNotificacoes().Select(n => new DomainNotification(string.Empty, n.Mensagem)) ?? _domainNotifications.ObterNotificacoes());

        return BadRequest(errorResponse);
    }

    protected ActionResult NotificarValidationResult(ValidationResult validationResult)
    {
        if (!validationResult.IsValid)
        {
            foreach (var item in validationResult.Errors)
            {
                NotificarErro(item.ErrorMessage);
            }
        }

        return CustomResponse();
    }

    protected ActionResult CustomResponse(ModelStateDictionary modelState)
    {
        if (!modelState.IsValid) NotificarErroModelInvalida(modelState);
        return CustomResponse();
    }

    protected void NotificarErroModelInvalida(ModelStateDictionary modelState)
    {
        var erros = modelState.Values.SelectMany(e => e.Errors);
        foreach (var erro in erros)
        {
            var errorMsg = erro.Exception == null ? erro.ErrorMessage : erro.Exception.Message;
            NotificarErro(errorMsg);
        }
    }

    protected void NotificarErro(string mensagem)
    {
        _notificador?.Handle(new Notificacao(mensagem));

        _domainNotifications?.Handle(new DomainNotification(Guid.NewGuid().ToString(), mensagem), CancellationToken.None).Wait();
    }
}


public class ResponseResult(object? data, IEnumerable<DomainNotification> errors)
{
    public bool Success { get; set; } = errors?.Count() == 0;
    public object? Data { get; set; } = data;
    public IEnumerable<DomainNotification>? Errors { get; set; } = errors;
}
