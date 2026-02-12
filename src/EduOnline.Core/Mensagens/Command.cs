using FluentValidation.Results;
using MediatR;

namespace EduOnline.Core.Mensagens;

public abstract class Command : Message, IRequest<bool>
{
    public DateTime Timestamp { get; private set; }
    public ValidationResult? ValidationResult { get; set; }

    protected Command()
    {
        AggregateId = Guid.NewGuid();
        Timestamp = DateTime.Now;
    }

    public virtual bool EhValido()
    {
        throw new NotImplementedException();
    }
}
