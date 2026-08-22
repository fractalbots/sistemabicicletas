/* ================================================================
   PROYECTO FINAL - SISTEMAS CLIENTE SERVIDOR
   BikeStoreDB - Script 02: objetos adicionales
   ----------------------------------------------------------------
   Autor : Ledesma Rodriguez Josue David
   Rol   : Base de datos / Arquitectura de datos / Integracion
   ----------------------------------------------------------------
   Complementa a 01_BikeStoreDB.sql con los objetos que faltaban
   para cubrir los escenarios funcionales obligatorios:

     - Indices de apoyo a las consultas 8, 9 y 10
     - Restriccion de formato para el telefono del cliente
     - sp_BuscarBicicletas    (escenarios 4 y 5)
     - sp_ObtenerDetalleVenta (detalle de una venta)

   El script es idempotente: se puede ejecutar varias veces sin
   error, porque cada objeto se verifica antes de crearse.

   Nota sobre los procedimientos: se usa DROP IF EXISTS + CREATE en
   lugar de CREATE OR ALTER. SQL Server resuelve los nombres que
   empiezan por 'sp_' buscando primero en la base master, y esa regla
   hace fallar la comprobacion de existencia de CREATE OR ALTER con el
   error 208. Con DROP + CREATE el resultado es el mismo y funciona en
   cualquier version.
   Ejecutar DESPUES de 01_BikeStoreDB.sql.
   ================================================================ */

USE BikeStoreDB;
GO

/* ----------------------------------------------------------------
   1. INDICES FALTANTES
   ---------------------------------------------------------------- */

-- Escenario 8: historial de ventas ordenado por fecha descendente.
-- El indice existente IX_Venta_Cliente encabeza por IdCliente, asi que
-- no sirve para recorrer todas las ventas por fecha.
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_Venta_Fecha' AND object_id = OBJECT_ID('Venta'))
    CREATE INDEX IX_Venta_Fecha ON Venta(Fecha DESC) INCLUDE (IdCliente, Total, Estado);
GO

-- Escenario 10: consulta de stock bajo y agotado sin recorrer toda la tabla.
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_Bicicleta_Stock' AND object_id = OBJECT_ID('Bicicleta'))
    CREATE INDEX IX_Bicicleta_Stock ON Bicicleta(Stock) INCLUDE (Marca, Modelo, StockMinimo, Estado);
GO

-- Historial de movimientos de un producto: sin este indice, buscar en que
-- ventas aparece una bicicleta obliga a un scan completo de DetalleVenta.
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_DetalleVenta_Bicicleta' AND object_id = OBJECT_ID('DetalleVenta'))
    CREATE INDEX IX_DetalleVenta_Bicicleta ON DetalleVenta(IdBicicleta) INCLUDE (IdVenta, Cantidad, PrecioUnitario);
GO

/* ----------------------------------------------------------------
   2. VALIDACION DE TELEFONO
   Solo digitos y longitud entre 7 y 15. Se declara WITH CHECK para
   que valide tambien las filas ya cargadas.
   ---------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Cliente_Telefono')
    ALTER TABLE Cliente WITH CHECK
        ADD CONSTRAINT CK_Cliente_Telefono
        CHECK (Telefono IS NULL
               OR (Telefono NOT LIKE '%[^0-9]%' AND LEN(Telefono) BETWEEN 7 AND 15));
GO

/* ----------------------------------------------------------------
   3. sp_BuscarBicicletas
      Escenarios obligatorios 4 y 5: consultar todas las bicicletas
      y buscarlas por nombre, categoria y marca.

      Los cuatro parametros son opcionales. El patron
      "@Parametro IS NULL OR columna = @Parametro" permite combinar
      cualquier subconjunto de filtros con una sola consulta, en
      lugar de escribir un procedimiento por combinacion.

      OPTION (RECOMPILE) evita el "parameter sniffing": obliga al
      motor a generar un plan adecuado a los filtros realmente
      enviados en cada llamada.
   ---------------------------------------------------------------- */
DROP PROCEDURE IF EXISTS dbo.sp_BuscarBicicletas;
GO

CREATE PROCEDURE dbo.sp_BuscarBicicletas
    @Texto           VARCHAR(100) = NULL,   -- busca en marca, modelo y descripcion
    @IdCategoria     INT          = NULL,
    @Marca           VARCHAR(60)  = NULL,
    @SoloDisponibles BIT          = 0       -- 1 = excluye las agotadas
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  IdBicicleta,
            Marca,
            Modelo,
            IdCategoria,
            Categoria,
            Precio,
            Stock,
            StockMinimo,
            EstadoStock,
            Estado
    FROM    vw_InventarioBicicletas
    WHERE   Estado = 1
      AND  (@IdCategoria IS NULL OR IdCategoria = @IdCategoria)
      AND  (@Marca       IS NULL OR Marca  LIKE '%' + @Marca + '%')
      AND  (@Texto       IS NULL OR Marca  LIKE '%' + @Texto + '%'
                                 OR Modelo LIKE '%' + @Texto + '%')
      AND  (@SoloDisponibles = 0 OR Stock > 0)
    ORDER BY Categoria, Marca, Modelo
    OPTION (RECOMPILE);
END
GO

/* ----------------------------------------------------------------
   4. sp_ObtenerDetalleVenta
      Devuelve las lineas de una venta con la descripcion del
      producto ya resuelta, para la pantalla de detalle y para el
      endpoint GET /api/ventas/{id}.
   ---------------------------------------------------------------- */
DROP PROCEDURE IF EXISTS dbo.sp_ObtenerDetalleVenta;
GO

CREATE PROCEDURE dbo.sp_ObtenerDetalleVenta
    @IdVenta INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Venta WHERE IdVenta = @IdVenta)
        THROW 50007, 'La venta solicitada no existe.', 1;

    SELECT  d.IdDetalle,
            d.IdVenta,
            v.Fecha,
            d.IdBicicleta,
            b.Marca,
            b.Modelo,
            c.Nombre AS Categoria,
            d.Cantidad,
            d.PrecioUnitario,
            d.Subtotal
    FROM    DetalleVenta d
            INNER JOIN Venta     v ON v.IdVenta     = d.IdVenta
            INNER JOIN Bicicleta b ON b.IdBicicleta = d.IdBicicleta
            INNER JOIN Categoria c ON c.IdCategoria = b.IdCategoria
    WHERE   d.IdVenta = @IdVenta
    ORDER BY d.IdDetalle;
END
GO

PRINT 'Script 02 ejecutado: indices, restriccion de telefono y procedimientos de busqueda.';
GO
