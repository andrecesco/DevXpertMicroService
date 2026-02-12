using EduOnline.Alunos.Domain.Enumeradores;
using EduOnline.Alunos.Domain.ValueObjects;
using EduOnline.Core.DomainObjects;

namespace EduOnline.Alunos.Domain.Models;

public class Matricula : Entity
{
    public Guid AlunoId { get; private set; }
    public Guid CursoId { get; private set; }
    public string CursoNome { get; private set; } = string.Empty;
    public DateOnly Validade { get; private set; }
    public int StatusId { get; private set; }
    public int PagamentoStatusId { get; private set; }
    public HistoricoAprendizagem? HistoricoAprendizagem { get; private set; }
    public bool Ativo { get; private set; }

    //EF Relations
    public Certificado? Certificado { get; private set; }
    public Aluno? Aluno { get; private set; }

    private Matricula() { }

    public Matricula(Guid id, Guid alunoId, Guid cursoId, string cursoNome, DateOnly validade)
    {
        Id = id;
        AlunoId = alunoId;
        CursoId = cursoId;
        CursoNome = cursoNome;
        Ativo = true;
        Validade = validade;
        StatusId = MatriculaStatus.NaoInciado.Id;
        PagamentoStatusId = PagamentoStatus.NaoPago.Id;
        Validar();
    }

    public void AlterarCursoNome(string cursoNome)
    {
        CursoNome = cursoNome;
        Validar();
    }

    public void AlterarValidade(DateOnly validade)
    {
        Validade = validade;
        Validar();
    }

    public void AdicionarHistoricoAprendizagem(HistoricoAprendizagem historico)
    {
        HistoricoAprendizagem = historico;
    }

    public void AtualizarStatusParaIniciado()
    {
        StatusId = StatusId == MatriculaStatus.NaoInciado.Id && HistoricoAprendizagem?.AulasConcluidas.Length > 0
            ? MatriculaStatus.Iniciado.Id
            : StatusId;
    }

    public void AlterarMatriculaParaFinalizado()
    {
        StatusId = MatriculaStatus.Finalizado.Id;
    }

    public void Ativar() => Ativo = true;

    public void Desativar() => Ativo = false;

    public void AltararStatusPagamento(PagamentoStatus pagamentoStatus)
    {
        PagamentoStatusId = pagamentoStatus.Id;
    }

    void Validar()
    {
        Validacoes.ValidarSeIgual(Id, Guid.Empty, "O campo Id deve ser preenchido");
        Validacoes.ValidarSeIgual(AlunoId, Guid.Empty, "O campo AlunoId deve ser preenchido");
        Validacoes.ValidarSeIgual(CursoId, Guid.Empty, "O campo CursoId deve ser preenchido");
        Validacoes.ValidarSeVazio(CursoNome, "O campo CursoNome deve ser preenchido");
        Validacoes.ValidarTamanho(CursoNome, 100, "O campo CursoNome deve ter no máximo 100 caracteres");
        Validacoes.ValidarSeMenorQue(Validade, DateOnly.FromDateTime(DateTime.UtcNow), "O campo Validade deve ser maior ou igual a data atual");
    }
}
