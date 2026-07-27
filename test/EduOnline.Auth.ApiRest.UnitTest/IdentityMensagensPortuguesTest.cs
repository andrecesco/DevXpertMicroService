using EduOnline.Auth.ApiRest.Extensions;

namespace EduOnline.Auth.ApiRest.UnitTest;

public class IdentityMensagensPortuguesTest
{
    [Fact(DisplayName = "IdentityMensagensPortugues deve retornar códigos e descrições esperadas")]
    public void DeveRetornarMensagensEsperadas()
    {
        var describer = new IdentityMensagensPortugues();

        Assert.Equal("DefaultError", describer.DefaultError().Code);
        Assert.Equal("Um erro desconhecido ocorreu.", describer.DefaultError().Description);

        Assert.Equal("ConcurrencyFailure", describer.ConcurrencyFailure().Code);
        Assert.Equal("Falha de concorrência otimista, o objeto foi modificado.", describer.ConcurrencyFailure().Description);

        Assert.Equal("PasswordMismatch", describer.PasswordMismatch().Code);
        Assert.Equal("InvalidToken", describer.InvalidToken().Code);
        Assert.Equal("LoginAlreadyAssociated", describer.LoginAlreadyAssociated().Code);

        Assert.Equal("Login 'user' é inválido, pode conter apenas letras ou dígitos.", describer.InvalidUserName("user").Description);
        Assert.Equal("Email 'email@teste.com' é inválido.", describer.InvalidEmail("email@teste.com").Description);
        Assert.Equal("Login 'user' já está sendo utilizado.", describer.DuplicateUserName("user").Description);
        Assert.Equal("Email 'email@teste.com' já está sendo utilizado.", describer.DuplicateEmail("email@teste.com").Description);
        Assert.Equal("A permissão 'Aluno' é inválida.", describer.InvalidRoleName("Aluno").Description);
        Assert.Equal("A permissão 'Aluno' já está sendo utilizada.", describer.DuplicateRoleName("Aluno").Description);

        Assert.Equal("UserAlreadyHasPassword", describer.UserAlreadyHasPassword().Code);
        Assert.Equal("UserLockoutNotEnabled", describer.UserLockoutNotEnabled().Code);
        Assert.Equal("Usuário já possui a permissão 'Aluno'.", describer.UserAlreadyInRole("Aluno").Description);
        Assert.Equal("Usuário não tem a permissão 'Aluno'.", describer.UserNotInRole("Aluno").Description);

        Assert.Equal("Senhas devem conter ao menos 8 caracteres.", describer.PasswordTooShort(8).Description);
        Assert.Equal("PasswordRequiresNonAlphanumeric", describer.PasswordRequiresNonAlphanumeric().Code);
        Assert.Equal("PasswordRequiresDigit", describer.PasswordRequiresDigit().Code);
        Assert.Equal("PasswordRequiresLower", describer.PasswordRequiresLower().Code);
        Assert.Equal("PasswordRequiresUpper", describer.PasswordRequiresUpper().Code);
    }
}
