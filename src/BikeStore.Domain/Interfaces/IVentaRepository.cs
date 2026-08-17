using BikeStore.Domain.DTOs;
using BikeStore.Domain.Entities;

namespace BikeStore.Domain.Interfaces;

/// <summary>
/// Operaciones de venta resueltas mediante procedimientos almacenados.
/// La transaccionalidad y el descuento de stock viven en SQL Server.
/// </summary>
public interface IVentaRepository : IRepository<Venta>
{
    /// <summary>Ejecuta sp_RegistrarVenta y devuelve el Id generado.</summary>
    Task<int> RegistrarVentaAsync(CrearVentaDto venta);

    /// <summary>Ejecuta sp_AnularVenta y devuelve el stock al inventario.</summary>
    Task AnularVentaAsync(int idVenta);

    /// <summary>Ejecuta sp_BicicletasStockBajo.</summary>
    Task<IEnumerable<InventarioBicicleta>> ObtenerStockBajoAsync();

    /// <summary>Ejecuta sp_VentasPorCliente.</summary>
    Task<IEnumerable<HistorialVenta>> ObtenerVentasPorClienteAsync(int idCliente);

    /// <summary>Venta con cliente y detalle cargados, para mostrar la factura.</summary>
    Task<Venta?> ObtenerConDetalleAsync(int idVenta);
}
