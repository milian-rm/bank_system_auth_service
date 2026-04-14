using AuthService.Domain.Entities;
using AuthService.Domain.Constants;
using AuthService.Application.Services; // Importante para UuidGenerator
using Microsoft.EntityFrameworkCore;

namespace AuthService.Persistence.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // 1. Verificar si ya existen roles
        if (!context.Roles.Any())
        {
            var roles = new List<Role>
            {
                new() { 
                    Id = UuidGenerator.GenerateRoleId(), 
                    Name = RoleConstants.ADMIN_ROLE 
                },
                new() { 
                    Id = UuidGenerator.GenerateRoleId(), 
                    Name = RoleConstants.USER_ROLE 
                }
            };
            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        // 2. Seed de un usuario administrador por defecto
        if (!await context.Users.AnyAsync())
        {
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleConstants.ADMIN_ROLE);
            
            if (adminRole != null)
            {
                var userId = UuidGenerator.GenerateUserId();

                var adminUser = new User
                {
                    Id = userId,
                    Name = "Admin",
                    Surname = "System",
                    Username = "admin",
                    Email = "admin@bank.local",
                    Dpi = "0000000000000",
                    Password = "12345678", // Recuerda hashear esto en el futuro
                    Status = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UserProfile = new UserProfile
                    {
                        Id = UuidGenerator.GenerateUserId(),
                        UserId = userId,
                        ProfilePictureUrl = "https://res.cloudinary.com/default-avatar.png",
                        Phone = "00000000",
                        Address = "Ciudad de Guatemala",
                        JobTitle = "System Administrator",
                        MonthlyIncome = 0.00m
                    },
                    UserEmail = new UserEmail
                    {
                        Id = UuidGenerator.GenerateUserId(),
                        UserId = userId,
                        EmailVerified = true,
                        EmailVerificationToken = null,
                        EmailVerificationTokenExpiration = null
                    },
                    UserRoles = new List<UserRole>
                    {
                        new UserRole
                        {
                            Id = UuidGenerator.GenerateUserId(),
                            UserId = userId,
                            RoleId = adminRole.Id
                            // Eliminamos CreatedAt y UpdatedAt de aquí para evitar errores
                        }
                    }
                };

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
            }
        }
    }
}