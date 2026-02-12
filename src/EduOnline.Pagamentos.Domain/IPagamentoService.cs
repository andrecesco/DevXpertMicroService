using EduOnline.Core.DomainObjects.Dtos;

namespace EduOnline.Pagamentos.Domain;

public interface IPagamentoService
{
    Task<Transacao> RealizarPagamentoCurso(PagamentoCurso pagamentoCurso);
}
