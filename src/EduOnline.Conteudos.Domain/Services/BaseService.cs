using EduOnline.Core.Data;
using EduOnline.Core.DomainObjects;
using EduOnline.Core.Mensagens;
using FluentValidation;
using FluentValidation.Results;

namespace EduOnline.Conteudos.Domain.Services;

public class BaseService(INotificador notificador)
{
    protected ValidationResult ValidationResult = new();

    protected void Notificar(ValidationResult validationResult)
    {
        foreach (var item in validationResult.Errors)
        {
            Notificar(item.ErrorMessage);
        }
    }

    protected void Notificar(string mensagem)
    {
        notificador.Handle(new Notificacao(mensagem));
    }

    protected bool ExecutarValidacao<TV, TE>(TV validacao, TE entidade)
        where TV : AbstractValidator<TE>
        where TE : Entity
    {
        var validator = validacao.Validate(entidade);

        if (validator.IsValid) return true;

        Notificar(validator);

        return false;
    }

    public async Task PersistirDados(IUnitOfWork uow)
    {
        if (!await uow.Commit())
            Notificar("Ocorreu um erro ao salvar os dados no banco.");
    }
}
