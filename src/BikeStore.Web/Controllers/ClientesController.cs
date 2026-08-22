using BikeStore.Domain.DTOs;
using BikeStore.Web.Models;
using BikeStore.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.Web.Controllers;

/// <summary>Gestion de clientes y consulta de su historial de compras.</summary>
public class ClientesController : Controller
{
    private readonly BikeStoreApiClient _api;

    public ClientesController(BikeStoreApiClient api) => _api = api;

    public async Task<IActionResult> Index(string? cedula)
    {
        if (!string.IsNullOrWhiteSpace(cedula))
        {
            var encontrado = await _api.BuscarClientePorCedulaAsync(cedula.Trim());
            ViewBag.Cedula = cedula;

            if (encontrado is null)
            {
                TempData["Error"] = $"No existe un cliente con cedula {cedula}.";
                return View(new List<BikeStore.Domain.Entities.Cliente>());
            }

            return View(new List<BikeStore.Domain.Entities.Cliente> { encontrado });
        }

        return View(await _api.ObtenerClientesAsync());
    }

    public async Task<IActionResult> Historial(int id)
    {
        var cliente = await _api.ObtenerClienteAsync(id);
        if (cliente is null) return NotFound();

        return View(new HistorialClienteViewModel
        {
            Cliente = cliente,
            Ventas = await _api.ObtenerHistorialClienteAsync(id)
        });
    }

    [HttpGet]
    public IActionResult Create() => View(new ClienteFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClienteFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var (exito, error) = await _api.CrearClienteAsync(modelo.Datos);
        if (!exito)
        {
            ModelState.AddModelError(string.Empty, error ?? "No se pudo registrar el cliente.");
            return View(modelo);
        }

        TempData["Exito"] = $"Cliente {modelo.Datos.Nombres} {modelo.Datos.Apellidos} registrado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var cliente = await _api.ObtenerClienteAsync(id);
        if (cliente is null) return NotFound();

        return View(new ClienteFormViewModel
        {
            IdCliente = cliente.IdCliente,
            Datos = new ClienteDto
            {
                Cedula = cliente.Cedula,
                Nombres = cliente.Nombres,
                Apellidos = cliente.Apellidos,
                Telefono = cliente.Telefono,
                Correo = cliente.Correo,
                Direccion = cliente.Direccion,
                Activo = cliente.Activo
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ClienteFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var (exito, error) = await _api.ActualizarClienteAsync(modelo.IdCliente, modelo.Datos);
        if (!exito)
        {
            ModelState.AddModelError(string.Empty, error ?? "No se pudo actualizar el cliente.");
            return View(modelo);
        }

        TempData["Exito"] = "Cliente actualizado.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Baja logica: el cliente deja de aparecer para nuevas ventas
    /// pero conserva sus compras anteriores.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(int id)
    {
        var (exito, error) = await _api.DesactivarClienteAsync(id);
        if (exito) TempData["Exito"] = "Cliente desactivado. Su historial se conserva.";
        else       TempData["Error"] = error;

        return RedirectToAction(nameof(Index));
    }
}
