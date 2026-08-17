using BikeStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BikeStore.Data;

/// <summary>
/// Contexto de Entity Framework Core mapeado contra el script 01_BikeStoreDB.sql.
/// El modelo se configura con Fluent API para reflejar exactamente el esquema
/// existente; no se usan migraciones porque la base de datos ya está creada.
/// </summary>
public class BikeStoreContext : DbContext
{
    public BikeStoreContext(DbContextOptions<BikeStoreContext> options) : base(options) { }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Bicicleta> Bicicletas => Set<Bicicleta>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();

    // Vistas de solo lectura
    public DbSet<InventarioBicicleta> InventarioBicicletas => Set<InventarioBicicleta>();
    public DbSet<HistorialVenta> HistorialVentas => Set<HistorialVenta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(e =>
        {
            e.ToTable("Categoria");
            e.HasKey(x => x.IdCategoria);
            e.Property(x => x.Nombre).HasColumnType("varchar(60)").IsRequired();
            e.Property(x => x.Descripcion).HasColumnType("varchar(200)");
            e.Property(x => x.Activo).HasDefaultValue(true);
            e.Property(x => x.FechaRegistro).HasDefaultValueSql("SYSDATETIME()");
            e.HasIndex(x => x.Nombre).IsUnique();
        });

        modelBuilder.Entity<Bicicleta>(e =>
        {
            e.ToTable("Bicicleta");
            e.HasKey(x => x.IdBicicleta);
            e.Property(x => x.Marca).HasColumnType("varchar(60)").IsRequired();
            e.Property(x => x.Modelo).HasColumnType("varchar(80)").IsRequired();
            e.Property(x => x.Descripcion).HasColumnType("varchar(250)");
            e.Property(x => x.Precio).HasColumnType("decimal(12,2)");
            e.Property(x => x.Stock).HasDefaultValue(0);
            e.Property(x => x.StockMinimo).HasDefaultValue(5);
            e.Property(x => x.Estado).HasDefaultValue(true);
            e.Property(x => x.FechaRegistro).HasDefaultValueSql("SYSDATETIME()");
            e.HasIndex(x => new { x.Marca, x.Modelo }).IsUnique();

            e.HasOne(x => x.Categoria)
             .WithMany(c => c.Bicicletas)
             .HasForeignKey(x => x.IdCategoria)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cliente>(e =>
        {
            e.ToTable("Cliente");
            e.HasKey(x => x.IdCliente);
            e.Property(x => x.Cedula).HasColumnType("char(10)").IsRequired();
            e.Property(x => x.Nombres).HasColumnType("varchar(80)").IsRequired();
            e.Property(x => x.Apellidos).HasColumnType("varchar(80)").IsRequired();
            e.Property(x => x.Telefono).HasColumnType("varchar(15)");
            e.Property(x => x.Correo).HasColumnType("varchar(120)");
            e.Property(x => x.Direccion).HasColumnType("varchar(200)");
            e.Property(x => x.Activo).HasDefaultValue(true);
            e.Property(x => x.FechaRegistro).HasDefaultValueSql("SYSDATETIME()");
            e.HasIndex(x => x.Cedula).IsUnique();
            e.Ignore(x => x.NombreCompleto);
        });

        modelBuilder.Entity<Venta>(e =>
        {
            e.ToTable("Venta");
            e.HasKey(x => x.IdVenta);
            e.Property(x => x.Fecha).HasDefaultValueSql("SYSDATETIME()");
            e.Property(x => x.Subtotal).HasColumnType("decimal(12,2)");
            e.Property(x => x.PorcentajeIva).HasColumnType("decimal(5,2)").HasDefaultValue(15.00m);
            e.Property(x => x.Iva).HasColumnType("decimal(12,2)");
            e.Property(x => x.Total).HasColumnType("decimal(12,2)");
            e.Property(x => x.Estado).HasColumnType("varchar(15)").HasDefaultValue("EMITIDA");

            e.HasOne(x => x.Cliente)
             .WithMany(c => c.Ventas)
             .HasForeignKey(x => x.IdCliente)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DetalleVenta>(e =>
        {
            e.ToTable("DetalleVenta");
            e.HasKey(x => x.IdDetalle);
            e.Property(x => x.PrecioUnitario).HasColumnType("decimal(12,2)");

            // Columna calculada persistida: SQL Server la genera, EF solo la lee.
            e.Property(x => x.Subtotal)
             .HasColumnType("decimal(12,2)")
             .HasComputedColumnSql("CONVERT(DECIMAL(12,2), Cantidad * PrecioUnitario)", stored: true);

            e.HasIndex(x => new { x.IdVenta, x.IdBicicleta }).IsUnique();

            e.HasOne(x => x.Venta)
             .WithMany(v => v.Detalles)
             .HasForeignKey(x => x.IdVenta)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Bicicleta)
             .WithMany(b => b.Detalles)
             .HasForeignKey(x => x.IdBicicleta)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Vistas: entidades sin clave primaria, solo lectura.
        modelBuilder.Entity<InventarioBicicleta>(e =>
        {
            e.HasNoKey();
            e.ToView("vw_InventarioBicicletas");
            e.Property(x => x.Precio).HasColumnType("decimal(12,2)");
        });

        modelBuilder.Entity<HistorialVenta>(e =>
        {
            e.HasNoKey();
            e.ToView("vw_HistorialVentas");
            e.Property(x => x.Subtotal).HasColumnType("decimal(12,2)");
            e.Property(x => x.Iva).HasColumnType("decimal(12,2)");
            e.Property(x => x.Total).HasColumnType("decimal(12,2)");
        });

        base.OnModelCreating(modelBuilder);
    }
}
