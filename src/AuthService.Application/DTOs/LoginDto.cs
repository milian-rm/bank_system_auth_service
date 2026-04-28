using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = string.Empty;
}