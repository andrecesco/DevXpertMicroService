using EduOnline.Core.DomainObjects;

namespace EduOnline.Conteudos.Domain.Enumeradores;

public class Nivel(int id, string nome) : Enumerador(id, nome)
{
    public static Nivel Iniciante => new(1, "Iniciante");
    public static Nivel Intermediario => new(2, "Intermediário");
    public static Nivel Avancado => new(3, "Avançado");
}
