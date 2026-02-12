using EduOnline.Core.Mensagens;
using FluentValidation;

namespace EduOnline.Alunos.Application.Commands;

public class AtualizarHistoricoCommand(Guid matriculaId, Guid aulaId) : Command
{
    public Guid MatriculaId { get; private set; } = matriculaId;
    public Guid AulaId { get; private set; } = aulaId;

    public override bool EhValido()
    {
        ValidationResult = new CommandValidation().Validate(this);
        return ValidationResult.IsValid;
    }

    public class CommandValidation : AbstractValidator<AtualizarHistoricoCommand>
    {
        public CommandValidation()
        {
            RuleFor(c => c.MatriculaId)
                .NotEmpty()
                .WithMessage("O campo AlunoId deve ser preenchido");
            RuleFor(c => c.AulaId)
                .NotEmpty()
                .WithMessage("O campo CursoId deve ser preenchido");
        }
    }
}
