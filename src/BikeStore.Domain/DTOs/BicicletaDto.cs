using System.ComponentModel.DataAnnotations;

namespace BikeStore.Domain.DTOs;

/// <summary>Datos de entrada para crear o actualizar una bicicleta.</summary>
public class BicicletaDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría.")]
    public int IdCategoria { get; set; }

    [Required, StringLength(60)]
    public string Marca { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Modelo { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Descripcion { get; set; }

    [Range(0.01, 999999999, ErrorMessage = "El precio debe ser mayor a cero.")]
    public decimal Precio { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [Range(0, int.MaxValue)]
    public int StockMinimo { get; set; } = 5;

    public bool Estado { get; set; } = true;
}
