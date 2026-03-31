using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.WebApps.ApiRest.Models;

[ExcludeFromCodeCoverage]
public class RealizarPagamentoRequest
{
    [Required]
    public Guid MatriculaId { get; set; }

    [Required]
    public Guid CursoId { get; set; }

    [Required]
    public decimal Total { get; set; }

    [Required]
    public string NomeCartao { get; set; }

    [Required]
    public string NumeroCartao { get; set; }

    [Required]
    public string ExpiracaoCartao { get; set; }

    [Required]
    public string CvvCartao { get; set; }
}
