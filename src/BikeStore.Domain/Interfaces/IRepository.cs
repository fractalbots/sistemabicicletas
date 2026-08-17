using System.Linq.Expressions;

namespace BikeStore.Domain.Interfaces;

/// <summary>
/// Contrato genérico de acceso a datos. Se declara en el Dominio para que las
/// capas superiores dependan de la abstracción y no de Entity Framework.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> ObtenerTodosAsync();
    Task<T?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<T>> BuscarAsync(Expression<Func<T, bool>> predicado);
    Task<T> AgregarAsync(T entidad);
    Task ActualizarAsync(T entidad);
    Task<bool> EliminarAsync(int id);
    Task<bool> ExisteAsync(int id);
}
