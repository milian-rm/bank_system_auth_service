using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs.Email;

public class VerifyEmailDto
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El token de verificación es obligatorio.")]
    public string Token { get; set; } = string.Empty;
}