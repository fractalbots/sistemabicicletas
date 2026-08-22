using BikeStore.Domain.DTOs;
using BikeStore.Web.Models;
using BikeStore.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.Web.Controllers;

/// <summary>
/// Registro y consulta de ventas. El calculo definitivo de subtotal, IVA y
/// total lo realiza el servidor mediante sp_RegistrarVenta; lo que se muestra
/// en pantalla antes de confirmar es solo una estimacion.
/// </summary>
public class VentasController : Controller
{
    private readonly BikeStoreApiClient _api;

    public VentasController(BikeStoreApiClient api) => _api = api;

    /// <summary>Historial de ventas.</summary>
    public async Task<IActionResult> Index()
    {
        var ventas = await _api.ObtenerVentasAsync();
        return View(ventas.OrderByDescending(v => v.IdVenta).ToList());
    }

    /// <summary>Factura completa con su detalle.</summary>
    public async Task<IActionResult> Details(int id)
    {
        var venta = await _api.ObtenerVentaAsync(id);
        if (venta is null) return NotFound();
        return View(venta);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var modelo = new NuevaVentaViewModel
        {
            Clientes = (await _api.ObtenerClientesAsync()).Where(c => c.Activo).ToList(),
            Bicicletas = (await _api.ObtenerInventarioAsync())
                            .Where(b => b.Estado && b.Stock > 0).ToList()
        };
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NuevaVentaViewModel modelo)
    {
        // Las lineas vacias que puede dejar el formulario se descartan antes de validar.
        modelo.Detalle = modelo.Detalle
            .Where(d => d.IdBicicleta > 0 && d.Cantidad > 0)
            .ToList();

        if (modelo.Detalle.Count == 0)
            ModelState.AddModelError(string.Empty, "Agregue al menos una bicicleta antes de registrar la venta.");

        if (!ModelState.IsValid)
            return View(await RecargarListasAsync(modelo));

        var peticion = new CrearVentaDto
        {
            IdCliente = modelo.IdCliente,
            Detalle = modelo.Detalle
        };

        var (idVenta, error) = await _api.RegistrarVentaAsync(peticion);

        if (idVenta is null)
        {
            ModelState.AddModelError(string.Empty, error ?? "No se pudo registrar la venta.");
            return View(await RecargarListasAsync(modelo));
        }

        TempData["Exito"] = $"Venta #{idVenta} registrada correctamente.";
        return RedirectToAction(nameof(Details), new { id = idVenta.Value });
    }

    /// <summary>Anula la venta y devuelve las unidades al inventario.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular(int id)
    {
        var (exito, error) = await _api.AnularVentaAsync(id);
        if (exito) TempData["Exito"] = $"Venta #{id} anulada. El stock fue restituido.";
        else       TempData["Error"] = error;

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<NuevaVentaViewModel> RecargarListasAsync(NuevaVentaViewModel modelo)
    {
        modelo.Clientes = (await _api.ObtenerClientesAsync()).Where(c => c.Activo).ToList();
        modelo.Bicicletas = (await _api.ObtenerInventarioAsync())
                                .Where(b => b.Estado && b.Stock > 0).ToList();
        return modelo;
    }
}
