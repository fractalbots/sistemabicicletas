using BikeStore.Domain.Entities;

namespace BikeStore.Domain.Interfaces;

/// <summary>Consultas de catálogo apoyadas en la vista vw_InventarioBicicletas.</summary>
public interface IBicicletaRepository : IRepository<Bicicleta>
{
    Task<IEnumerable<InventarioBicicleta>> ObtenerInventarioAsync();
    Task<IEnumerable<InventarioBicicleta>> BuscarInventarioAsync(string? marca, string? modelo, int? idCategoria);
}
