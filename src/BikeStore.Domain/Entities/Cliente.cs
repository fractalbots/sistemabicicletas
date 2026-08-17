namespace BikeStore.Domain.Entities;

/// <summary>Cliente de la tienda. La cédula es única y de 10 dígitos.</summary>
public class Cliente
{
    public int IdCliente { get; set; }
    public string Cedula { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? Direccion { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; }

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();

    public string NombreCompleto => $"{Nombres} {Apellidos}";
}
