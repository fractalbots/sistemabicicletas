using BikeStore.Data.Repositories;
using BikeStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BikeStore.Data;

/// <summary>
/// Registro centralizado de la capa de datos. La API sólo llama a este método
/// y no necesita conocer las implementaciones concretas de los repositorios.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddCapaDatos(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<BikeStoreContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IBicicletaRepository, BicicletaRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();

        return services;
    }
}
