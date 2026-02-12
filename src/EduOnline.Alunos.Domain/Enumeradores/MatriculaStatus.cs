using EduOnline.Core.DomainObjects;

namespace EduOnline.Alunos.Domain.Enumeradores;

public class MatriculaStatus(int id, string nome) : Enumerador(id, nome)
{
    public static MatriculaStatus NaoInciado => new(1, "Não Iniciado");
    public static MatriculaStatus Iniciado => new(2, "Iniciado");
    public static MatriculaStatus Finalizado => new(3, "Finalizado");
}
