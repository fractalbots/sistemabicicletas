using BikeStore.Domain.Entities;

namespace BikeStore.Web.Models;

/// <summary>
/// Modelo del formulario de nueva venta. El detalle viaja como JSON en un
/// campo oculto que arma el navegador, para permitir un carrito dinámico.
/// </summary>
public class CrearVentaViewModel
{
    public int IdCliente { get; set; }
    public string DetalleJson { get; set; } = "[]";

    public List<Cliente> Clientes { get; set; } = new();
    public List<InventarioBicicleta> Bicicletas { get; set; } = new();
}
