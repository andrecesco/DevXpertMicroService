using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Auth.ApiRest.Models;

[ExcludeFromCodeCoverage]
public class UsuarioLoginModel
{
    [Required(ErrorMessage = "O campo {0} é obrigatório!")]
    [EmailAddress(ErrorMessage = "O campo {0} é inválido!")]
    [DefaultValue("admin@eduonline.com")]
    public string? Email { get; set; } = "admin@eduonline.com";

    [Required(ErrorMessage = "O campo {0} é obrigatório!")]
    [StringLength(100, ErrorMessage = "O campo {0} precisa estar entre {2} e {1} caracteres!", MinimumLength = 6)]
    [DefaultValue("Teste@123")]
    public string? Senha { get; set; } = "Teste@123";
}

[ExcludeFromCodeCoverage]
public class UsuarioRegistroModel
{
    [Required(ErrorMessage = "O campo {0} é obrigatório!")]
    public required string Nome { get; set; }

    [Required(ErrorMessage = "O campo {0} é obrigatório!")]
    [EmailAddress(ErrorMessage = "O campo {0} é inválido!")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "O campo {0} é obrigatório!")]
    [StringLength(100, ErrorMessage = "O campo {0} precisa estar entre {2} e {1} caracteres!", MinimumLength = 6)]
    public required string Senha { get; set; }

    [Compare("Senha", ErrorMessage = "As senhas não conferem.")]
    public required string ConfirmaSenha { get; set; }

    [RegularExpression("^(Aluno|Administrador)$", ErrorMessage = "O perfil informado é inválido.")]
    public required string Perfil { get; set; } = "Aluno";
}

public class UsuarioTokenModel
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required IEnumerable<ClaimModel> Claims { get; set; }
}

public class UsuarioRepostaModel
{
    public required string AccessToken { get; set; }
    public required Guid RefreshToken { get; set; }
    public required double ExpiraEm { get; set; }
    public required UsuarioTokenModel UsuarioToken { get; set; }
}

public class ClaimModel
{
    public required string Value { get; set; }
    public required string Type { get; set; }
}
