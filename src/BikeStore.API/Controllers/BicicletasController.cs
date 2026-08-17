using BikeStore.Domain.DTOs;
using BikeStore.Domain.Entities;
using BikeStore.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.API.Controllers;

/// <summary>Catálogo e inventario de bicicletas.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BicicletasController : ControllerBase
{
    private readonly IBicicletaRepository _repositorio;
    private readonly IVentaRepository _ventaRepositorio;

    public BicicletasController(IBicicletaRepository repositorio, IVentaRepository ventaRepositorio)
    {
        _repositorio = repositorio;
        _ventaRepositorio = ventaRepositorio;
    }

    /// <summary>Lista las bicicletas con su categoría.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Bicicleta>>> ObtenerTodas()
        => Ok(await _repositorio.ObtenerTodosAsync());

    /// <summary>Inventario con el estado de stock calculado (vista vw_InventarioBicicletas).</summary>
    [HttpGet("inventario")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InventarioBicicleta>>> ObtenerInventario()
        => Ok(await _repositorio.ObtenerInventarioAsync());

    /// <summary>Busca en el inventario por marca, modelo y/o categoría.</summary>
    /// <param name="marca">Coincidencia parcial de la marca.</param>
    /// <param name="modelo">Coincidencia parcial del modelo.</param>
    /// <param name="idCategoria">Filtra por categoría.</param>
    [HttpGet("buscar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InventarioBicicleta>>> Buscar(
        [FromQuery] string? marca,
        [FromQuery] string? modelo,
        [FromQuery] int? idCategoria)
        => Ok(await _repositorio.BuscarInventarioAsync(marca, modelo, idCategoria));

    /// <summary>Bicicletas en stock bajo o agotadas (procedimiento sp_BicicletasStockBajo).</summary>
    [HttpGet("stock-bajo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InventarioBicicleta>>> ObtenerStockBajo()
        => Ok(await _ventaRepositorio.ObtenerStockBajoAsync());

    /// <summary>Obtiene una bicicleta por su identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Bicicleta>> ObtenerPorId(int id)
    {
        var bicicleta = await _repositorio.ObtenerPorIdAsync(id);
        return bicicleta is null
            ? NotFound(new { mensaje = $"No existe la bicicleta {id}." })
            : Ok(bicicleta);
    }

    /// <summary>Registra una nueva bicicleta en el catálogo.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Bicicleta>> Crear([FromBody] BicicletaDto dto)
    {
        var bicicleta = new Bicicleta
        {
            IdCategoria = dto.IdCategoria,
            Marca = dto.Marca,
            Modelo = dto.Modelo,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            Stock = dto.Stock,
            StockMinimo = dto.StockMinimo,
            Estado = dto.Estado
        };

        await _repositorio.AgregarAsync(bicicleta);
        return CreatedAtAction(nameof(ObtenerPorId),
                               new { id = bicicleta.IdBicicleta }, bicicleta);
    }

    /// <summary>Actualiza los datos de una bicicleta.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] BicicletaDto dto)
    {
        var bicicleta = await _repositorio.ObtenerPorIdAsync(id);
        if (bicicleta is null)
            return NotFound(new { mensaje = $"No existe la bicicleta {id}." });

        bicicleta.IdCategoria = dto.IdCategoria;
        bicicleta.Marca = dto.Marca;
        bicicleta.Modelo = dto.Modelo;
        bicicleta.Descripcion = dto.Descripcion;
        bicicleta.Precio = dto.Precio;
        bicicleta.Stock = dto.Stock;
        bicicleta.StockMinimo = dto.StockMinimo;
        bicicleta.Estado = dto.Estado;

        await _repositorio.ActualizarAsync(bicicleta);
        return NoContent();
    }

    /// <summary>Desactiva una bicicleta (baja lógica, conserva el histórico de ventas).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int id)
    {
        var bicicleta = await _repositorio.ObtenerPorIdAsync(id);
        if (bicicleta is null)
            return NotFound(new { mensaje = $"No existe la bicicleta {id}." });

        bicicleta.Estado = false;
        await _repositorio.ActualizarAsync(bicicleta);
        return NoContent();
    }
}
