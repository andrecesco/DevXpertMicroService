using System.ComponentModel.DataAnnotations;

namespace EduOnline.WebApps.ApiRest.Models;

public class ConteudoProgramaticoRequest
{
    [Required]
    public string Tema { get; set; }
    [Required]
    public int NivelId { get; set; }
    [Required]
    public int CargaHoraria { get; set; }
}
