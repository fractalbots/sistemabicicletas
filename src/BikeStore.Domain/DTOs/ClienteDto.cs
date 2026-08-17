using System.ComponentModel.DataAnnotations;

namespace BikeStore.Domain.DTOs;

/// <summary>Datos de entrada para crear o actualizar un cliente.</summary>
public class ClienteDto
{
    [Required]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "La cédula debe tener exactamente 10 dígitos.")]
    public string Cedula { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Nombres { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Apellidos { get; set; } = string.Empty;

    [StringLength(15)]
    public string? Telefono { get; set; }

    [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
    [StringLength(120)]
    public string? Correo { get; set; }

    [StringLength(200)]
    public string? Direccion { get; set; }

    public bool Activo { get; set; } = true;
}
