namespace BikeStore.Domain.Entities;

/// <summary>Proyección de solo lectura de la vista vw_HistorialVentas.</summary>
public class HistorialVenta
{
    public int IdVenta { get; set; }
    public DateTime Fecha { get; set; }
    public int IdCliente { get; set; }
    public string Cedula { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public int NumItems { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
}
