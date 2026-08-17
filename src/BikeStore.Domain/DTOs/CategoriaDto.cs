using System.ComponentModel.DataAnnotations;

namespace BikeStore.Domain.DTOs;

/// <summary>Datos de entrada para crear o actualizar una categoría.</summary>
public class CategoriaDto
{
    [Required, StringLength(60)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;
}
