using System.Linq.Expressions;
using BikeStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BikeStore.Data.Repositories;

/// <summary>
/// Implementación genérica del patrón Repository sobre EF Core.
/// Evita duplicar el CRUD básico en cada entidad.
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly BikeStoreContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(BikeStoreContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> ObtenerTodosAsync()
        => await _dbSet.AsNoTracking().ToListAsync();

    public virtual async Task<T?> ObtenerPorIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public virtual async Task<IEnumerable<T>> BuscarAsync(Expression<Func<T, bool>> predicado)
        => await _dbSet.AsNoTracking().Where(predicado).ToListAsync();

    public virtual async Task<T> AgregarAsync(T entidad)
    {
        await _dbSet.AddAsync(entidad);
        await _context.SaveChangesAsync();
        return entidad;
    }

    public virtual async Task ActualizarAsync(T entidad)
    {
        _dbSet.Update(entidad);
        await _context.SaveChangesAsync();
    }

    public virtual async Task<bool> EliminarAsync(int id)
    {
        var entidad = await _dbSet.FindAsync(id);
        if (entidad is null) return false;

        _dbSet.Remove(entidad);
        await _context.SaveChangesAsync();
        return true;
    }

    public virtual async Task<bool> ExisteAsync(int id)
        => await _dbSet.FindAsync(id) is not null;
}
