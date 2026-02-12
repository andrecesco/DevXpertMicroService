using EduOnline.Core.DomainObjects;

namespace EduOnline.Alunos.Domain.Enumeradores;

public class PagamentoStatus(int id, string nome) : Enumerador(id, nome)
{
    public static PagamentoStatus NaoPago => new(1, "Não Pago");
    public static PagamentoStatus Pago => new(2, "Pago");
    public static PagamentoStatus Recusado => new(3, "Recusado");
}
