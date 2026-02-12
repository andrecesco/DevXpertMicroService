using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduOnline.Core.DomainObjects.Dtos;

public class PagamentoCurso
{ 
    public Guid MatriculaId { get; set; }
    public Guid CursoId { get; set; }
    public Guid AlunoId { get; set; }
    public decimal Total { get; set; }
    public string? NomeCartao { get; set; }
    public string? NumeroCartao { get; set; }
    public string? ExpiracaoCartao { get; set; }
    public string? CvvCartao { get; set; }
}
