namespace BikeStore.Domain.Entities;

/// <summary>Línea de la factura. Subtotal es una columna calculada persistida en SQL Server.</summary>
public class DetalleVenta
{
    public int IdDetalle { get; set; }
    public int IdVenta { get; set; }
    public int IdBicicleta { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public Venta? Venta { get; set; }
    public Bicicleta? Bicicleta { get; set; }
}
