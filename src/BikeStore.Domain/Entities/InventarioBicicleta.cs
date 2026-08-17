namespace BikeStore.Domain.Entities;

/// <summary>Proyección de solo lectura de la vista vw_InventarioBicicletas.</summary>
public class InventarioBicicleta
{
    public int IdBicicleta { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int IdCategoria { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public string EstadoStock { get; set; } = string.Empty;
    public bool Estado { get; set; }
}
