using System.ComponentModel.DataAnnotations;

namespace EduOnline.WebApps.ApiRest.Models;

public class AulaRequest
{
    [Required(ErrorMessage = "Titulo é obrigatório")]
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public string LinkMaterial { get; set; }
    [Required(ErrorMessage = "DuracaoEmMinutos é obrigatório")]
    public int DuracaoEmMinutos { get; set; }
}
