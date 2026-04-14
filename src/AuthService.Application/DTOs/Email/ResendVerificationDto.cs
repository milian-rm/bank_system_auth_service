using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs.Email;

public class ResendVerificationDto
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio para el reenvío.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}