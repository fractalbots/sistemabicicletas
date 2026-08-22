using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BikeStore.Domain.DTOs;
using BikeStore.Domain.Entities;

namespace BikeStore.Web.Services;

/// <summary>
/// Unico punto de contacto entre la aplicacion web y la API REST.
/// Los controladores MVC dependen de esta clase y nunca construyen
/// peticiones HTTP por su cuenta.
/// </summary>
public class BikeStoreApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<BikeStoreApiClient> _log;

    public BikeStoreApiClient(HttpClient http, ILogger<BikeStoreApiClient> log)
    {
        _http = http;
        _log = log;
    }

    /* ============================================================
       Utilidades internas
       ============================================================ */

    /// <summary>GET que devuelve una lista; ante cualquier fallo devuelve lista vacia.</summary>
    private async Task<List<T>> ObtenerListaAsync<T>(string ruta)
    {
        try
        {
            var datos = await _http.GetFromJsonAsync<List<T>>(ruta);
            return datos ?? new List<T>();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error al consultar {Ruta}", ruta);
            return new List<T>();
        }
    }

    /// <summary>GET de un elemento; devuelve null si no existe o si la API no responde.</summary>
    private async Task<T?> ObtenerUnoAsync<T>(string ruta) where T : class
    {
        try
        {
            var respuesta = await _http.GetAsync(ruta);
            if (!respuesta.IsSuccessStatusCode) return null;
            return await respuesta.Content.ReadFromJsonAsync<T>();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error al consultar {Ruta}", ruta);
            return null;
        }
    }

    /// <summary>
    /// Extrae el campo "mensaje" que devuelve la API en sus respuestas de error.
    /// Es lo que permite mostrar al usuario "Stock insuficiente..." en lugar de
    /// un codigo HTTP suelto.
    /// </summary>
    private static async Task<string> LeerMensajeErrorAsync(HttpResponseMessage respuesta)
    {
        try
        {
            var cuerpo = await respuesta.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(cuerpo))
            {
                using var doc = JsonDocument.Parse(cuerpo);
                if (doc.RootElement.TryGetProperty("mensaje", out var mensaje))
                    return mensaje.GetString() ?? "Error desconocido.";
                if (doc.RootElement.TryGetProperty("title", out var titulo))
                    return titulo.GetString() ?? "Error desconocido.";
            }
        }
        catch { /* el cuerpo no era JSON: se usa el mensaje generico */ }

        return respuesta.StatusCode switch
        {
            HttpStatusCode.NotFound => "El recurso solicitado no existe.",
            HttpStatusCode.Conflict => "La operacion entra en conflicto con el estado actual.",
            HttpStatusCode.BadRequest => "Los datos enviados no son validos.",
            _ => $"La API respondio {(int)respuesta.StatusCode}."
        };
    }

    private async Task<(bool Exito, string? Error)> EnviarAsync(HttpMethod metodo, string ruta, object? cuerpo = null)
    {
        try
        {
            using var peticion = new HttpRequestMessage(metodo, ruta);
            if (cuerpo is not null)
                peticion.Content = JsonContent.Create(cuerpo);

            var respuesta = await _http.SendAsync(peticion);
            if (respuesta.IsSuccessStatusCode) return (true, null);

            return (false, await LeerMensajeErrorAsync(respuesta));
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error al llamar {Ruta}", ruta);
            return (false, "No se pudo contactar con la API. Verifique que este en ejecucion.");
        }
    }

    /* ============================================================
       Categorias
       ============================================================ */

    public Task<List<Categoria>> ObtenerCategoriasAsync()
        => ObtenerListaAsync<Categoria>("api/categorias");

    /* ============================================================
       Bicicletas e inventario
       ============================================================ */

    public Task<List<InventarioBicicleta>> ObtenerInventarioAsync()
        => ObtenerListaAsync<InventarioBicicleta>("api/bicicletas/inventario");

    public Task<List<InventarioBicicleta>> ObtenerStockBajoAsync()
        => ObtenerListaAsync<InventarioBicicleta>("api/bicicletas/stock-bajo");

    public Task<List<InventarioBicicleta>> BuscarInventarioAsync(string? marca, string? modelo, int? idCategoria)
    {
        var parametros = new List<string>();
        if (!string.IsNullOrWhiteSpace(marca))  parametros.Add($"marca={Uri.EscapeDataString(marca)}");
        if (!string.IsNullOrWhiteSpace(modelo)) parametros.Add($"modelo={Uri.EscapeDataString(modelo)}");
        if (idCategoria is > 0)                 parametros.Add($"idCategoria={idCategoria}");

        var ruta = "api/bicicletas/buscar";
        if (parametros.Count > 0) ruta += "?" + string.Join("&", parametros);

        return ObtenerListaAsync<InventarioBicicleta>(ruta);
    }

    public Task<Bicicleta?> ObtenerBicicletaAsync(int id)
        => ObtenerUnoAsync<Bicicleta>($"api/bicicletas/{id}");

    public Task<(bool Exito, string? Error)> CrearBicicletaAsync(BicicletaDto dto)
        => EnviarAsync(HttpMethod.Post, "api/bicicletas", dto);

    public Task<(bool Exito, string? Error)> ActualizarBicicletaAsync(int id, BicicletaDto dto)
        => EnviarAsync(HttpMethod.Put, $"api/bicicletas/{id}", dto);

    public Task<(bool Exito, string? Error)> DesactivarBicicletaAsync(int id)
        => EnviarAsync(HttpMethod.Delete, $"api/bicicletas/{id}");

    /* ============================================================
       Clientes
       ============================================================ */

    public Task<List<Cliente>> ObtenerClientesAsync()
        => ObtenerListaAsync<Cliente>("api/clientes");

    public Task<Cliente?> ObtenerClienteAsync(int id)
        => ObtenerUnoAsync<Cliente>($"api/clientes/{id}");

    public Task<Cliente?> BuscarClientePorCedulaAsync(string cedula)
        => ObtenerUnoAsync<Cliente>($"api/clientes/cedula/{Uri.EscapeDataString(cedula)}");

    public Task<List<HistorialVenta>> ObtenerHistorialClienteAsync(int id)
        => ObtenerListaAsync<HistorialVenta>($"api/clientes/{id}/ventas");

    public Task<(bool Exito, string? Error)> CrearClienteAsync(ClienteDto dto)
        => EnviarAsync(HttpMethod.Post, "api/clientes", dto);

    public Task<(bool Exito, string? Error)> ActualizarClienteAsync(int id, ClienteDto dto)
        => EnviarAsync(HttpMethod.Put, $"api/clientes/{id}", dto);

    public Task<(bool Exito, string? Error)> DesactivarClienteAsync(int id)
        => EnviarAsync(HttpMethod.Delete, $"api/clientes/{id}");

    /* ============================================================
       Ventas
       ============================================================ */

    public Task<List<Venta>> ObtenerVentasAsync()
        => ObtenerListaAsync<Venta>("api/ventas");

    public Task<Venta?> ObtenerVentaAsync(int id)
        => ObtenerUnoAsync<Venta>($"api/ventas/{id}");

    /// <summary>
    /// Registra la venta. Devuelve el numero generado, o el mensaje de error
    /// que produjo el procedimiento almacenado (stock insuficiente, cliente
    /// inactivo, venta vacia).
    /// </summary>
    public async Task<(int? IdVenta, string? Error)> RegistrarVentaAsync(CrearVentaDto dto)
    {
        try
        {
            var respuesta = await _http.PostAsJsonAsync("api/ventas", dto);

            if (!respuesta.IsSuccessStatusCode)
                return (null, await LeerMensajeErrorAsync(respuesta));

            var venta = await respuesta.Content.ReadFromJsonAsync<Venta>();
            return (venta?.IdVenta, null);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error al registrar la venta");
            return (null, "No se pudo contactar con la API. Verifique que este en ejecucion.");
        }
    }

    public Task<(bool Exito, string? Error)> AnularVentaAsync(int id)
        => EnviarAsync(HttpMethod.Post, $"api/ventas/{id}/anular");
}
