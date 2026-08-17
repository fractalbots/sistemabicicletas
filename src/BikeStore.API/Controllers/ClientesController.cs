using BikeStore.Domain.DTOs;
using BikeStore.Domain.Entities;
using BikeStore.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.API.Controllers;

/// <summary>Gestión de los clientes de la tienda.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ClientesController : ControllerBase
{
    private readonly IRepository<Cliente> _repositorio;
    private readonly IVentaRepository _ventaRepositorio;

    public ClientesController(IRepository<Cliente> repositorio, IVentaRepository ventaRepositorio)
    {
        _repositorio = repositorio;
        _ventaRepositorio = ventaRepositorio;
    }

    /// <summary>Lista todos los clientes.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Cliente>>> ObtenerTodos()
        => Ok(await _repositorio.ObtenerTodosAsync());

    /// <summary>Obtiene un cliente por su identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Cliente>> ObtenerPorId(int id)
    {
        var cliente = await _repositorio.ObtenerPorIdAsync(id);
        return cliente is null
            ? NotFound(new { mensaje = $"No existe el cliente {id}." })
            : Ok(cliente);
    }

    /// <summary>Busca un cliente por su número de cédula.</summary>
    [HttpGet("cedula/{cedula}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Cliente>> ObtenerPorCedula(string cedula)
    {
        var resultado = await _repositorio.BuscarAsync(c => c.Cedula == cedula);
        var cliente = resultado.FirstOrDefault();

        return cliente is null
            ? NotFound(new { mensaje = $"No existe un cliente con cédula {cedula}." })
            : Ok(cliente);
    }

    /// <summary>Historial de compras del cliente (procedimiento sp_VentasPorCliente).</summary>
    [HttpGet("{id:int}/ventas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<HistorialVenta>>> ObtenerVentas(int id)
    {
        if (!await _repositorio.ExisteAsync(id))
            return NotFound(new { mensaje = $"No existe el cliente {id}." });

        return Ok(await _ventaRepositorio.ObtenerVentasPorClienteAsync(id));
    }

    /// <summary>Registra un nuevo cliente.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Cliente>> Crear([FromBody] ClienteDto dto)
    {
        var existentes = await _repositorio.BuscarAsync(c => c.Cedula == dto.Cedula);
        if (existentes.Any())
            return Conflict(new { mensaje = $"Ya existe un cliente con cédula {dto.Cedula}." });

        var cliente = new Cliente
        {
            Cedula = dto.Cedula,
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            Telefono = dto.Telefono,
            Correo = dto.Correo,
            Direccion = dto.Direccion,
            Activo = dto.Activo
        };

        await _repositorio.AgregarAsync(cliente);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = cliente.IdCliente }, cliente);
    }

    /// <summary>Actualiza los datos de un cliente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ClienteDto dto)
    {
        var cliente = await _repositorio.ObtenerPorIdAsync(id);
        if (cliente is null)
            return NotFound(new { mensaje = $"No existe el cliente {id}." });

        cliente.Cedula = dto.Cedula;
        cliente.Nombres = dto.Nombres;
        cliente.Apellidos = dto.Apellidos;
        cliente.Telefono = dto.Telefono;
        cliente.Correo = dto.Correo;
        cliente.Direccion = dto.Direccion;
        cliente.Activo = dto.Activo;

        await _repositorio.ActualizarAsync(cliente);
        return NoContent();
    }

    /// <summary>Desactiva un cliente (baja lógica, conserva su historial de ventas).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int id)
    {
        var cliente = await _repositorio.ObtenerPorIdAsync(id);
        if (cliente is null)
            return NotFound(new { mensaje = $"No existe el cliente {id}." });

        cliente.Activo = false;
        await _repositorio.ActualizarAsync(cliente);
        return NoContent();
    }
}
