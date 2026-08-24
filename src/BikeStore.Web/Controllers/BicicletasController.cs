using BikeStore.Domain.DTOs;
using BikeStore.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.Web.Controllers;

/// <summary>Catálogo de bicicletas. Toda la información proviene de la API REST.</summary>
public class BicicletasController : Controller
{
    private readonly ServicioApi _api;

    public BicicletasController(ServicioApi api) => _api = api;

    public async Task<IActionResult> Index(string? marca, string? modelo, int? idCategoria)
    {
        var hayFiltro = !string.IsNullOrWhiteSpace(marca)
                     || !string.IsNullOrWhiteSpace(modelo)
                     || idCategoria.HasValue;

        var respuesta = hayFiltro
            ? await _api.BuscarInventarioAsync(marca, modelo, idCategoria)
            : await _api.ObtenerInventarioAsync();

        if (!respuesta.Exito)
        {
            TempData["Error"] = respuesta.Mensaje;
            return View(new List<BikeStore.Domain.Entities.InventarioBicicleta>());
        }

        ViewBag.Marca = marca;
        ViewBag.Modelo = modelo;
        ViewBag.IdCategoria = idCategoria;
        ViewBag.Categorias = (await _api.ObtenerCategoriasAsync()).Datos ?? new();

        return View(respuesta.Datos);
    }

    public async Task<IActionResult> StockBajo()
    {
        var respuesta = await _api.ObtenerStockBajoAsync();

        if (!respuesta.Exito)
        {
            TempData["Error"] = respuesta.Mensaje;
            return View(new List<BikeStore.Domain.Entities.InventarioBicicleta>());
        }

        return View(respuesta.Datos);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Categorias = (await _api.ObtenerCategoriasAsync()).Datos ?? new();
        return View(new BicicletaDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BicicletaDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = (await _api.ObtenerCategoriasAsync()).Datos ?? new();
            return View(dto);
        }

        var respuesta = await _api.CrearBicicletaAsync(dto);

        if (!respuesta.Exito)
        {
            ModelState.AddModelError(string.Empty, respuesta.Mensaje!);
            ViewBag.Categorias = (await _api.ObtenerCategoriasAsync()).Datos ?? new();
            return View(dto);
        }

        TempData["Exito"] = $"Bicicleta {dto.Marca} {dto.Modelo} registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var respuesta = await _api.ObtenerBicicletaAsync(id);

        if (!respuesta.Exito)
        {
            TempData["Error"] = respuesta.Mensaje;
            return RedirectToAction(nameof(Index));
        }

        var b = respuesta.Datos!;
        ViewBag.Categorias = (await _api.ObtenerCategoriasAsync()).Datos ?? new();
        ViewBag.Id = id;

        return View(new BicicletaDto
        {
            IdCategoria = b.IdCategoria,
            Marca = b.Marca,
            Modelo = b.Modelo,
            Descripcion = b.Descripcion,
            Precio = b.Precio,
            Stock = b.Stock,
            StockMinimo = b.StockMinimo,
            Estado = b.Estado
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BicicletaDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = (await _api.ObtenerCategoriasAsync()).Datos ?? new();
            ViewBag.Id = id;
            return View(dto);
        }

        var respuesta = await _api.ActualizarBicicletaAsync(id, dto);

        if (!respuesta.Exito)
        {
            ModelState.AddModelError(string.Empty, respuesta.Mensaje!);
            ViewBag.Categorias = (await _api.ObtenerCategoriasAsync()).Datos ?? new();
            ViewBag.Id = id;
            return View(dto);
        }

        TempData["Exito"] = "Bicicleta actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(int id)
    {
        var respuesta = await _api.DesactivarBicicletaAsync(id);

        if (respuesta.Exito)
            TempData["Exito"] = "Bicicleta desactivada del catálogo.";
        else
            TempData["Error"] = respuesta.Mensaje;

        return RedirectToAction(nameof(Index));
    }
}
