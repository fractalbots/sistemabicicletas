namespace BikeStore.Domain.Entities;

/// <summary>Producto del inventario. El precio se administra únicamente desde el servidor.</summary>
public class Bicicleta
{
    public int IdBicicleta { get; set; }
    public int IdCategoria { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; } = 5;
    public bool Estado { get; set; } = true;
    public DateTime FechaRegistro { get; set; }

    public Categoria? Categoria { get; set; }
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}
