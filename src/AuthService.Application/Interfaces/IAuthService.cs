using AuthService.Application.DTOs;
using AuthService.Application.DTOs.Email;

namespace AuthService.Application.Interfaces;

public interface IAuthService
{
    // Métodos de acceso y registro
    Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    
    // Perfil y Usuarios
    Task<UserDetailDto?> GetUserByIdAsync(string userId);
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(); // Para el GET /api/users del ADMIN
    
    // Gestión de Email
    Task<EmailResponseDto> VerifyEmailAsync(VerifyEmailDto verifyEmailDto);
    Task<EmailResponseDto> ResendVerificationEmailAsync(ResendVerificationDto resendDto);
    Task<EmailResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<EmailResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
}