using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Conteudos.ApiRest.Models;

[ExcludeFromCodeCoverage]
public class ConteudoProgramaticoRequest
{
    [Required]
    public string Tema { get; set; } = string.Empty;

    [Required]
    public int NivelId { get; set; }

    [Required]
    public int CargaHoraria { get; set; }
}
