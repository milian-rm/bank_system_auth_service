using AuthService.Application.Interfaces;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AuthService.Application.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(25, ErrorMessage = "El nombre no puede exceder los 25 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [MaxLength(25, ErrorMessage = "El apellido no puede exceder los 25 caracteres.")]
    public string Surname { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El nombre de usuario no puede exceder los 50 caracteres.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DPI es obligatorio.")]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "El DPI debe tener exactamente 13 dígitos.")]
    public string Dpi { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección de habitación es obligatoria.")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
    [MaxLength(15, ErrorMessage = "El teléfono no puede exceder los 15 caracteres.")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del trabajo es obligatorio.")]
    public string JobTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los ingresos mensuales son obligatorios.")]
    [Range(100.00, double.MaxValue, ErrorMessage = "Los ingresos mensuales deben ser de al menos Q100.00.")]
    public decimal MonthlyIncome { get; set; }

    public IFileData? ProfilePicture { get; set; }
}