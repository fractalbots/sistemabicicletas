using System.Net.Http.Json;
using System.Text.Json;
using BikeStore.Domain.DTOs;
using BikeStore.Domain.Entities;

namespace BikeStore.Web.Services;

/// <summary>
/// Único punto de acceso a BikeStore.API desde el frontend MVC.
/// Centraliza rutas, serialización y traducción de errores.
/// </summary>
public class ServicioApi
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions _opciones = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ServicioApi(HttpClient http) => _http = http;

    /* ------------------------- Bicicletas ------------------------- */

    public Task<RespuestaApi<List<InventarioBicicleta>>> ObtenerInventarioAsync()
        => Get<List<InventarioBicicleta>>("api/Bicicletas/inventario");

    public Task<RespuestaApi<List<InventarioBicicleta>>> BuscarInventarioAsync(
        string? marca, string? modelo, int? idCategoria)
    {
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(marca)) partes.Add($"marca={Uri.EscapeDataString(marca)}");
        if (!string.IsNullOrWhiteSpace(modelo)) partes.Add($"modelo={Uri.EscapeDataString(modelo)}");
        if (idCategoria.HasValue) partes.Add($"idCategoria={idCategoria}");

        var qs = partes.Count > 0 ? "?" + string.Join("&", partes) : string.Empty;
        return Get<List<InventarioBicicleta>>($"api/Bicicletas/buscar{qs}");
    }

    public Task<RespuestaApi<List<InventarioBicicleta>>> ObtenerStockBajoAsync()
        => Get<List<InventarioBicicleta>>("api/Bicicletas/stock-bajo");

    public Task<RespuestaApi<Bicicleta>> ObtenerBicicletaAsync(int id)
        => Get<Bicicleta>($"api/Bicicletas/{id}");

    public Task<RespuestaApi<Bicicleta>> CrearBicicletaAsync(BicicletaDto dto)
        => Post<Bicicleta>("api/Bicicletas", dto);

    public Task<RespuestaApi<bool>> ActualizarBicicletaAsync(int id, BicicletaDto dto)
        => Put($"api/Bicicletas/{id}", dto);

    public Task<RespuestaApi<bool>> DesactivarBicicletaAsync(int id)
        => Delete($"api/Bicicletas/{id}");

    /* ------------------------- Categorías ------------------------- */

    public Task<RespuestaApi<List<Categoria>>> ObtenerCategoriasAsync()
        => Get<List<Categoria>>("api/Categorias");

    /* -------------------------- Clientes -------------------------- */

    public Task<RespuestaApi<List<Cliente>>> ObtenerClientesAsync()
        => Get<List<Cliente>>("api/Clientes");

    public Task<RespuestaApi<Cliente>> ObtenerClienteAsync(int id)
        => Get<Cliente>($"api/Clientes/{id}");

    public Task<RespuestaApi<Cliente>> CrearClienteAsync(ClienteDto dto)
        => Post<Cliente>("api/Clientes", dto);

    public Task<RespuestaApi<bool>> ActualizarClienteAsync(int id, ClienteDto dto)
        => Put($"api/Clientes/{id}", dto);

    public Task<RespuestaApi<bool>> DesactivarClienteAsync(int id)
        => Delete($"api/Clientes/{id}");

    public Task<RespuestaApi<List<HistorialVenta>>> ObtenerHistorialClienteAsync(int id)
        => Get<List<HistorialVenta>>($"api/Clientes/{id}/ventas");

    /* --------------------------- Ventas --------------------------- */

    public Task<RespuestaApi<List<Venta>>> ObtenerVentasAsync()
        => Get<List<Venta>>("api/Ventas");

    public Task<RespuestaApi<Venta>> ObtenerVentaAsync(int id)
        => Get<Venta>($"api/Ventas/{id}");

    public Task<RespuestaApi<Venta>> RegistrarVentaAsync(CrearVentaDto dto)
        => Post<Venta>("api/Ventas", dto);

    public async Task<RespuestaApi<bool>> AnularVentaAsync(int id)
    {
        try
        {
            var r = await _http.PostAsync($"api/Ventas/{id}/anular", null);

            return r.IsSuccessStatusCode
                ? RespuestaApi<bool>.Correcta(true, (int)r.StatusCode)
                : RespuestaApi<bool>.Fallida(await LeerError(r), (int)r.StatusCode);
        }
        catch (Exception ex)
        {
            return RespuestaApi<bool>.Fallida(SinConexion(ex), 503);
        }
    }

    /* ----------------------- Métodos privados ---------------------- */

    private async Task<RespuestaApi<T>> Get<T>(string ruta)
    {
        try
        {
            var r = await _http.GetAsync(ruta);

            if (!r.IsSuccessStatusCode)
                return RespuestaApi<T>.Fallida(await LeerError(r), (int)r.StatusCode);

            var datos = await r.Content.ReadFromJsonAsync<T>(_opciones);

            return datos is null
                ? RespuestaApi<T>.Fallida("La API devolvió una respuesta vacía.", 204)
                : RespuestaApi<T>.Correcta(datos);
        }
        catch (Exception ex)
        {
            return RespuestaApi<T>.Fallida(SinConexion(ex), 503);
        }
    }

    private async Task<RespuestaApi<T>> Post<T>(string ruta, object cuerpo)
    {
        try
        {
            var r = await _http.PostAsJsonAsync(ruta, cuerpo);

            if (!r.IsSuccessStatusCode)
                return RespuestaApi<T>.Fallida(await LeerError(r), (int)r.StatusCode);

            var datos = await r.Content.ReadFromJsonAsync<T>(_opciones);
            return RespuestaApi<T>.Correcta(datos!, (int)r.StatusCode);
        }
        catch (Exception ex)
        {
            return RespuestaApi<T>.Fallida(SinConexion(ex), 503);
        }
    }

    private async Task<RespuestaApi<bool>> Put(string ruta, object cuerpo)
    {
        try
        {
            var r = await _http.PutAsJsonAsync(ruta, cuerpo);

            return r.IsSuccessStatusCode
                ? RespuestaApi<bool>.Correcta(true, (int)r.StatusCode)
                : RespuestaApi<bool>.Fallida(await LeerError(r), (int)r.StatusCode);
        }
        catch (Exception ex)
        {
            return RespuestaApi<bool>.Fallida(SinConexion(ex), 503);
        }
    }

    private async Task<RespuestaApi<bool>> Delete(string ruta)
    {
        try
        {
            var r = await _http.DeleteAsync(ruta);

            return r.IsSuccessStatusCode
                ? RespuestaApi<bool>.Correcta(true, (int)r.StatusCode)
                : RespuestaApi<bool>.Fallida(await LeerError(r), (int)r.StatusCode);
        }
        catch (Exception ex)
        {
            return RespuestaApi<bool>.Fallida(SinConexion(ex), 503);
        }
    }

    /// <summary>Extrae el mensaje de error del cuerpo que envía la API.</summary>
    private static async Task<string> LeerError(HttpResponseMessage r)
    {
        try
        {
            var error = await r.Content.ReadFromJsonAsync<ErrorApi>(_opciones);

            return error?.Mensaje
                ?? error?.Title
                ?? $"La API respondió con el código {(int)r.StatusCode}.";
        }
        catch
        {
            return $"La API respondió con el código {(int)r.StatusCode}.";
        }
    }

    private static string SinConexion(Exception ex)
        => "No se pudo contactar con la API. Verifique que BikeStore.API esté en ejecución. "
         + $"({ex.Message})";
}
