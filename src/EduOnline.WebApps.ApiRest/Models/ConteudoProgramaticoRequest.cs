using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.WebApps.ApiRest.Models;

[ExcludeFromCodeCoverage]
public class ConteudoProgramaticoRequest
{
    [Required]
    public string Tema { get; set; }
    [Required]
    public int NivelId { get; set; }
    [Required]
    public int CargaHoraria { get; set; }
}
