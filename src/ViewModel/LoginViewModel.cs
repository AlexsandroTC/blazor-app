using System.ComponentModel.DataAnnotations;

namespace BlazorApp.ViewModel;

public class LoginViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor insira um email.")]
    public string? UserName { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor insira uma Senha.")]
    public string? Password { get; set; }
}