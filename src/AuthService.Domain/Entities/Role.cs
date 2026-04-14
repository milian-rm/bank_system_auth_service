using System.ComponentModel.DataAnnotations;

namespace AuthService.Domain.Entities;

public class Role
{
    [Key]
    [MaxLength(16)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Name { get; set; } = string.Empty; // ADMIN, USER

    public string? Description { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = []; 
}