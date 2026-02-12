using System.ComponentModel.DataAnnotations;

namespace EduOnline.WebApps.ApiRest.Models;

public class UsuarioLoginModel
{
    [Required(ErrorMessage = "O campo {0} é obrigatório!")]
    [EmailAddress(ErrorMessage = "O campo {0} é inválido!")]
    public string Email { get; set; }

    [Required(ErrorMessage = "O campo {0} é obrigatório!")]
    [StringLength(100, ErrorMessage = "O campo {0} precisa estar entre {2} e {1} caracteres!", MinimumLength = 6)]
    public string Senha { get; set; }
}

public class UsuarioRegistroModel
{
    [Required(ErrorMessage = "O campo {0} é obrigatório!")]
    public string Nome { get; set; }

    [Required(ErrorMessage = "O campo {0} é obrigatório!")]
    [EmailAddress(ErrorMessage = "O campo {0} é inválido!")]
    public string Email { get; set; }

    [Required(ErrorMessage = "O campo {0} é obrigatório!")]
    [StringLength(100, ErrorMessage = "O campo {0} precisa estar entre {2} e {1} caracteres!", MinimumLength = 6)]
    public string Senha { get; set; }

    [Compare("Senha", ErrorMessage = "As senhas não conferem.")]
    public string ConfirmaSenha { get; set; }
}

public class UsuarioTokenModel
{
    public string Id { get; set; }
    public string Email { get; set; }
    public IEnumerable<ClaimModel> Claims { get; set; }
}

public class UsuarioRepostaModel
{
    public string AccessToken { get; set; }
    public double ExpiraEm { get; set; }
    public UsuarioTokenModel UsuarioToken { get; set; }
}

public class ClaimModel
{
    public string Value { get; set; }
    public string Type { get; set; }
}
