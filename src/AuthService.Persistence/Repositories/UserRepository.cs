using AuthService.Domain.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Persistence.Repositories;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<User> GetByIdAsync(string id)
    {
        var user = await context.Users
            .Include(u => u.UserProfile)
            .Include(u => u.UserEmail)
            .Include(u => u.UserPasswordReset)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        return user ?? throw new InvalidOperationException($"User with id {id} not found");
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await context.Users
            .Include(u => u.UserProfile)
            .Include(u => u.UserEmail)
            .Include(u => u.UserPasswordReset)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.Email, email));
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await context.Users
            .Include(u => u.UserProfile)
            .Include(u => u.UserEmail)
            .Include(u => u.UserPasswordReset)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.Username, username));
    }

    public async Task<User?> GetByEmailVerificationTokenAsync(string token)
    {
        return await context.Users
            .Include(u => u.UserEmail)
            .FirstOrDefaultAsync(u => u.UserEmail != null &&
                                u.UserEmail.EmailVerificationToken == token);
    }

    public async Task<User?> GetByPasswordResetTokenAsync(string token)
    {
        return await context.Users
            .Include(u => u.UserPasswordReset)
            .FirstOrDefaultAsync(u => u.UserPasswordReset != null &&
                                u.UserPasswordReset.PasswordResetToken == token);
    }

    public async Task<User> CreateAsync(User user)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return await GetByIdAsync(user.Id);
    }

    public async Task<User> UpdateAsync(User user)
    {
        context.Entry(user).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return await GetByIdAsync(user.Id);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var user = await GetByIdAsync(id);
        context.Users.Remove(user);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await context.Users.AnyAsync(u => EF.Functions.ILike(u.Email, email));
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await context.Users.AnyAsync(u => EF.Functions.ILike(u.Username, username));
    }

    public async Task UpdateUserRoleAsync(string userId, string roleId)
    {
        var existingRoles = await context.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync();

        context.UserRoles.RemoveRange(existingRoles);
        
        var newUserRole = new UserRole
        {
            Id = Guid.NewGuid().ToString()[..16], // Usamos Guid directo para evitar el error de UuidGenerator
            UserId = userId,
            RoleId = roleId
        };

        context.UserRoles.Add(newUserRole);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await context.Users // Corregido de _context a context
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .ToListAsync();
    }
}