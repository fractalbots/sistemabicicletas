using BikeStore.Domain.DTOs;
using BikeStore.Domain.Entities;
using BikeStore.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BikeStore.API.Controllers;

/// <summary>
/// Registro y consulta de ventas. Las operaciones de escritura se resuelven
/// mediante procedimientos almacenados para garantizar atomicidad.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VentasController : ControllerBase
{
    private readonly IVentaRepository _repositorio;

    public VentasController(IVentaRepository repositorio)
        => _repositorio = repositorio;

    /// <summary>Lista todas las ventas con los datos del cliente.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Venta>>> ObtenerTodas()
        => Ok(await _repositorio.ObtenerTodosAsync());

    /// <summary>Obtiene la factura completa: cabecera, cliente y líneas de detalle.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Venta>> ObtenerPorId(int id)
    {
        var venta = await _repositorio.ObtenerConDetalleAsync(id);
        return venta is null
            ? NotFound(new { mensaje = $"No existe la venta {id}." })
            : Ok(venta);
    }

    /// <summary>
    /// Registra una venta ejecutando sp_RegistrarVenta. El procedimiento valida
    /// el stock, descuenta el inventario y calcula subtotal, IVA y total en una
    /// sola transacción. Los precios se leen del servidor, nunca del cliente.
    /// </summary>
    /// <response code="201">Venta registrada correctamente.</response>
    /// <response code="400">Datos inválidos o cliente/bicicleta inexistente.</response>
    /// <response code="409">Stock insuficiente para completar la venta.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Venta>> Registrar([FromBody] CrearVentaDto dto)
    {
        try
        {
            var idVenta = await _repositorio.RegistrarVentaAsync(dto);
            var venta = await _repositorio.ObtenerConDetalleAsync(idVenta);

            return CreatedAtAction(nameof(ObtenerPorId), new { id = idVenta }, venta);
        }
        catch (SqlException ex)
        {
            return TraducirErrorSql(ex);
        }
    }

    /// <summary>
    /// Anula una venta ejecutando sp_AnularVenta, que devuelve el stock al inventario.
    /// </summary>
    [HttpPost("{id:int}/anular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Anular(int id)
    {
        try
        {
            await _repositorio.AnularVentaAsync(id);
            return Ok(new { mensaje = $"Venta {id} anulada y stock restituido." });
        }
        catch (SqlException ex)
        {
            return TraducirErrorSql(ex);
        }
    }

    /// <summary>
    /// Convierte los códigos de error lanzados por los procedimientos almacenados
    /// en respuestas HTTP con el significado correcto.
    /// </summary>
    private ObjectResult TraducirErrorSql(SqlException ex) => ex.Number switch
    {
        50001 => BadRequest(new { mensaje = "El cliente no existe o está inactivo." }),
        50002 => BadRequest(new { mensaje = "La venta debe contener al menos un producto." }),
        50003 => BadRequest(new { mensaje = "Las cantidades deben ser mayores a cero." }),
        50004 => BadRequest(new { mensaje = "Una o más bicicletas no existen o están inactivas." }),
        50005 => Conflict(new { mensaje = "Stock insuficiente para completar la venta." }),
        50006 => NotFound(new { mensaje = "La venta no existe o ya fue anulada." }),
        _     => StatusCode(StatusCodes.Status500InternalServerError,
                            new { mensaje = "Error al procesar la operación.", detalle = ex.Message })
    };
}
