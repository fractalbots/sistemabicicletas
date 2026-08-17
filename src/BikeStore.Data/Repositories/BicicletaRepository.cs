using BikeStore.Domain.Entities;
using BikeStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BikeStore.Data.Repositories;

/// <summary>Repositorio de bicicletas: CRUD heredado más consultas sobre la vista de inventario.</summary>
public class BicicletaRepository : Repository<Bicicleta>, IBicicletaRepository
{
    public BicicletaRepository(BikeStoreContext context) : base(context) { }

    public override async Task<IEnumerable<Bicicleta>> ObtenerTodosAsync()
        => await _context.Bicicletas
                         .AsNoTracking()
                         .Include(b => b.Categoria)
                         .OrderBy(b => b.Marca).ThenBy(b => b.Modelo)
                         .ToListAsync();

    public async Task<IEnumerable<InventarioBicicleta>> ObtenerInventarioAsync()
        => await _context.InventarioBicicletas.AsNoTracking().ToListAsync();

    public async Task<IEnumerable<InventarioBicicleta>> BuscarInventarioAsync(
        string? marca, string? modelo, int? idCategoria)
    {
        var consulta = _context.InventarioBicicletas.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(marca))
            consulta = consulta.Where(b => b.Marca.Contains(marca));

        if (!string.IsNullOrWhiteSpace(modelo))
            consulta = consulta.Where(b => b.Modelo.Contains(modelo));

        if (idCategoria.HasValue)
            consulta = consulta.Where(b => b.IdCategoria == idCategoria.Value);

        return await consulta.OrderBy(b => b.Marca).ThenBy(b => b.Modelo).ToListAsync();
    }
}
