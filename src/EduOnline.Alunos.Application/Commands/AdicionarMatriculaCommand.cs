using EduOnline.Core.Mensagens;
using FluentValidation;

namespace EduOnline.Alunos.Application.Commands;

public class AdicionarMatriculaCommand(Guid alunoId, Guid cursoId, string? cursoNome, decimal valor, string? nomeCartao, string? numeroCartao,
    string? expiracaoCartao, string? cvvCartao, int totalAulas) : Command
{
    public Guid AlunoId { get; private set; } = alunoId;
    public Guid CursoId { get; private set; } = cursoId;
    public string? CursoNome { get; private set; } = cursoNome;
    public int TotalAulas { get; private set; } = totalAulas;
    public decimal Valor { get; private set; } = valor;
    public string? NomeCartao { get; private set; } = nomeCartao;
    public string? NumeroCartao { get; private set; } = numeroCartao;
    public string? ExpiracaoCartao { get; private set; } = expiracaoCartao;
    public string? CvvCartao { get; private set; } = cvvCartao;

    public override bool EhValido()
    {
        ValidationResult = new CommandValidation().Validate(this);
        return ValidationResult.IsValid;
    }

    public class CommandValidation : AbstractValidator<AdicionarMatriculaCommand>
    {
        public CommandValidation()
        {
            RuleFor(c => c.AlunoId)
                .NotEmpty()
                .WithMessage("O campo AlunoId deve ser preenchido");

            RuleFor(c => c.CursoId)
                .NotEmpty()
                .WithMessage("O campo CursoId deve ser preenchido");

            RuleFor(c => c.CursoNome)
                .NotEmpty()
                .WithMessage("O campo CursoNome deve ser preenchido")
                .MaximumLength(100)
                .WithMessage("O campo CursoNome deve ter no máximo 100 caracteres");

            RuleFor(c => c.Valor)
                .GreaterThan(0)
                .WithMessage("O campo Valor deve ser preenchido");

            RuleFor(c => c.NomeCartao)
                .NotEmpty()
                .WithMessage("O campo NomeCartao deve ser preenchido")
                .MaximumLength(100)
                .WithMessage("O campo NomeCartao deve ter no máximo 100 caracteres");

            RuleFor(c => c.NumeroCartao)
                .NotEmpty()
                .WithMessage("O campo NumeroCartao deve ser preenchido")
                .MaximumLength(100)
                .WithMessage("O campo NumeroCartao deve ter no máximo 100 caracteres");

            RuleFor(c => c.ExpiracaoCartao)
                .NotEmpty()
                .WithMessage("O campo ExpiracaoCartao deve ser preenchido")
                .MaximumLength(100)
                .WithMessage("O campo ExpiracaoCartao deve ter no máximo 100 caracteres");

            RuleFor(c => c.CvvCartao)
                .NotEmpty()
                .WithMessage("O campo CvvCartao deve ser preenchido")
                .MaximumLength(100)
                .WithMessage("O campo CvvCartao deve ter no máximo 100 caracteres");

            RuleFor(c => c.TotalAulas)
                .GreaterThan(0)
                .WithMessage("O campo TotalAulas deve ser preenchido");
        }
    }
}
