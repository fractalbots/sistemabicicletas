using System.ComponentModel.DataAnnotations;

namespace BikeStore.Domain.DTOs;

/// <summary>
/// Ítem que el cliente HTTP envía al registrar una venta.
/// Nótese que NO incluye precio: el precio lo resuelve el servidor.
/// </summary>
public class DetalleVentaItemDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una bicicleta válida.")]
    public int IdBicicleta { get; set; }

    [Required]
    [Range(1, 1000, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public int Cantidad { get; set; }
}
