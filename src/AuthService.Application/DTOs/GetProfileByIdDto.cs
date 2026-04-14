using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs;

public class GetProfileByIdDto
{
    [Required(ErrorMessage = "El ID de usuario es requerido para la consulta.")]
    public string UserId { get; set; } = string.Empty;
}