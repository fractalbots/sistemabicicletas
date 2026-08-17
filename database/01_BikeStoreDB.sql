/* ================================================================
   PROYECTO FINAL - SISTEMAS CLIENTE SERVIDOR
   Sistema de gestión BikeStore
   Motor: Microsoft SQL Server 2019 o superior
   Contenido: creación de BD, tablas, restricciones, índices,
              vistas, procedimientos almacenados y datos de prueba
   ================================================================ */

/* ----------------------------------------------------------------
   1. CREACIÓN DE LA BASE DE DATOS
   ---------------------------------------------------------------- */
USE master;
GO

IF DB_ID('BikeStoreDB') IS NOT NULL
BEGIN
    ALTER DATABASE BikeStoreDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BikeStoreDB;
END
GO

CREATE DATABASE BikeStoreDB;
GO

USE BikeStoreDB;
GO


/* ----------------------------------------------------------------
   2. TABLA: Categoria
   ---------------------------------------------------------------- */
CREATE TABLE Categoria
(
    IdCategoria   INT IDENTITY(1,1) NOT NULL,
    Nombre        VARCHAR(60)       NOT NULL,
    Descripcion   VARCHAR(200)      NULL,
    Activo        BIT               NOT NULL CONSTRAINT DF_Categoria_Activo DEFAULT (1),
    FechaRegistro DATETIME2(0)      NOT NULL CONSTRAINT DF_Categoria_Fecha  DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Categoria        PRIMARY KEY (IdCategoria),
    CONSTRAINT UQ_Categoria_Nombre UNIQUE      (Nombre)
);
GO


/* ----------------------------------------------------------------
   3. TABLA: Bicicleta
   ---------------------------------------------------------------- */
CREATE TABLE Bicicleta
(
    IdBicicleta   INT IDENTITY(1,1) NOT NULL,
    IdCategoria   INT               NOT NULL,
    Marca         VARCHAR(60)       NOT NULL,
    Modelo        VARCHAR(80)       NOT NULL,
    Descripcion   VARCHAR(250)      NULL,
    Precio        DECIMAL(12,2)     NOT NULL,
    Stock         INT               NOT NULL CONSTRAINT DF_Bicicleta_Stock    DEFAULT (0),
    StockMinimo   INT               NOT NULL CONSTRAINT DF_Bicicleta_StockMin DEFAULT (5),
    Estado        BIT               NOT NULL CONSTRAINT DF_Bicicleta_Estado   DEFAULT (1),
    FechaRegistro DATETIME2(0)      NOT NULL CONSTRAINT DF_Bicicleta_Fecha    DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Bicicleta             PRIMARY KEY (IdBicicleta),
    CONSTRAINT FK_Bicicleta_Categoria   FOREIGN KEY (IdCategoria)
        REFERENCES Categoria(IdCategoria),
    CONSTRAINT UQ_Bicicleta_MarcaModelo UNIQUE (Marca, Modelo),
    CONSTRAINT CK_Bicicleta_Precio      CHECK (Precio > 0),
    CONSTRAINT CK_Bicicleta_Stock       CHECK (Stock >= 0),
    CONSTRAINT CK_Bicicleta_StockMinimo CHECK (StockMinimo >= 0)
);
GO


/* ----------------------------------------------------------------
   4. TABLA: Cliente
   ---------------------------------------------------------------- */
CREATE TABLE Cliente
(
    IdCliente     INT IDENTITY(1,1) NOT NULL,
    Cedula        CHAR(10)          NOT NULL,
    Nombres       VARCHAR(80)       NOT NULL,
    Apellidos     VARCHAR(80)       NOT NULL,
    Telefono      VARCHAR(15)       NULL,
    Correo        VARCHAR(120)      NULL,
    Direccion     VARCHAR(200)      NULL,
    Activo        BIT               NOT NULL CONSTRAINT DF_Cliente_Activo DEFAULT (1),
    FechaRegistro DATETIME2(0)      NOT NULL CONSTRAINT DF_Cliente_Fecha  DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Cliente        PRIMARY KEY (IdCliente),
    CONSTRAINT UQ_Cliente_Cedula UNIQUE      (Cedula),
    CONSTRAINT CK_Cliente_Cedula CHECK (Cedula NOT LIKE '%[^0-9]%' AND LEN(Cedula) = 10),
    CONSTRAINT CK_Cliente_Correo CHECK (Correo IS NULL OR Correo LIKE '%_@_%._%')
);
GO


/* ----------------------------------------------------------------
   5. TABLA: Venta
   ---------------------------------------------------------------- */
CREATE TABLE Venta
(
    IdVenta       INT IDENTITY(1,1) NOT NULL,
    IdCliente     INT               NOT NULL,
    Fecha         DATETIME2(0)      NOT NULL CONSTRAINT DF_Venta_Fecha  DEFAULT (SYSDATETIME()),
    Subtotal      DECIMAL(12,2)     NOT NULL CONSTRAINT DF_Venta_Subtotal DEFAULT (0),
    PorcentajeIva DECIMAL(5,2)      NOT NULL CONSTRAINT DF_Venta_PorcIva  DEFAULT (15.00),
    Iva           DECIMAL(12,2)     NOT NULL CONSTRAINT DF_Venta_Iva      DEFAULT (0),
    Total         DECIMAL(12,2)     NOT NULL CONSTRAINT DF_Venta_Total    DEFAULT (0),
    Estado        VARCHAR(15)       NOT NULL CONSTRAINT DF_Venta_Estado   DEFAULT ('EMITIDA'),

    CONSTRAINT PK_Venta           PRIMARY KEY (IdVenta),
    CONSTRAINT FK_Venta_Cliente   FOREIGN KEY (IdCliente) REFERENCES Cliente(IdCliente),
    CONSTRAINT CK_Venta_Montos    CHECK (Subtotal >= 0 AND Iva >= 0 AND Total >= 0),
    CONSTRAINT CK_Venta_Estado    CHECK (Estado IN ('EMITIDA','ANULADA'))
);
GO


/* ----------------------------------------------------------------
   6. TABLA: DetalleVenta
   ---------------------------------------------------------------- */
CREATE TABLE DetalleVenta
(
    IdDetalle      INT IDENTITY(1,1) NOT NULL,
    IdVenta        INT               NOT NULL,
    IdBicicleta    INT               NOT NULL,
    Cantidad       INT               NOT NULL,
    PrecioUnitario DECIMAL(12,2)     NOT NULL,
    Subtotal       AS (CONVERT(DECIMAL(12,2), Cantidad * PrecioUnitario)) PERSISTED,

    CONSTRAINT PK_DetalleVenta            PRIMARY KEY (IdDetalle),
    CONSTRAINT FK_DetalleVenta_Venta      FOREIGN KEY (IdVenta)
        REFERENCES Venta(IdVenta) ON DELETE CASCADE,
    CONSTRAINT FK_DetalleVenta_Bicicleta  FOREIGN KEY (IdBicicleta)
        REFERENCES Bicicleta(IdBicicleta),
    CONSTRAINT CK_DetalleVenta_Cantidad   CHECK (Cantidad > 0),
    CONSTRAINT CK_DetalleVenta_Precio     CHECK (PrecioUnitario > 0),
    CONSTRAINT UQ_DetalleVenta_Item       UNIQUE (IdVenta, IdBicicleta)
);
GO


/* ----------------------------------------------------------------
   7. ÍNDICES PARA LAS BÚSQUEDAS DEL ENUNCIADO
   ---------------------------------------------------------------- */
CREATE INDEX IX_Bicicleta_Categoria ON Bicicleta(IdCategoria) INCLUDE (Marca, Modelo, Precio, Stock);
CREATE INDEX IX_Bicicleta_Marca     ON Bicicleta(Marca);
CREATE INDEX IX_Bicicleta_Modelo    ON Bicicleta(Modelo);
CREATE INDEX IX_Cliente_Apellidos   ON Cliente(Apellidos);
CREATE INDEX IX_Venta_Cliente       ON Venta(IdCliente, Fecha DESC);
CREATE INDEX IX_DetalleVenta_Venta  ON DetalleVenta(IdVenta);
GO


/* ----------------------------------------------------------------
   8. VISTAS DE CONSULTA
   ---------------------------------------------------------------- */

-- Catálogo con estado de inventario calculado
CREATE VIEW vw_InventarioBicicletas
AS
SELECT  b.IdBicicleta,
        b.Marca,
        b.Modelo,
        c.IdCategoria,
        c.Nombre AS Categoria,
        b.Precio,
        b.Stock,
        b.StockMinimo,
        EstadoStock = CASE
                        WHEN b.Stock = 0             THEN 'AGOTADO'
                        WHEN b.Stock <= b.StockMinimo THEN 'STOCK BAJO'
                        ELSE 'DISPONIBLE'
                      END,
        b.Estado
FROM    Bicicleta b
        INNER JOIN Categoria c ON c.IdCategoria = b.IdCategoria;
GO

-- Historial de ventas con datos del cliente
CREATE VIEW vw_HistorialVentas
AS
SELECT  v.IdVenta,
        v.Fecha,
        cl.IdCliente,
        cl.Cedula,
        Cliente     = cl.Nombres + ' ' + cl.Apellidos,
        NumItems    = (SELECT COUNT(*) FROM DetalleVenta d WHERE d.IdVenta = v.IdVenta),
        v.Subtotal,
        v.Iva,
        v.Total,
        v.Estado
FROM    Venta v
        INNER JOIN Cliente cl ON cl.IdCliente = v.IdCliente;
GO


/* ----------------------------------------------------------------
   9. TIPO DE TABLA PARA EL DETALLE DE VENTA
   ---------------------------------------------------------------- */
CREATE TYPE TipoDetalleVenta AS TABLE
(
    IdBicicleta INT NOT NULL,
    Cantidad    INT NOT NULL
);
GO


/* ----------------------------------------------------------------
   10. PROCEDIMIENTO: sp_RegistrarVenta
       Registra la cabecera y el detalle, valida stock y actualiza
       el inventario dentro de una única transacción atómica.
   ---------------------------------------------------------------- */
CREATE PROCEDURE sp_RegistrarVenta
    @IdCliente INT,
    @Detalle   TipoDetalleVenta READONLY,
    @IdVenta   INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        /* --- Validaciones previas --- */
        IF NOT EXISTS (SELECT 1 FROM Cliente WHERE IdCliente = @IdCliente AND Activo = 1)
            THROW 50001, 'El cliente no existe o se encuentra inactivo.', 1;

        IF NOT EXISTS (SELECT 1 FROM @Detalle)
            THROW 50002, 'La venta debe contener al menos un producto.', 1;

        IF EXISTS (SELECT 1 FROM @Detalle WHERE Cantidad <= 0)
            THROW 50003, 'Las cantidades deben ser mayores a cero.', 1;

        IF EXISTS (SELECT 1 FROM @Detalle d
                   LEFT JOIN Bicicleta b ON b.IdBicicleta = d.IdBicicleta AND b.Estado = 1
                   WHERE b.IdBicicleta IS NULL)
            THROW 50004, 'Una o más bicicletas no existen o están inactivas.', 1;

        BEGIN TRANSACTION;

            /* Bloqueo de las filas de inventario involucradas */
            DECLARE @Faltante VARCHAR(200);

            SELECT TOP 1 @Faltante = b.Marca + ' ' + b.Modelo
            FROM   @Detalle d
                   INNER JOIN Bicicleta b WITH (UPDLOCK, HOLDLOCK)
                        ON b.IdBicicleta = d.IdBicicleta
            WHERE  b.Stock < d.Cantidad;

            IF @Faltante IS NOT NULL
                THROW 50005, 'Stock insuficiente para completar la venta.', 1;

            /* Cabecera */
            INSERT INTO Venta (IdCliente) VALUES (@IdCliente);
            SET @IdVenta = SCOPE_IDENTITY();

            /* Detalle: el precio se toma de la BD, nunca del cliente */
            INSERT INTO DetalleVenta (IdVenta, IdBicicleta, Cantidad, PrecioUnitario)
            SELECT @IdVenta, d.IdBicicleta, d.Cantidad, b.Precio
            FROM   @Detalle d
                   INNER JOIN Bicicleta b ON b.IdBicicleta = d.IdBicicleta;

            /* Actualización automática del inventario */
            UPDATE b
            SET    b.Stock = b.Stock - d.Cantidad
            FROM   Bicicleta b
                   INNER JOIN @Detalle d ON d.IdBicicleta = b.IdBicicleta;

            /* Cálculo de subtotal, IVA y total */
            DECLARE @Subtotal DECIMAL(12,2), @PorcIva DECIMAL(5,2);

            SELECT @Subtotal = SUM(Subtotal) FROM DetalleVenta WHERE IdVenta = @IdVenta;
            SELECT @PorcIva  = PorcentajeIva FROM Venta        WHERE IdVenta = @IdVenta;

            UPDATE Venta
            SET    Subtotal = @Subtotal,
                   Iva      = ROUND(@Subtotal * @PorcIva / 100, 2),
                   Total    = @Subtotal + ROUND(@Subtotal * @PorcIva / 100, 2)
            WHERE  IdVenta = @IdVenta;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


/* ----------------------------------------------------------------
   11. PROCEDIMIENTO: sp_AnularVenta
       Revierte una venta y devuelve el stock al inventario.
   ---------------------------------------------------------------- */
CREATE PROCEDURE sp_AnularVenta
    @IdVenta INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM Venta WHERE IdVenta = @IdVenta AND Estado = 'EMITIDA')
            THROW 50006, 'La venta no existe o ya fue anulada.', 1;

        BEGIN TRANSACTION;

            UPDATE b
            SET    b.Stock = b.Stock + d.Cantidad
            FROM   Bicicleta b
                   INNER JOIN DetalleVenta d ON d.IdBicicleta = b.IdBicicleta
            WHERE  d.IdVenta = @IdVenta;

            UPDATE Venta SET Estado = 'ANULADA' WHERE IdVenta = @IdVenta;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


/* ----------------------------------------------------------------
   12. PROCEDIMIENTO: sp_BicicletasStockBajo
   ---------------------------------------------------------------- */
CREATE PROCEDURE sp_BicicletasStockBajo
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_InventarioBicicletas
    WHERE  EstadoStock IN ('STOCK BAJO','AGOTADO') AND Estado = 1
    ORDER  BY Stock ASC;
END
GO


/* ----------------------------------------------------------------
   13. PROCEDIMIENTO: sp_VentasPorCliente
   ---------------------------------------------------------------- */
CREATE PROCEDURE sp_VentasPorCliente
    @IdCliente INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_HistorialVentas
    WHERE  IdCliente = @IdCliente
    ORDER  BY Fecha DESC;
END
GO


/* ================================================================
   14. DATOS DE PRUEBA
   ================================================================ */

/* --- Categorías --- */
INSERT INTO Categoria (Nombre, Descripcion) VALUES
('Montaña',    'Bicicletas todo terreno con suspensión y llantas anchas'),
('Ruta',       'Bicicletas ligeras de alta velocidad para asfalto'),
('BMX',        'Bicicletas acrobáticas de cuadro reforzado'),
('Eléctricas', 'Bicicletas con motor eléctrico asistido y batería'),
('Infantiles', 'Bicicletas para niños de 3 a 12 años');
GO

/* --- Bicicletas --- */
INSERT INTO Bicicleta (IdCategoria, Marca, Modelo, Descripcion, Precio, Stock, StockMinimo) VALUES
(1, 'Trek',      'Marlin 7',        'MTB aro 29, cuadro de aluminio, 12 velocidades', 1250.00, 12,  4),
(1, 'Specialized','Rockhopper 29',  'MTB aro 29, frenos de disco hidráulicos',        1180.00,  8,  4),
(1, 'GW',        'Alligator 27.5',  'MTB aro 27.5, doble suspensión',                  620.00,  3,  5),
(2, 'Giant',     'Contend AR 3',    'Ruta, cuadro ALUXX, grupo Shimano Sora',         1450.00,  6,  3),
(2, 'Scott',     'Speedster 40',    'Ruta de aluminio, horquilla de carbono',         1320.00,  4,  3),
(2, 'Cannondale','CAAD Optimo 3',   'Ruta de competencia, grupo Shimano Tiagra',      1890.00,  2,  3),
(3, 'Mongoose',  'Legion L20',      'BMX freestyle aro 20, cuadro Hi-Ten',             430.00, 15,  5),
(3, 'GT',        'Performer 21',    'BMX old school aro 21, rotor 360°',               510.00,  0,  4),
(4, 'Bianchi',   'E-Omnia C',       'Eléctrica urbana, motor 250W, autonomía 90 km',  2790.00,  5,  2),
(4, 'Haibike',   'Trekking 5',      'Eléctrica de trekking, batería 630Wh',           3150.00,  3,  2),
(5, 'Vertigo',   'Kids 16',         'Infantil aro 16 con ruedas de apoyo',             185.00, 20,  6),
(5, 'Shimano',   'Junior 20',       'Infantil aro 20, 6 velocidades',                  240.00,  9,  6);
GO

/* --- Clientes --- */
INSERT INTO Cliente (Cedula, Nombres, Apellidos, Telefono, Correo, Direccion) VALUES
('1723456789', 'Jorge Eduardo', 'Chapaca Garzón', '0991234567', 'jorge.chapaca@correo.com',  'Av. Amazonas N32-45, Quito'),
('1712345678', 'María Fernanda','Andrade López',  '0987654321', 'maria.andrade@correo.com',  'Av. 6 de Diciembre y Portugal, Quito'),
('0923456781', 'Luis Alberto',  'Vera Mendoza',   '0961122334', 'luis.vera@correo.com',      'Cdla. Kennedy Norte, Guayaquil'),
('1104567892', 'Ana Cristina',  'Suárez Pineda',  '0975566778', 'ana.suarez@correo.com',     'Av. Loja y Cuenca, Loja'),
('0604567893', 'Carlos Andrés', 'Moreno Ríos',    '0998877665', 'carlos.moreno@correo.com',  'Av. Daniel León Borja, Riobamba'),
('1798765432', 'Gabriela Paz',  'Terán Ocaña',    '0954433221', 'gabriela.teran@correo.com', 'Valle de los Chillos, Quito');
GO

/* --- Ventas de prueba (usando el procedimiento almacenado) --- */
DECLARE @Detalle TipoDetalleVenta;
DECLARE @NuevaVenta INT;

-- Venta 1
INSERT INTO @Detalle (IdBicicleta, Cantidad) VALUES (1, 1), (11, 2);
EXEC sp_RegistrarVenta @IdCliente = 1, @Detalle = @Detalle, @IdVenta = @NuevaVenta OUTPUT;
DELETE FROM @Detalle;

-- Venta 2
INSERT INTO @Detalle (IdBicicleta, Cantidad) VALUES (9, 1);
EXEC sp_RegistrarVenta @IdCliente = 2, @Detalle = @Detalle, @IdVenta = @NuevaVenta OUTPUT;
DELETE FROM @Detalle;

-- Venta 3
INSERT INTO @Detalle (IdBicicleta, Cantidad) VALUES (4, 1), (7, 3), (12, 1);
EXEC sp_RegistrarVenta @IdCliente = 1, @Detalle = @Detalle, @IdVenta = @NuevaVenta OUTPUT;
DELETE FROM @Detalle;

-- Venta 4
INSERT INTO @Detalle (IdBicicleta, Cantidad) VALUES (5, 2);
EXEC sp_RegistrarVenta @IdCliente = 4, @Detalle = @Detalle, @IdVenta = @NuevaVenta OUTPUT;
GO


/* ================================================================
   15. CONSULTAS DE VERIFICACIÓN (para las capturas del informe)
   ================================================================ */

-- Inventario completo con estado calculado
SELECT * FROM vw_InventarioBicicletas ORDER BY Categoria, Marca;

-- Bicicletas con stock bajo o agotado
EXEC sp_BicicletasStockBajo;

-- Historial de ventas
SELECT * FROM vw_HistorialVentas ORDER BY Fecha DESC;

-- Ventas de un cliente específico
EXEC sp_VentasPorCliente @IdCliente = 1;

-- Detalle completo de una venta
SELECT  v.IdVenta, v.Fecha, b.Marca, b.Modelo,
        d.Cantidad, d.PrecioUnitario, d.Subtotal,
        v.Subtotal AS SubtotalVenta, v.Iva, v.Total
FROM    Venta v
        INNER JOIN DetalleVenta d ON d.IdVenta = v.IdVenta
        INNER JOIN Bicicleta   b ON b.IdBicicleta = d.IdBicicleta
WHERE   v.IdVenta = 1;

-- Búsqueda por categoría y marca (escenario obligatorio 5)
SELECT * FROM vw_InventarioBicicletas
WHERE  Categoria = 'Montaña' AND Marca LIKE '%Trek%';
GO
