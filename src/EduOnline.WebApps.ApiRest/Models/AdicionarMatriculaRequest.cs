namespace EduOnline.WebApps.ApiRest.Models;

public class AdicionarMatriculaRequest
{
    public Guid CursoId { get; set; }
    public string NomeCartao { get; set; }
    public string NumeroCartao { get; set; }
    public string ExpiracaoCartao { get; set; }
    public string CvvCartao { get; set; }
}
