using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Auth.ApiRest.Models;

[ExcludeFromCodeCoverage]
public class RefreshToken
{
    public RefreshToken()
    {
        Id = Guid.NewGuid();
        Token = Guid.NewGuid();
    }

    public Guid Id { get; set; }
    public required string Username { get; set; }
    public Guid Token { get; set; }
    public DateTime ExpirationDate { get; set; }
}
