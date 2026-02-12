using EduOnline.Core.DomainObjects.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduOnline.Pagamentos.Domain;

public interface IPagamentoService
{
    Task<Transacao> RealizarPagamentoCurso(PagamentoCurso pagamentoCurso);
}
