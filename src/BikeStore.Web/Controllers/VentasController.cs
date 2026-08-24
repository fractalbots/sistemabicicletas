using System.Text.Json;
using BikeStore.Domain.DTOs;
using BikeStore.Domain.Entities;
using BikeStore.Web.Models;
using BikeStore.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.Web.Controllers;

/// <summary>Registro, consulta y anulación de ventas a través de la API REST.</summary>
public class VentasController : Controller
{
    private readonly ServicioApi _api;

    public VentasController(ServicioApi api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var respuesta = await _api.ObtenerVentasAsync();

        if (!respuesta.Exito)
        {
            TempData["Error"] = respuesta.Mensaje;
            return View(new List<Venta>());
        }

        return View(respuesta.Datos);
    }

    public async Task<IActionResult> Details(int id)
    {
        var respuesta = await _api.ObtenerVentaAsync(id);

        if (!respuesta.Exito)
        {
            TempData["Error"] = respuesta.Mensaje;
            return RedirectToAction(nameof(Index));
        }

        return View(respuesta.Datos);
    }

    public async Task<IActionResult> Create()
        => View(await ArmarFormularioAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearVentaViewModel modelo)
    {
        List<DetalleVentaItemDto>? detalle = null;

        try
        {
            detalle = JsonSerializer.Deserialize<List<DetalleVentaItemDto>>(
                modelo.DetalleJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            ModelState.AddModelError(string.Empty, "El detalle de la venta no se pudo interpretar.");
        }

        if (modelo.IdCliente <= 0)
            ModelState.AddModelError(nameof(modelo.IdCliente), "Debe seleccionar un cliente.");

        if (detalle is null || detalle.Count == 0)
            ModelState.AddModelError(string.Empty, "Agregue al menos una bicicleta a la venta.");

        if (!ModelState.IsValid)
            return View(await ArmarFormularioAsync(modelo));

        var respuesta = await _api.RegistrarVentaAsync(new CrearVentaDto
        {
            IdCliente = modelo.IdCliente,
            Detalle = detalle!
        });

        if (!respuesta.Exito)
        {
            ModelState.AddModelError(string.Empty, respuesta.Mensaje!);
            return View(await ArmarFormularioAsync(modelo));
        }

        TempData["Exito"] = $"Venta #{respuesta.Datos!.IdVenta} registrada por "
                          + $"${respuesta.Datos.Total:N2}.";

        return RedirectToAction(nameof(Details), new { id = respuesta.Datos.IdVenta });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular(int id)
    {
        var respuesta = await _api.AnularVentaAsync(id);

        if (respuesta.Exito)
            TempData["Exito"] = $"Venta #{id} anulada. El stock fue restituido.";
        else
            TempData["Error"] = respuesta.Mensaje;

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Carga los combos de clientes y bicicletas disponibles.</summary>
    private async Task<CrearVentaViewModel> ArmarFormularioAsync(CrearVentaViewModel? previo = null)
    {
        var modelo = previo ?? new CrearVentaViewModel();

        var clientes = await _api.ObtenerClientesAsync();
        var inventario = await _api.ObtenerInventarioAsync();

        if (!clientes.Exito) TempData["Error"] = clientes.Mensaje;
        if (!inventario.Exito) TempData["Error"] = inventario.Mensaje;

        modelo.Clientes = (clientes.Datos ?? new()).Where(c => c.Activo).ToList();

        modelo.Bicicletas = (inventario.Datos ?? new())
            .Where(b => b.Estado && b.Stock > 0)
            .ToList();

        return modelo;
    }
}
