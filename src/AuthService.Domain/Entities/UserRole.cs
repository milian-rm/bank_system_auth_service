using System.ComponentModel.DataAnnotations;

namespace AuthService.Domain.Entities;

public class UserRole
{
    [Key]
    [MaxLength(16)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [MaxLength(16)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(16)]
    public string RoleId { get; set; } = string.Empty;

    // Navegación
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}