using BikeStore.Domain.DTOs;
using BikeStore.Domain.Entities;
using BikeStore.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.API.Controllers;

/// <summary>Gestión de las categorías del catálogo.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriasController : ControllerBase
{
    private readonly IRepository<Categoria> _repositorio;

    public CategoriasController(IRepository<Categoria> repositorio)
        => _repositorio = repositorio;

    /// <summary>Lista todas las categorías registradas.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Categoria>>> ObtenerTodas()
        => Ok(await _repositorio.ObtenerTodosAsync());

    /// <summary>Obtiene una categoría por su identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Categoria>> ObtenerPorId(int id)
    {
        var categoria = await _repositorio.ObtenerPorIdAsync(id);
        return categoria is null
            ? NotFound(new { mensaje = $"No existe la categoría {id}." })
            : Ok(categoria);
    }

    /// <summary>Crea una nueva categoría.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Categoria>> Crear([FromBody] CategoriaDto dto)
    {
        var categoria = new Categoria
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Activo = dto.Activo
        };

        await _repositorio.AgregarAsync(categoria);
        return CreatedAtAction(nameof(ObtenerPorId),
                               new { id = categoria.IdCategoria }, categoria);
    }

    /// <summary>Actualiza los datos de una categoría existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] CategoriaDto dto)
    {
        var categoria = await _repositorio.ObtenerPorIdAsync(id);
        if (categoria is null)
            return NotFound(new { mensaje = $"No existe la categoría {id}." });

        categoria.Nombre = dto.Nombre;
        categoria.Descripcion = dto.Descripcion;
        categoria.Activo = dto.Activo;

        await _repositorio.ActualizarAsync(categoria);
        return NoContent();
    }

    /// <summary>Elimina una categoría. Falla si tiene bicicletas asociadas.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var eliminada = await _repositorio.EliminarAsync(id);
            return eliminada
                ? NoContent()
                : NotFound(new { mensaje = $"No existe la categoría {id}." });
        }
        catch (Exception)
        {
            return Conflict(new
            {
                mensaje = "No se puede eliminar: la categoría tiene bicicletas asociadas. " +
                          "Desactívela en lugar de eliminarla."
            });
        }
    }
}
