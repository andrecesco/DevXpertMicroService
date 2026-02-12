using EduOnline.Core.DomainObjects;

namespace EduOnline.Alunos.Domain.ValueObjects;

public class HistoricoAprendizagem
{
    public int TotalAulas { get; private set; }
    public Guid[] AulasConcluidas { get; private set; } = [];

    public HistoricoAprendizagem(int totalAulas, Guid[] aulasConcluidas)
    {
        TotalAulas = totalAulas;
        AulasConcluidas = aulasConcluidas;
        Validar();
    }

    public void AdicionarAulaConcluida(Guid aulaConcluida)
    {
        if (AulasConcluidas.Contains(aulaConcluida))
            throw new DomainException("Aula já concluída");

        var aulas = AulasConcluidas.ToList();
        aulas.Add(aulaConcluida);
        AulasConcluidas = [.. aulas];
        Validar();
    }

    public void Validar()
    {
        Validacoes.ValidarMinimoMaximo(TotalAulas, 0, 1000, "O campo Total de Aulas deve ter no mínimo 0 e no máximo 1000");
        Validacoes.ValidarSeNulo(AulasConcluidas, "O campo Aulas Concluídas deve ser preenchido");
        if (AulasConcluidas.Length > TotalAulas)
            throw new DomainException("O campo Aulas Concluídas não pode ser maior que o Total de Aulas");
    }

    public bool TodasAulasConcluidas()
    {
        return AulasConcluidas.Length == TotalAulas;
    }

    public bool AulaFoiConcluida(Guid aulaId)
    {
        return AulasConcluidas.Contains(aulaId);
    }
}
