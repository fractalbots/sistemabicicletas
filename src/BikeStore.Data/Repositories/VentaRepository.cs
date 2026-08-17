using System.Data;
using BikeStore.Domain.DTOs;
using BikeStore.Domain.Entities;
using BikeStore.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BikeStore.Data.Repositories;

/// <summary>
/// Repositorio de ventas. Las operaciones críticas se delegan a procedimientos
/// almacenados para que la atomicidad, el bloqueo de inventario y el cálculo de
/// montos ocurran dentro del motor de base de datos.
/// </summary>
public class VentaRepository : Repository<Venta>, IVentaRepository
{
    public VentaRepository(BikeStoreContext context) : base(context) { }

    /// <summary>
    /// Invoca sp_RegistrarVenta enviando el detalle como Table-Valued Parameter
    /// del tipo TipoDetalleVenta. Un solo viaje al servidor para toda la venta.
    /// </summary>
    public async Task<int> RegistrarVentaAsync(CrearVentaDto venta)
    {
        var tabla = new DataTable();
        tabla.Columns.Add("IdBicicleta", typeof(int));
        tabla.Columns.Add("Cantidad", typeof(int));

        foreach (var item in venta.Detalle)
            tabla.Rows.Add(item.IdBicicleta, item.Cantidad);

        var pCliente = new SqlParameter("@IdCliente", SqlDbType.Int) { Value = venta.IdCliente };

        var pDetalle = new SqlParameter("@Detalle", SqlDbType.Structured)
        {
            TypeName = "dbo.TipoDetalleVenta",
            Value = tabla
        };

        var pIdVenta = new SqlParameter("@IdVenta", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_RegistrarVenta @IdCliente, @Detalle, @IdVenta OUTPUT",
            pCliente, pDetalle, pIdVenta);

        return (int)pIdVenta.Value;
    }

    /// <summary>Invoca sp_AnularVenta, que revierte la venta y repone el stock.</summary>
    public async Task AnularVentaAsync(int idVenta)
    {
        var p = new SqlParameter("@IdVenta", SqlDbType.Int) { Value = idVenta };
        await _context.Database.ExecuteSqlRawAsync("EXEC sp_AnularVenta @IdVenta", p);
    }

    public async Task<IEnumerable<InventarioBicicleta>> ObtenerStockBajoAsync()
        => await _context.InventarioBicicletas
                         .FromSqlRaw("EXEC sp_BicicletasStockBajo")
                         .ToListAsync();

    public async Task<IEnumerable<HistorialVenta>> ObtenerVentasPorClienteAsync(int idCliente)
    {
        var p = new SqlParameter("@IdCliente", SqlDbType.Int) { Value = idCliente };

        return await _context.HistorialVentas
                             .FromSqlRaw("EXEC sp_VentasPorCliente @IdCliente", p)
                             .ToListAsync();
    }

    public async Task<Venta?> ObtenerConDetalleAsync(int idVenta)
        => await _context.Ventas
                         .AsNoTracking()
                         .Include(v => v.Cliente)
                         .Include(v => v.Detalles)
                             .ThenInclude(d => d.Bicicleta)
                         .FirstOrDefaultAsync(v => v.IdVenta == idVenta);

    public override async Task<IEnumerable<Venta>> ObtenerTodosAsync()
        => await _context.Ventas
                         .AsNoTracking()
                         .Include(v => v.Cliente)
                         .OrderByDescending(v => v.Fecha)
                         .ToListAsync();
}
