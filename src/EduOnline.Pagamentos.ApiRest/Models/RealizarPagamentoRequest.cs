using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Pagamentos.ApiRest.Models;

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
    public string NomeCartao { get; set; } = string.Empty;

    [Required]
    public string NumeroCartao { get; set; } = string.Empty;

    [Required]
    public string ExpiracaoCartao { get; set; } = string.Empty;

    [Required]
    public string CvvCartao { get; set; } = string.Empty;
}
