using System.Diagnostics;
using BikeStore.Web.Models;
using BikeStore.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.Web.Controllers;

/// <summary>Panel de inicio con las cifras generales del sistema.</summary>
public class HomeController : Controller
{
    private readonly BikeStoreApiClient _api;

    public HomeController(BikeStoreApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var categorias = await _api.ObtenerCategoriasAsync();
        var inventario = await _api.ObtenerInventarioAsync();
        var clientes   = await _api.ObtenerClientesAsync();
        var ventas     = await _api.ObtenerVentasAsync();
        var stockBajo  = await _api.ObtenerStockBajoAsync();

        var modelo = new PanelViewModel
        {
            ApiDisponible   = categorias.Count > 0,
            TotalBicicletas = inventario.Count,
            TotalClientes   = clientes.Count,
            TotalVentas     = ventas.Count,
            PorReponer      = stockBajo.Count,
            VentasTotales   = ventas.Where(v => v.Estado == "EMITIDA").Sum(v => v.Total),
            StockCritico    = stockBajo.OrderBy(b => b.Stock).Take(5).ToList(),
            UltimasVentas   = ventas.OrderByDescending(v => v.Fecha).Take(5).ToList()
        };

        return View(modelo);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
