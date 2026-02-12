using EduOnline.Core.DomainObjects;

namespace EduOnline.Pagamentos.Domain;

public class StatusTransacao(int id, string nome) : Enumerador(id, nome)
{
    public static StatusTransacao Aprovado => new(1, "Aprovado");
    public static StatusTransacao Recusado => new(2, "Recusdo");
}
