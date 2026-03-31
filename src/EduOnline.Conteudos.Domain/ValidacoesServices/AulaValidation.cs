using FluentValidation;

namespace EduOnline.Conteudos.Domain.ValidacoesServices;

public class AulaValidation : AbstractValidator<Aula>
{
    public AulaValidation()
    {
        RuleFor(a => a.CursoId)
            .NotEqual(Guid.Empty)
            .WithMessage("O campo {PropertyName} deve ser preenchido");
        RuleFor(a => a.Titulo)
            .NotEmpty()
            .WithMessage("O campo {PropertyName} deve ser preenchido")
            .MaximumLength(100)
            .WithMessage("O campo {PropertyName} deve ter no máximo 100 caracteres");
        RuleFor(a => a.Descricao)
            .MaximumLength(100)
            .WithMessage("O campo {PropertyName} deve ter no máximo 100 caracteres");
        RuleFor(a => a.LinkMaterial)
            .NotEmpty()
            .WithMessage("O campo {PropertyName} deve ser preenchido")
            .MaximumLength(2048)
            .WithMessage("O campo {PropertyName} deve ter no máximo 2048 caracteres");
    }
}
