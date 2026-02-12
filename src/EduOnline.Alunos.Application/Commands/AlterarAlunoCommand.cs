using EduOnline.Core.Mensagens;
using FluentValidation;

namespace EduOnline.Alunos.Application.Commands;

public class AlterarAlunoCommand : Command
{
    public string? Nome { get; private set; }
    public DateOnly DataNascimento { get; private set; }

    public AlterarAlunoCommand(Guid id, string? nome, DateOnly dataNascimento)
    {
        AggregateId = id;
        Nome = nome;
        DataNascimento = dataNascimento;
    }

    public override bool EhValido()
    {
        ValidationResult = new CommandValidation().Validate(this);
        return ValidationResult.IsValid;
    }

    public class CommandValidation : AbstractValidator<AlterarAlunoCommand>
    {
        public CommandValidation()
        {
            RuleFor(c => c.Nome)
                .NotEmpty()
                .WithMessage("O campo Nome deve ser preenchido")
                .MaximumLength(100)
                .WithMessage("O campo Nome deve ter no máximo 100 caracteres");

            RuleFor(c => c.DataNascimento)
                .NotEmpty()
                .WithMessage("O campo Data de Nascimento deve ser preenchido");
        }
    }
}
