using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs;

public class UpdateUserRoleDto
{
    /// <summary>
    /// Rol único por usuario; acepta nombres de rol como "ADMIN" o "USER".
    /// </summary>
    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El nombre del rol no puede exceder los 20 caracteres.")]
    public string RoleName { get; set; } = string.Empty;
}