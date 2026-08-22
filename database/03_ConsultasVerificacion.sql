/* ================================================================
   BikeStoreDB - Script 03: consultas de verificacion
   ----------------------------------------------------------------
   Autor : Ledesma Rodriguez Josue David
   ----------------------------------------------------------------
   Un bloque por cada uno de los diez escenarios funcionales
   obligatorios del enunciado. Sirve para dos cosas:

     1. Comprobar que el modelo de datos responde a todo lo pedido.
     2. Generar las capturas de pruebas del informe tecnico
        (entregable 4h).

   Los escenarios que MODIFICAN datos (1, 2, 3, 6 y 7) se ejecutan
   dentro de una transaccion que termina en ROLLBACK. Asi el script
   se puede correr las veces que haga falta sin ensuciar la base ni
   descuadrar el inventario: se ve el antes, el despues, y todo
   vuelve a su estado original.

   Ejecutar DESPUES de 01_BikeStoreDB.sql y 02_ObjetosAdicionales.sql
   ================================================================ */

USE BikeStoreDB;
GO
SET NOCOUNT ON;
GO

/* ================================================================
   ESCENARIO 4 - Consultar todas las bicicletas   [GET /api/bicicletas]
   ================================================================ */
PRINT '=== ESCENARIO 4: catalogo completo ===';
SELECT * FROM vw_InventarioBicicletas ORDER BY Categoria, Marca, Modelo;
GO

/* ================================================================
   ESCENARIO 5 - Buscar por categoria y marca
   [GET /api/bicicletas/buscar?idCategoria=1&marca=Trek]
   ================================================================ */
PRINT '=== ESCENARIO 5a: bicicletas de montana ===';
EXEC sp_BuscarBicicletas @IdCategoria = 1;

PRINT '=== ESCENARIO 5b: todo lo de la marca Trek ===';
EXEC sp_BuscarBicicletas @Marca = 'Trek';

PRINT '=== ESCENARIO 5c: busqueda combinada, solo disponibles ===';
EXEC sp_BuscarBicicletas @IdCategoria = 1, @Texto = 'Marlin', @SoloDisponibles = 1;
GO

/* ================================================================
   ESCENARIO 8 - Historial de ventas   [GET /api/ventas]
   ================================================================ */
PRINT '=== ESCENARIO 8: historial de ventas ===';
SELECT * FROM vw_HistorialVentas ORDER BY Fecha DESC;
GO

/* ================================================================
   ESCENARIO 9 - Ventas de un cliente   [GET /api/clientes/1/ventas]
   ================================================================ */
PRINT '=== ESCENARIO 9: compras del cliente 1 ===';
EXEC sp_VentasPorCliente @IdCliente = 1;
GO

/* ================================================================
   ESCENARIO 10 - Bicicletas con stock bajo o agotado
   [GET /api/bicicletas/stock-bajo]
   ================================================================ */
PRINT '=== ESCENARIO 10: inventario por reponer ===';
EXEC sp_BicicletasStockBajo;
GO

/* ================================================================
   DETALLE DE UNA VENTA   [GET /api/ventas/1]
   ================================================================ */
PRINT '=== Detalle de la venta 1 ===';
EXEC sp_ObtenerDetalleVenta @IdVenta = 1;
GO


/* ================================================================
   ESCENARIOS DE ESCRITURA
   Todo lo que sigue se revierte al final con ROLLBACK.
   ================================================================ */
BEGIN TRANSACTION;

/* ----------------------------------------------------------------
   ESCENARIO 1 - Registrar una nueva bicicleta   [POST /api/bicicletas]
   ---------------------------------------------------------------- */
PRINT '=== ESCENARIO 1: registrar bicicleta ===';

INSERT INTO Bicicleta (IdCategoria, Marca, Modelo, Descripcion, Precio, Stock, StockMinimo)
VALUES (1, 'Merida', 'Big Nine 20', 'MTB aro 29, cuadro de aluminio, frenos de disco', 980.00, 7, 3);

DECLARE @NuevaBici INT = SCOPE_IDENTITY();

SELECT 'Bicicleta registrada' AS Resultado, *
FROM   vw_InventarioBicicletas WHERE IdBicicleta = @NuevaBici;

/* ----------------------------------------------------------------
   ESCENARIO 2 - Actualizar precio y stock   [PUT /api/bicicletas/{id}]
   ---------------------------------------------------------------- */
PRINT '=== ESCENARIO 2: actualizar precio y stock ===';

SELECT 'ANTES' AS Momento, IdBicicleta, Marca, Modelo, Precio, Stock
FROM   Bicicleta WHERE IdBicicleta = 1;

UPDATE Bicicleta
SET    Precio = 1350.00,
       Stock  = 20
WHERE  IdBicicleta = 1;

SELECT 'DESPUES' AS Momento, IdBicicleta, Marca, Modelo, Precio, Stock
FROM   Bicicleta WHERE IdBicicleta = 1;

/* ----------------------------------------------------------------
   ESCENARIO 3 - Eliminar una bicicleta   [DELETE /api/bicicletas/{id}]

   Se prueban los dos casos:
     a) baja logica de un producto que ya tiene ventas
     b) borrado fisico de un producto sin movimientos
   ---------------------------------------------------------------- */
PRINT '=== ESCENARIO 3a: baja logica (producto con ventas) ===';

UPDATE Bicicleta SET Estado = 0 WHERE IdBicicleta = 1;

SELECT 'Producto dado de baja' AS Resultado, IdBicicleta, Marca, Modelo, Estado
FROM   Bicicleta WHERE IdBicicleta = 1;

PRINT '=== ESCENARIO 3b: borrado fisico (producto sin movimientos) ===';

DELETE FROM Bicicleta WHERE IdBicicleta = @NuevaBici;

SELECT 'Filas restantes con ese Id' AS Resultado, COUNT(*) AS Filas
FROM   Bicicleta WHERE IdBicicleta = @NuevaBici;

/* ----------------------------------------------------------------
   ESCENARIO 6 - Registrar un nuevo cliente   [POST /api/clientes]
   ---------------------------------------------------------------- */
PRINT '=== ESCENARIO 6: registrar cliente ===';

INSERT INTO Cliente (Cedula, Nombres, Apellidos, Telefono, Correo, Direccion)
VALUES ('1755512340', 'Josue David', 'Ledesma Rodriguez', '0987654321',
        'josue.ledesma@correo.com', 'Av. Amazonas N34-120, Quito');

DECLARE @NuevoCliente INT = SCOPE_IDENTITY();

SELECT 'Cliente registrado' AS Resultado, IdCliente, Cedula,
       Nombres + ' ' + Apellidos AS Cliente, Correo
FROM   Cliente WHERE IdCliente = @NuevoCliente;

/* ----------------------------------------------------------------
   ESCENARIO 7 - Registrar una venta con varios productos y
                 actualizar automaticamente el inventario
                 [POST /api/ventas]
   ---------------------------------------------------------------- */
PRINT '=== ESCENARIO 7: venta de varios productos ===';

SELECT 'STOCK ANTES' AS Momento, IdBicicleta, Marca, Modelo, Stock
FROM   Bicicleta WHERE IdBicicleta IN (2, 11);

DECLARE @Detalle TipoDetalleVenta;
DECLARE @IdVentaNueva INT;

INSERT INTO @Detalle (IdBicicleta, Cantidad) VALUES (2, 1), (11, 3);

EXEC sp_RegistrarVenta @IdCliente = @NuevoCliente,
                       @Detalle   = @Detalle,
                       @IdVenta   = @IdVentaNueva OUTPUT;

SELECT 'Venta generada' AS Resultado, * FROM vw_HistorialVentas WHERE IdVenta = @IdVentaNueva;

PRINT '--- Detalle de la venta generada ---';
EXEC sp_ObtenerDetalleVenta @IdVenta = @IdVentaNueva;

SELECT 'STOCK DESPUES' AS Momento, IdBicicleta, Marca, Modelo, Stock
FROM   Bicicleta WHERE IdBicicleta IN (2, 11);

ROLLBACK TRANSACTION;
PRINT '>>> ROLLBACK aplicado: la base queda en su estado original.';
GO


/* ================================================================
   PRUEBAS DE VALIDACION Y MANEJO DE ERRORES
   Comprueban que las reglas de negocio se cumplen a nivel de motor.
   Cada bloque debe FALLAR de forma controlada, con su mensaje.
   ================================================================ */

PRINT '=== VALIDACION 1: venta con stock insuficiente (error 50005) ===';
BEGIN TRY
    DECLARE @D1 TipoDetalleVenta;
    DECLARE @V1 INT;
    INSERT INTO @D1 (IdBicicleta, Cantidad) VALUES (6, 9999);
    EXEC sp_RegistrarVenta @IdCliente = 1, @Detalle = @D1, @IdVenta = @V1 OUTPUT;
    PRINT 'ERROR: la venta no debio registrarse.';
END TRY
BEGIN CATCH
    PRINT 'Rechazada correctamente -> ' + ERROR_MESSAGE();
END CATCH
GO

PRINT '=== VALIDACION 2: venta sin productos (error 50002) ===';
BEGIN TRY
    DECLARE @D2 TipoDetalleVenta;
    DECLARE @V2 INT;
    EXEC sp_RegistrarVenta @IdCliente = 1, @Detalle = @D2, @IdVenta = @V2 OUTPUT;
    PRINT 'ERROR: la venta no debio registrarse.';
END TRY
BEGIN CATCH
    PRINT 'Rechazada correctamente -> ' + ERROR_MESSAGE();
END CATCH
GO

PRINT '=== VALIDACION 3: cliente inexistente (error 50001) ===';
BEGIN TRY
    DECLARE @D3 TipoDetalleVenta;
    DECLARE @V3 INT;
    INSERT INTO @D3 (IdBicicleta, Cantidad) VALUES (1, 1);
    EXEC sp_RegistrarVenta @IdCliente = 9999, @Detalle = @D3, @IdVenta = @V3 OUTPUT;
    PRINT 'ERROR: la venta no debio registrarse.';
END TRY
BEGIN CATCH
    PRINT 'Rechazada correctamente -> ' + ERROR_MESSAGE();
END CATCH
GO

PRINT '=== VALIDACION 4: cedula con letras (CK_Cliente_Cedula) ===';
BEGIN TRY
    INSERT INTO Cliente (Cedula, Nombres, Apellidos) VALUES ('17ABC45678', 'Prueba', 'Invalida');
    PRINT 'ERROR: el cliente no debio insertarse.';
END TRY
BEGIN CATCH
    PRINT 'Rechazada correctamente -> ' + ERROR_MESSAGE();
END CATCH
GO

PRINT '=== VALIDACION 5: precio negativo (CK_Bicicleta_Precio) ===';
BEGIN TRY
    INSERT INTO Bicicleta (IdCategoria, Marca, Modelo, Precio, Stock)
    VALUES (1, 'Prueba', 'Precio negativo', -500.00, 5);
    PRINT 'ERROR: la bicicleta no debio insertarse.';
END TRY
BEGIN CATCH
    PRINT 'Rechazada correctamente -> ' + ERROR_MESSAGE();
END CATCH
GO

PRINT '=== VALIDACION 6: categoria con bicicletas asociadas (FK) ===';
BEGIN TRY
    DELETE FROM Categoria WHERE IdCategoria = 1;
    PRINT 'ERROR: la categoria no debio eliminarse.';
END TRY
BEGIN CATCH
    PRINT 'Rechazada correctamente -> ' + ERROR_MESSAGE();
END CATCH
GO

PRINT '';
PRINT '================================================================';
PRINT ' Verificacion completada. Los diez escenarios del enunciado';
PRINT ' quedan cubiertos y las validaciones rechazan lo que deben.';
PRINT '================================================================';
GO
