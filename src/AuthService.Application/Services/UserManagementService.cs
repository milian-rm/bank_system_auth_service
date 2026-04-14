using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;

namespace AuthService.Application.Services;

public class UserManagementService : IUserManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICloudinaryService _cloudinaryService;

    public UserManagementService(
        IUserRepository userRepository, 
        IRoleRepository roleRepository, 
        ICloudinaryService cloudinaryService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<UserResponseDto> UpdateUserRoleAsync(string userId, string roleName)
    {
        // Normalizar
        roleName = roleName?.Trim().ToUpperInvariant() ?? string.Empty;

        // Validar entradas
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("Invalid userId", nameof(userId));
        
        if (!RoleConstants.AllowedRoles.Contains(roleName))
            throw new InvalidOperationException($"Role not allowed. Use {RoleConstants.ADMIN_ROLE} or {RoleConstants.USER_ROLE}");

        // Cargar al usuario con roles
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        // Prevenir eliminar al último administrador
        var isUserAdmin = user.UserRoles.Any(r => r.Role.Name == RoleConstants.ADMIN_ROLE);
        if (isUserAdmin && roleName != RoleConstants.ADMIN_ROLE)
        {
            var adminCount = await _roleRepository.CountUsersInRoleAsync(RoleConstants.ADMIN_ROLE);

            if (adminCount <= 1)
            {
                throw new InvalidOperationException("Cannot remove the last administrator");
            }
        }

        // Buscar entidad de rol
        var role = await _roleRepository.GetByNameAsync(roleName)
                       ?? throw new InvalidOperationException($"Role {roleName} not found");

        // Actualizar rol usando el repositorio
        await _userRepository.UpdateUserRoleAsync(userId, role.Id);

        // Recargar usuario para devolver datos actualizados
        user = await _userRepository.GetByIdAsync(userId);

        return MapToUserResponseDto(user!);
    }

    public async Task<IReadOnlyList<string>> GetUserRolesAsync(string userId)
    {
        var roleNames = await _roleRepository.GetUserRoleNamesAsync(userId);
        return roleNames;
    }

    public async Task<IReadOnlyList<UserResponseDto>> GetUsersByRoleAsync(string roleName)
    {
        roleName = roleName?.Trim().ToUpperInvariant() ?? string.Empty;
        var usersInRole = await _roleRepository.GetUsersByRoleAsync(roleName);
        
        return usersInRole.Select(MapToUserResponseDto).ToList().AsReadOnly();
    }

    // --- Métodos de Mapeo Privados (Estructura Original) ---

    private UserResponseDto MapToUserResponseDto(User user)
    {
        var userRole = user.UserRoles.FirstOrDefault()?.Role?.Name ?? RoleConstants.USER_ROLE;
        
        return new UserResponseDto
        {
            Id = user.Id,
            FullName = $"{user.Name} {user.Surname}",
            Username = user.Username,
            Email = user.Email,
            Role = userRole,
            Status = user.Status
        };
    }
}