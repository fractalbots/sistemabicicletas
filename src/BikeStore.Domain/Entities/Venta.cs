namespace BikeStore.Domain.Entities;

/// <summary>
/// Cabecera de la factura. Los montos los calcula el procedimiento almacenado
/// sp_RegistrarVenta; la aplicación nunca los escribe directamente.
/// </summary>
public class Venta
{
    public int IdVenta { get; set; }
    public int IdCliente { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Subtotal { get; set; }
    public decimal PorcentajeIva { get; set; } = 15.00m;
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "EMITIDA";

    public Cliente? Cliente { get; set; }
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}
