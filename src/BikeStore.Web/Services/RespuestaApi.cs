namespace BikeStore.Web.Services;

/// <summary>
/// Envuelve el resultado de una llamada a la API para que los controladores
/// MVC distingan éxito de error sin tener que manejar excepciones.
/// </summary>
public class RespuestaApi<T>
{
    public bool Exito { get; init; }
    public T? Datos { get; init; }
    public string? Mensaje { get; init; }
    public int CodigoHttp { get; init; }

    public static RespuestaApi<T> Correcta(T datos, int codigo = 200)
        => new() { Exito = true, Datos = datos, CodigoHttp = codigo };

    public static RespuestaApi<T> Fallida(string mensaje, int codigo)
        => new() { Exito = false, Mensaje = mensaje, CodigoHttp = codigo };
}

/// <summary>Cuerpo de error que devuelve la API.</summary>
public class ErrorApi
{
    public string? Mensaje { get; set; }
    public string? Title { get; set; }
    public string? Detalle { get; set; }
}
