using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Core.Api.Identidade;

[ExcludeFromCodeCoverage]
public class AppTokenSettings
{
    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    public string Issuer { get; set; } = string.Empty;

    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    public string Audience { get; set; } = string.Empty;

    [Required(ErrorMessage = "O Segredo JWT é obrigatório")]
    [MinLength(32, ErrorMessage = "O Segredo JWT deve ter no mínimo 32 caracteres para garantir a segurança.")]
    public string Segredo { get; set; } = string.Empty;
}
