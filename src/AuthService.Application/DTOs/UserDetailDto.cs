namespace AuthService.Application.DTOs;

public class UserDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Dpi { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public decimal MonthlyIncome { get; set; }
    public string? ProfilePicture { get; set; }
    public bool Status { get; set; }
    public bool IsVerified { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}