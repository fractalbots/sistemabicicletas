using BikeStore.Domain.DTOs;
using BikeStore.Domain.Entities;
using BikeStore.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.Web.Controllers;

/// <summary>Gestión de clientes contra los endpoints /api/Clientes.</summary>
public class ClientesController : Controller
{
    private readonly ServicioApi _api;

    public ClientesController(ServicioApi api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var respuesta = await _api.ObtenerClientesAsync();

        if (!respuesta.Exito)
        {
            TempData["Error"] = respuesta.Mensaje;
            return View(new List<Cliente>());
        }

        return View(respuesta.Datos);
    }

    public IActionResult Create() => View(new ClienteDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClienteDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var respuesta = await _api.CrearClienteAsync(dto);

        if (!respuesta.Exito)
        {
            ModelState.AddModelError(string.Empty, respuesta.Mensaje!);
            return View(dto);
        }

        TempData["Exito"] = $"Cliente {dto.Nombres} {dto.Apellidos} registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var respuesta = await _api.ObtenerClienteAsync(id);

        if (!respuesta.Exito)
        {
            TempData["Error"] = respuesta.Mensaje;
            return RedirectToAction(nameof(Index));
        }

        var c = respuesta.Datos!;
        ViewBag.Id = id;

        return View(new ClienteDto
        {
            Cedula = c.Cedula,
            Nombres = c.Nombres,
            Apellidos = c.Apellidos,
            Telefono = c.Telefono,
            Correo = c.Correo,
            Direccion = c.Direccion,
            Activo = c.Activo
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClienteDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Id = id;
            return View(dto);
        }

        var respuesta = await _api.ActualizarClienteAsync(id, dto);

        if (!respuesta.Exito)
        {
            ModelState.AddModelError(string.Empty, respuesta.Mensaje!);
            ViewBag.Id = id;
            return View(dto);
        }

        TempData["Exito"] = "Cliente actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Historial de compras (procedimiento sp_VentasPorCliente vía API).</summary>
    public async Task<IActionResult> Historial(int id)
    {
        var cliente = await _api.ObtenerClienteAsync(id);

        if (!cliente.Exito)
        {
            TempData["Error"] = cliente.Mensaje;
            return RedirectToAction(nameof(Index));
        }

        var historial = await _api.ObtenerHistorialClienteAsync(id);

        ViewBag.Cliente = cliente.Datos;
        return View(historial.Datos ?? new List<HistorialVenta>());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(int id)
    {
        var respuesta = await _api.DesactivarClienteAsync(id);

        if (respuesta.Exito)
            TempData["Exito"] = "Cliente desactivado.";
        else
            TempData["Error"] = respuesta.Mensaje;

        return RedirectToAction(nameof(Index));
    }
}
