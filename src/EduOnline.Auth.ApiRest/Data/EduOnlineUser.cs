using Microsoft.AspNetCore.Identity;

namespace EduOnline.Auth.ApiRest.Data;

public class EduOnlineUser : IdentityUser
{
    public int StatusId { get; set; } = Status.Pendente.Id;
    public string StatusNome { get; set; } = Status.Pendente.Nome;
}
