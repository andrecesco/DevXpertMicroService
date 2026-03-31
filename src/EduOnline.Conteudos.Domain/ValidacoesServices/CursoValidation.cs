using EduOnline.Conteudos.Domain.Enumeradores;
using EduOnline.Core.DomainObjects;
using FluentValidation;

namespace EduOnline.Conteudos.Domain.ValidacoesServices;

public class CursoValidation : AbstractValidator<Curso>
{
    public CursoValidation()
    {
        RuleFor(c => c.Nome)
            .NotEmpty()
            .WithMessage("O campo {PropertyName} deve ser preenchido")
            .MaximumLength(100)
            .WithMessage("O campo {PropertyName} deve ter no máximo 100 caracteres");

        RuleFor(c => c.Autor)
            .NotEmpty()
            .WithMessage("O campo {PropertyName} deve ser preenchido")
            .MaximumLength(100)
            .WithMessage("O campo {PropertyName} deve ter no máximo 100 caracteres");

        RuleFor(m => m.Validade)
            .GreaterThan(DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("O campo {PropertyName} precisa ser fornecido com uma data futura");

        RuleFor(c => c.ConteudoProgramatico)
            .NotNull()
            .WithMessage("O campo {PropertyName} deve ser preenchido");

        When(c => c.ConteudoProgramatico != null, () =>
        {
            RuleFor(c => c.ConteudoProgramatico.Tema)
                .NotEmpty()
                .WithMessage("O campo {PropertyName} deve ser preenchido")
                .MaximumLength(2048)
                .WithMessage("O campo {PropertyName} deve ter no máximo 2048 caracteres");

            RuleFor(c => c.ConteudoProgramatico.NivelId)
                .GreaterThan(0)
                .WithMessage("O campo {PropertyName} deve ser preenchido")
                .Must(a => Enumerador.GetById<Nivel>(a) != null)
                .WithMessage("O campo {PropertyName} deve ser o nível válido.");

            RuleFor(c => c.ConteudoProgramatico.CargaHoraria)
                .InclusiveBetween(1, 1000)
                .WithMessage("O campo {PropertyName} deve ser preenchido com valores entre {From} e {To}");
        });
    }
}
