using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs.Email;

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Debe proporcionar su correo electrónico para recuperar la cuenta.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}