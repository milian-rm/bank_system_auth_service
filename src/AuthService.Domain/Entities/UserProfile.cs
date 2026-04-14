using System.ComponentModel.DataAnnotations;

namespace AuthService.Domain.Entities;

public class UserProfile
{
    [Key]
    [MaxLength(16)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [MaxLength(16)]
    public string UserId { get; set; } = string.Empty;

    public string? ProfilePictureUrl { get; set; }

    [Required(ErrorMessage = "La dirección es requerida")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "El celular es requerido")]
    [MaxLength(15)]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de trabajo es requerido")]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    public decimal MonthlyIncome { get; set; }

    // Navegación
    public User User { get; set; } = null!;
}