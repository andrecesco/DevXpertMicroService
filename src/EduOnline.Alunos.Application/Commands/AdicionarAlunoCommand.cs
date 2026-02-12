using EduOnline.Core.Mensagens;
using FluentValidation;

namespace EduOnline.Alunos.Application.Commands;

public class AdicionarAlunoCommand : Command
{
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    public AdicionarAlunoCommand(Guid id, string nome, string email)
    {
        AggregateId = id;
        Nome = nome;
        Email = email;
    }

    public override bool EhValido()
    {
        ValidationResult = new CommandValidation().Validate(this);
        return ValidationResult.IsValid;
    }

    public class CommandValidation : AbstractValidator<AdicionarAlunoCommand>
    {
        public CommandValidation()
        {
            RuleFor(c => c.Nome)
                .NotEmpty()
                .WithMessage("O campo Nome deve ser preenchido")
                .MaximumLength(100)
                .WithMessage("O campo Nome deve ter no máximo 100 caracteres");

            RuleFor(c => c.Email)
                .EmailAddress()
                .WithMessage("O campo Email deve ser preenchido corretamente")
                .MaximumLength(100)
                .WithMessage("O campo Email deve ter no máximo 100 caracteres");
        }
    }
}
