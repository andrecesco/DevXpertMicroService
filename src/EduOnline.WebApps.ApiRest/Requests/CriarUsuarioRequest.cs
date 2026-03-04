using System.ComponentModel.DataAnnotations;

namespace EduOnline.Bff.ApiRest.Requests;

public class CriarUsuarioRequest
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
