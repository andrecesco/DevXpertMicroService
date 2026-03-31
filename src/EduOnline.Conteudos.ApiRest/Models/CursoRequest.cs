using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Conteudos.ApiRest.Models;

[ExcludeFromCodeCoverage]
public class CursoRequest
{
    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public string Autor { get; set; } = string.Empty;

    [Required]
    public DateOnly Validade { get; set; }

    [Required]
    public decimal Valor { get; set; }

    [Required]
    public ConteudoProgramaticoRequest ConteudoProgramatico { get; set; } = new();
}
