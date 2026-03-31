using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.WebApps.ApiRest.Models;

[ExcludeFromCodeCoverage]
public class CursoRequest
{
    [Required]
    public string Nome { get; set; }
    [Required]
    public string Autor { get; set; }
    [Required]
    public DateOnly Validade { get; set; }
    [Required]
    public decimal Valor { get; set; }
    public ConteudoProgramaticoRequest ConteudoProgramatico { get; set; } = new ConteudoProgramaticoRequest();
}
