using BikeStore.Domain.DTOs;
using BikeStore.Web.Models;
using BikeStore.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.Web.Controllers;

/// <summary>
/// Catalogo e inventario. Toda la informacion se obtiene de la API REST:
/// este controlador no conoce la base de datos.
/// </summary>
public class BicicletasController : Controller
{
    private readonly BikeStoreApiClient _api;

    public BicicletasController(BikeStoreApiClient api) => _api = api;

    /// <summary>Inventario con filtros por marca, modelo y categoria.</summary>
    public async Task<IActionResult> Index(string? marca, string? modelo, int? idCategoria)
    {
        var modeloVista = new FiltroInventarioViewModel
        {
            Marca = marca,
            Modelo = modelo,
            IdCategoria = idCategoria,
            Categorias = await _api.ObtenerCategoriasAsync()
        };

        modeloVista.Resultados = modeloVista.HayFiltros
            ? await _api.BuscarInventarioAsync(marca, modelo, idCategoria)
            : await _api.ObtenerInventarioAsync();

        return View(modeloVista);
    }

    /// <summary>Productos que requieren reposicion (sp_BicicletasStockBajo).</summary>
    public async Task<IActionResult> StockBajo()
        => View(await _api.ObtenerStockBajoAsync());

    [HttpGet]
    public async Task<IActionResult> Create()
        => View(new BicicletaFormViewModel { Categorias = await _api.ObtenerCategoriasAsync() });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BicicletaFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            modelo.Categorias = await _api.ObtenerCategoriasAsync();
            return View(modelo);
        }

        var (exito, error) = await _api.CrearBicicletaAsync(modelo.Datos);
        if (!exito)
        {
            ModelState.AddModelError(string.Empty, error ?? "No se pudo registrar la bicicleta.");
            modelo.Categorias = await _api.ObtenerCategoriasAsync();
            return View(modelo);
        }

        TempData["Exito"] = $"Bicicleta {modelo.Datos.Marca} {modelo.Datos.Modelo} registrada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var bicicleta = await _api.ObtenerBicicletaAsync(id);
        if (bicicleta is null) return NotFound();

        return View(new BicicletaFormViewModel
        {
            IdBicicleta = bicicleta.IdBicicleta,
            Categorias = await _api.ObtenerCategoriasAsync(),
            Datos = new BicicletaDto
            {
                IdCategoria = bicicleta.IdCategoria,
                Marca = bicicleta.Marca,
                Modelo = bicicleta.Modelo,
                Descripcion = bicicleta.Descripcion,
                Precio = bicicleta.Precio,
                Stock = bicicleta.Stock,
                StockMinimo = bicicleta.StockMinimo,
                Estado = bicicleta.Estado
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BicicletaFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            modelo.Categorias = await _api.ObtenerCategoriasAsync();
            return View(modelo);
        }

        var (exito, error) = await _api.ActualizarBicicletaAsync(modelo.IdBicicleta, modelo.Datos);
        if (!exito)
        {
            ModelState.AddModelError(string.Empty, error ?? "No se pudo actualizar la bicicleta.");
            modelo.Categorias = await _api.ObtenerCategoriasAsync();
            return View(modelo);
        }

        TempData["Exito"] = "Bicicleta actualizada.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Baja logica: conserva el historial de ventas del producto.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(int id)
    {
        var (exito, error) = await _api.DesactivarBicicletaAsync(id);
        if (exito) TempData["Exito"] = "Bicicleta desactivada.";
        else       TempData["Error"] = error;

        return RedirectToAction(nameof(Index));
    }
}
