using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AuthService.Persistence.Data;

public class ApplicationDbContext : DbContext
{
    // MÉTODO CONSTRUCTOR
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // REPRESENTACIÓN DE TABLAS EN EL MODELO
    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserEmail> UserEmails { get; set; }
    public DbSet<UserPasswordReset> UserPasswordResets { get; set; }

    // CONVIERTE CAMEL CASE A SNAKE CASE Y CONFIGURA ENTIDADES
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar Snake Case a todas las tablas y columnas
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (!string.IsNullOrEmpty(tableName))
            {
                entity.SetTableName(ToSnakeCase(tableName));
            }
            foreach (var property in entity.GetProperties())
            {
                var columnName = property.GetColumnName();
                if (!string.IsNullOrEmpty(columnName))
                {
                    property.SetColumnName(ToSnakeCase(columnName));
                }
            }
        }

        // ------------------------------------------------------------
        // CONFIGURACIÓN ESPECÍFICA DE LA ENTIDAD USER
        // ------------------------------------------------------------
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Indices únicos bancarios
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Dpi).IsUnique(); // Indice único para el DPI

            entity.Property(e => e.Dpi).IsRequired().HasMaxLength(13);

            // Relación de 1:1 con UserProfile
            entity.HasOne(e => e.UserProfile)
                .WithOne(p => p.User)
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación 1:N con UserRoles
            entity.HasMany(e => e.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación 1:1 con UserEmail
            entity.HasOne(e => e.UserEmail)
                .WithOne(ue => ue.User)
                .HasForeignKey<UserEmail>(ue => ue.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación 1:1 con UserPasswordReset
            entity.HasOne(e => e.UserPasswordReset)
                .WithOne(upr => upr.User)
                .HasForeignKey<UserPasswordReset>(upr => upr.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ------------------------------------------------------------
        // CONFIGURACIÓN ESPECÍFICA DE LA ENTIDAD USERPROFILE (Datos Bancarios)
        // ------------------------------------------------------------
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Configuración de precisión para moneda (Ingresos Mensuales)
            entity.Property(e => e.MonthlyIncome)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.Address).IsRequired().HasMaxLength(250);
            entity.Property(e => e.JobTitle).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(15);
        });

        // CONFIGURACIÓN ESPECÍFICA DE LA ENTIDAD USERROLE
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
        });

        // CONFIGURACIÓN ESPECÍFICA DE LA ENTIDAD ROLE
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
        });
    }

    // FUNCIÓN PARA CONFIGURAR EL NOMBRE DE CLASE A NOMBRE DE DB
    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return string.Concat(
            input.Select((x, i) => i > 0 && char.IsUpper(x) 
                ? "_" + x.ToString().ToLower() 
                : x.ToString().ToLower())
        );
    }
}