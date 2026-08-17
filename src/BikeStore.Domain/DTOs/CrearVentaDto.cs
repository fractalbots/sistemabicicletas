using System.ComponentModel.DataAnnotations;

namespace BikeStore.Domain.DTOs;

/// <summary>Petición de registro de venta que consume sp_RegistrarVenta.</summary>
public class CrearVentaDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un cliente válido.")]
    public int IdCliente { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "La venta debe contener al menos un producto.")]
    public List<DetalleVentaItemDto> Detalle { get; set; } = new();
}
