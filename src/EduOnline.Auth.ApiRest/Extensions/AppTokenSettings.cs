using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Auth.ApiRest.Extensions;

[ExcludeFromCodeCoverage]
public class AppTokenSettings
{
    [Range(1, 365, ErrorMessage = "A expiração do Refresh Token deve ser entre 1 e 365 dias.")]
    [DefaultValue(8)]
    public int RefreshTokenExpiration { get; set; }

    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    public string Issuer { get; set; } = string.Empty;

    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    public string Audience { get; set; } = string.Empty;

    [Required(ErrorMessage = "O Segredo JWT é obrigatório")]
    [MinLength(32, ErrorMessage = "O Segredo JWT deve ter no mínimo 32 caracteres para garantir a segurança.")]
    public string Segredo { get; set; } = string.Empty;
}
