using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Application.Exceptions;
using AuthService.Application.Validators;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AuthService.Application.DTOs.Email;
using AuthService.Application.Extensions;
namespace AuthService.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHashService passwordHashService,
    IJwtTokenService jwtTokenService,
    ICloudinaryService cloudinaryService,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<AuthService> logger) : IAuthService
{

    private readonly ICloudinaryService _cloudinaryService = cloudinaryService;

    public async Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // Verificar si el email ya existe
        if (await userRepository.ExistsByEmailAsync(registerDto.Email))
        {
            logger.LogRegistrationWithExistingEmail();
            throw new BusinessException(ErrorCodes.EMAIL_ALREADY_EXISTS, "Email already exists");
        }

        // Verificar si el username ya existe
        if (await userRepository.ExistsByUsernameAsync(registerDto.Username))
        {
            logger.LogRegistrationWithExistingUsername();
            throw new BusinessException(ErrorCodes.USERNAME_ALREADY_EXISTS, "Username already exists");
        }

        // Validar y manejar la imagen de perfil
        string profilePicturePath;

        if (registerDto.ProfilePicture != null && registerDto.ProfilePicture.Size > 0)
        {
            var (isValid, errorMessage) = FileValidator.ValidateImage(registerDto.ProfilePicture);
            if (!isValid)
            {
                logger.LogWarning($"File validation failed: {errorMessage}");
                throw new BusinessException(ErrorCodes.INVALID_FILE_FORMAT, errorMessage!);
            }

            try
            {
                var fileName = FileValidator.GenerateSecureFileName(registerDto.ProfilePicture.FileName);
                profilePicturePath = await _cloudinaryService.UploadImageAsync(registerDto.ProfilePicture, fileName);
            }
            catch (Exception)
            {
                logger.LogImageUploadError();
                throw new BusinessException(ErrorCodes.IMAGE_UPLOAD_FAILED, "Failed to upload profile image");
            }
        }
        else
        {
            profilePicturePath = _cloudinaryService.GetDefaultAvatarUrl();
        }

        // Crear nuevo usuario y entidades relacionadas
        var emailVerificationToken = TokenGenerator.GenerateEmailVerificationToken();

        var userId = UuidGenerator.GenerateUserId();
        var userProfileId = UuidGenerator.GenerateUserId();
        var userEmailId = UuidGenerator.GenerateUserId();
        var userRoleId = UuidGenerator.GenerateUserId();

        // Obtener el rol por defecto (USER_ROLE) ya seedado en DB
        var defaultRole = await roleRepository.GetByNameAsync(RoleConstants.USER_ROLE);
        if (defaultRole == null)
        {
            throw new InvalidOperationException($"Default role '{RoleConstants.USER_ROLE}' not found. Ensure seeding runs before registration.");
        }

        var user = new User
        {
            Id = userId,
            Name = registerDto.Name,
            Surname = registerDto.Surname,
            Username = registerDto.Username,
            Email = registerDto.Email.ToLowerInvariant(),
            Dpi = registerDto.Dpi, // Campo OBLIGATORIO agregado para el banco
            Password = passwordHashService.HashPassword(registerDto.Password),
            Status = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserProfile = new UserProfile
            {
                Id = userProfileId,
                UserId = userId,
                ProfilePictureUrl = profilePicturePath,
                Phone = registerDto.Phone,
                Address = registerDto.Address,       // Campo bancario agregado
                JobTitle = registerDto.JobTitle,     // Campo bancario agregado
                MonthlyIncome = registerDto.MonthlyIncome // Campo bancario agregado
            },
            UserEmail = new UserEmail
            {
                Id = userEmailId,
                UserId = userId,
                EmailVerified = false,
                EmailVerificationToken = emailVerificationToken,
                EmailVerificationTokenExpiration = DateTime.UtcNow.AddHours(24)
            },
            UserRoles =
            [
                new Domain.Entities.UserRole
                {
                    Id = userRoleId,
                    UserId = userId,
                    RoleId = defaultRole.Id
                }
            ]
        };

        // Guardar usuario y entidades relacionadas
        var createdUser = await userRepository.CreateAsync(user);

        logger.LogUserRegistered(createdUser.Username);

        // Enviar email de verificación en background
        _ = Task.Run(async () =>
        {
            try
            {
                await emailService.SendEmailVerificationAsync(createdUser.Email, createdUser.Username, emailVerificationToken);
                logger.LogInformation("Verification email sent");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send verification email");
            }
        });

        // Crear respuesta sin JWT - solo confirmación de registro
        return new RegisterResponseDto
        {
            Success = true,
            UserId = createdUser.Id,
            Message = "Usuario registrado exitosamente. Por favor, verifica tu email para activar la cuenta."
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        // Buscar usuario por email (En este sistema bancario usamos email)
        var user = await userRepository.GetByEmailAsync(loginDto.Email.ToLowerInvariant());

        // Verificar si el usuario existe
        if (user == null)
        {
            logger.LogFailedLoginAttempt();
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        // Verificar si el usuario está activo
        if (!user.Status)
        {
            logger.LogFailedLoginAttempt();
            throw new UnauthorizedAccessException("User account is disabled");
        }

        // Verificar contraseña
        if (!passwordHashService.VerifyPassword(loginDto.Password, user.Password))
        {
            logger.LogFailedLoginAttempt();
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        logger.LogUserLoggedIn();

        // Generar token JWT
        var token = jwtTokenService.GenerateToken(user);

        // Crear respuesta
        return new AuthResponseDto
        {
            Success = true,
            Message = "Login exitoso",
            Token = token,
            Data = MapToUserDataDto(user)
        };
    }

    public async Task<EmailResponseDto> VerifyEmailAsync(VerifyEmailDto verifyEmailDto)
    {
        var user = await userRepository.GetByEmailVerificationTokenAsync(verifyEmailDto.Token);
        if (user == null || user.UserEmail == null)
        {
            return new EmailResponseDto
            {
                Success = false,
                Message = "Invalid or expired verification token"
            };
        }

        user.UserEmail.EmailVerified = true;
        user.Status = true;
        user.UserEmail.EmailVerificationToken = null;
        user.UserEmail.EmailVerificationTokenExpiration = null;

        await userRepository.UpdateAsync(user);

        // Enviar email de bienvenida
        try
        {
            await emailService.SendWelcomeEmailAsync(user.Email, user.Username);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
        }

        logger.LogInformation("Email verified successfully for user {Username}", user.Username);

        return new EmailResponseDto
        {
            Success = true,
            Message = "Email verificado exitosamente"
        };
    }

    public async Task<EmailResponseDto> ResendVerificationEmailAsync(ResendVerificationDto resendDto)
    {
        var user = await userRepository.GetByEmailAsync(resendDto.Email);
        if (user == null || user.UserEmail == null)
        {
            return new EmailResponseDto
            {
                Success = false,
                Message = "Usuario no encontrado"
            };
        }

        if (user.UserEmail.EmailVerified)
        {
            return new EmailResponseDto
            {
                Success = false,
                Message = "El email ya ha sido verificado"
            };
        }

        var newToken = TokenGenerator.GenerateEmailVerificationToken();
        user.UserEmail.EmailVerificationToken = newToken;
        user.UserEmail.EmailVerificationTokenExpiration = DateTime.UtcNow.AddHours(24);

        await userRepository.UpdateAsync(user);

        try
        {
            await emailService.SendEmailVerificationAsync(user.Email, user.Username, newToken);
            return new EmailResponseDto
            {
                Success = true,
                Message = "Email de verificación enviado exitosamente"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to resend verification email");
            return new EmailResponseDto { Success = false, Message = "Error al enviar el email" };
        }
    }

    public async Task<EmailResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        var user = await userRepository.GetByEmailAsync(forgotPasswordDto.Email);
        if (user == null)
        {
            return new EmailResponseDto
            {
                Success = true,
                Message = "Si el email existe, se ha enviado un enlace de recuperación"
            };
        }

        var resetToken = TokenGenerator.GeneratePasswordResetToken();

        if (user.UserPasswordReset == null)
        {
            user.UserPasswordReset = new UserPasswordReset
            {
                Id = UuidGenerator.GenerateUserId(),
                UserId = user.Id,
                PasswordResetToken = resetToken,
                PasswordResetTokenExpiration = DateTime.UtcNow.AddHours(1)
            };
        }
        else
        {
            user.UserPasswordReset.PasswordResetToken = resetToken;
            user.UserPasswordReset.PasswordResetTokenExpiration = DateTime.UtcNow.AddHours(1);
        }

        await userRepository.UpdateAsync(user);

        try
        {
            await emailService.SendPasswordResetAsync(user.Email, user.Username, resetToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send password reset email");
        }

        return new EmailResponseDto
        {
            Success = true,
            Message = "Si el email existe, se ha enviado un enlace de recuperación"
        };
    }

    public async Task<EmailResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var user = await userRepository.GetByPasswordResetTokenAsync(resetPasswordDto.Token);
        if (user == null || user.UserPasswordReset == null)
        {
            return new EmailResponseDto
            {
                Success = false,
                Message = "Token de reset inválido o expirado"
            };
        }

        user.Password = passwordHashService.HashPassword(resetPasswordDto.NewPassword);
        user.UserPasswordReset.PasswordResetToken = string.Empty;
        user.UserPasswordReset.PasswordResetTokenExpiration = DateTime.UtcNow.AddDays(-1);

        await userRepository.UpdateAsync(user);

        return new EmailResponseDto
        {
            Success = true,
            Message = "Contraseña actualizada exitosamente"
        };
    }

    public async Task<UserDetailDto?> GetUserByIdAsync(string userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        return MapToUserDetailDto(user);
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await userRepository.GetAllAsync();
        return users.Select(MapToUserResponseDto);
    }

    // --- Métodos de Mapeo Privados (Estructura Original) ---

    private UserDataDto MapToUserDataDto(User user)
    {
        return new UserDataDto
        {
            Uid = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.UserRoles.FirstOrDefault()?.Role?.Name ?? RoleConstants.USER_ROLE
        };
    }

    private UserResponseDto MapToUserResponseDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Status = user.Status,
            Role = user.UserRoles.FirstOrDefault()?.Role?.Name ?? RoleConstants.USER_ROLE
        };
    }

    private UserDetailDto MapToUserDetailDto(User user)
    {
        return new UserDetailDto
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Username = user.Username,
            Email = user.Email,
            Dpi = user.Dpi,
            Phone = user.UserProfile?.Phone ?? string.Empty,
            Address = user.UserProfile?.Address ?? string.Empty,
            JobTitle = user.UserProfile?.JobTitle ?? string.Empty,
            MonthlyIncome = user.UserProfile?.MonthlyIncome ?? 0,
            Status = user.Status,
            Role = user.UserRoles.FirstOrDefault()?.Role?.Name ?? RoleConstants.USER_ROLE,
            CreatedAt = user.CreatedAt
        };
    }
}