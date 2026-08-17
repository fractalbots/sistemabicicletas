namespace BikeStore.Domain.Entities;

/// <summary>Categoría a la que pertenece una bicicleta (Montaña, Ruta, etc.).</summary>
public class Categoria
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; }

    public ICollection<Bicicleta> Bicicletas { get; set; } = new List<Bicicleta>();
}
