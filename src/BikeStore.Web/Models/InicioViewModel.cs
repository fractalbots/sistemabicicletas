using BikeStore.Domain.Entities;

namespace BikeStore.Web.Models;

/// <summary>Datos del panel principal.</summary>
public class InicioViewModel
{
    public int TotalBicicletas { get; set; }
    public int TotalClientes { get; set; }
    public int TotalVentas { get; set; }
    public decimal MontoVendido { get; set; }
    public List<InventarioBicicleta> StockBajo { get; set; } = new();
    public bool ApiDisponible { get; set; }
    public string? MensajeError { get; set; }
}
