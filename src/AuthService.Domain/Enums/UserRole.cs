namespace AuthService.Domain.Enums;

public enum UserRole
{
    /// <summary>
    /// Administrador con acceso total al sistema bancario.
    /// </summary>
    ADMIN = 1,

    /// <summary>
    /// Cliente estándar del banco.
    /// </summary>
    USER = 2
}