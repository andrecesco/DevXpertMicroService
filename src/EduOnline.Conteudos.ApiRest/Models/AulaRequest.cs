using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Conteudos.ApiRest.Models;

[ExcludeFromCodeCoverage]
public class AulaRequest
{
    [Required(ErrorMessage = "Titulo é obrigatório")]
    public string Titulo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public string LinkMaterial { get; set; } = string.Empty;

    [Required(ErrorMessage = "DuracaoEmMinutos é obrigatório")]
    public int DuracaoEmMinutos { get; set; }
}
