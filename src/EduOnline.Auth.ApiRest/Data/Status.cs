using EduOnline.Core.DomainObjects;

namespace EduOnline.Auth.ApiRest.Data;

public sealed class Status : Enumerador
{
    public static Status Pendente => new(1, "Pendente");
    public static Status Cadastrado => new(2, "Cadastrado");

    private Status(int id, string nome) : base(id, nome) { }
}
