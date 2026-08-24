using System.Diagnostics;
using BikeStore.Web.Models;
using BikeStore.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.Web.Controllers;

/// <summary>Panel principal con indicadores del negocio.</summary>
public class HomeController : Controller
{
    private readonly ServicioApi _api;

    public HomeController(ServicioApi api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var modelo = new InicioViewModel();

        var inventario = await _api.ObtenerInventarioAsync();
        if (!inventario.Exito)
        {
            modelo.ApiDisponible = false;
            modelo.MensajeError = inventario.Mensaje;
            return View(modelo);
        }

        modelo.ApiDisponible = true;
        modelo.TotalBicicletas = inventario.Datos!.Count;

        var clientes = await _api.ObtenerClientesAsync();
        if (clientes.Exito) modelo.TotalClientes = clientes.Datos!.Count;

        var ventas = await _api.ObtenerVentasAsync();
        if (ventas.Exito)
        {
            var emitidas = ventas.Datos!.Where(v => v.Estado == "EMITIDA").ToList();
            modelo.TotalVentas = emitidas.Count;
            modelo.MontoVendido = emitidas.Sum(v => v.Total);
        }

        var stockBajo = await _api.ObtenerStockBajoAsync();
        if (stockBajo.Exito) modelo.StockBajo = stockBajo.Datos!;

        return View(modelo);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
}
