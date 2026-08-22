using System.ComponentModel.DataAnnotations;
using BikeStore.Domain.DTOs;
using BikeStore.Domain.Entities;

namespace BikeStore.Web.Models;

/// <summary>Datos que la pantalla de nueva venta envia al servidor.</summary>
public class NuevaVentaViewModel
{
    [Display(Name = "Cliente")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cliente.")]
    public int IdCliente { get; set; }

    /// <summary>Lineas agregadas en la pantalla antes de confirmar.</summary>
    public List<DetalleVentaItemDto> Detalle { get; set; } = new();

    /* Listas de apoyo, se recargan en cada peticion */
    public List<Cliente> Clientes { get; set; } = new();
    public List<InventarioBicicleta> Bicicletas { get; set; } = new();

    /// <summary>Porcentaje de IVA usado solo para el calculo estimado en pantalla.</summary>
    public decimal PorcentajeIva { get; set; } = 15m;
}

/// <summary>Filtros de la pantalla de inventario.</summary>
public class FiltroInventarioViewModel
{
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public int? IdCategoria { get; set; }

    public List<Categoria> Categorias { get; set; } = new();
    public List<InventarioBicicleta> Resultados { get; set; } = new();

    public bool HayFiltros =>
        !string.IsNullOrWhiteSpace(Marca) || !string.IsNullOrWhiteSpace(Modelo) || IdCategoria is > 0;
}

/// <summary>Formulario de alta y edicion de bicicletas.</summary>
public class BicicletaFormViewModel
{
    public int IdBicicleta { get; set; }
    public BicicletaDto Datos { get; set; } = new();
    public List<Categoria> Categorias { get; set; } = new();
    public bool EsEdicion => IdBicicleta > 0;
}

/// <summary>Formulario de alta y edicion de clientes.</summary>
public class ClienteFormViewModel
{
    public int IdCliente { get; set; }
    public ClienteDto Datos { get; set; } = new();
    public bool EsEdicion => IdCliente > 0;
}

/// <summary>Historial de compras de un cliente.</summary>
public class HistorialClienteViewModel
{
    public Cliente? Cliente { get; set; }
    public List<HistorialVenta> Ventas { get; set; } = new();
}

/// <summary>Cifras del panel de inicio.</summary>
public class PanelViewModel
{
    public int TotalBicicletas { get; set; }
    public int TotalClientes { get; set; }
    public int TotalVentas { get; set; }
    public int PorReponer { get; set; }
    public decimal VentasTotales { get; set; }
    public bool ApiDisponible { get; set; } = true;
    public List<InventarioBicicleta> StockCritico { get; set; } = new();
    public List<Venta> UltimasVentas { get; set; } = new();
}
