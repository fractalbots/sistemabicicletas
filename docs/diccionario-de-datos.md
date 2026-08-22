# Diccionario de Datos — BikeStoreDB

**Base de datos:** `BikeStoreDB` · **Motor:** Microsoft SQL Server 2019+ · **Esquema:** `dbo`
**Responsable:** Ledesma Rodríguez Josué David — *Base de datos / Arquitectura de datos*
**Corresponde a:** `database/01_BikeStoreDB.sql`

**Convenciones:** las tablas se nombran en singular; las claves primarias siguen el patrón `Id<Entidad>`; `PK` = clave primaria, `FK` = clave foránea, `UK` = clave única.

---

## Tabla `Categoria`

Clasificación del catálogo de productos (Montaña, Ruta, BMX, Eléctricas, Infantiles).

| Campo | Tipo | Nulo | Clave | Valor por defecto | Descripción |
|---|---|---|---|---|---|
| `IdCategoria` | `INT IDENTITY(1,1)` | No | PK | autoincremental | Identificador único de la categoría. |
| `Nombre` | `VARCHAR(60)` | No | UK | — | Nombre de la categoría. No se repite. |
| `Descripcion` | `VARCHAR(200)` | Sí | — | `NULL` | Texto descriptivo mostrado en el catálogo. |
| `Activo` | `BIT` | No | — | `1` | Baja lógica. `0` la oculta sin borrar sus bicicletas. |
| `FechaRegistro` | `DATETIME2(0)` | No | — | `SYSDATETIME()` | Fecha de alta del registro. |

**Restricciones:** `PK_Categoria` · `UQ_Categoria_Nombre`

---

## Tabla `Bicicleta`

Producto que comercializa la tienda. Concentra el inventario.

| Campo | Tipo | Nulo | Clave | Valor por defecto | Descripción |
|---|---|---|---|---|---|
| `IdBicicleta` | `INT IDENTITY(1,1)` | No | PK | autoincremental | Identificador único del producto. |
| `IdCategoria` | `INT` | No | FK → `Categoria` | — | Categoría a la que pertenece. |
| `Marca` | `VARCHAR(60)` | No | UK* | — | Fabricante (Trek, Giant, Scott…). |
| `Modelo` | `VARCHAR(80)` | No | UK* | — | Modelo comercial. |
| `Descripcion` | `VARCHAR(250)` | Sí | — | `NULL` | Ficha técnica resumida. |
| `Precio` | `DECIMAL(12,2)` | No | — | — | Precio de venta al público, en USD. Debe ser > 0. |
| `Stock` | `INT` | No | — | `0` | Unidades disponibles. Nunca negativo. |
| `StockMinimo` | `INT` | No | — | `5` | Umbral de reposición del producto. |
| `Estado` | `BIT` | No | — | `1` | Baja lógica del producto. |
| `FechaRegistro` | `DATETIME2(0)` | No | — | `SYSDATETIME()` | Fecha de alta en el catálogo. |

\* `UQ_Bicicleta_MarcaModelo` es una clave única **compuesta** sobre (`Marca`, `Modelo`): impide cargar dos veces el mismo producto.

**Restricciones:** `PK_Bicicleta` · `FK_Bicicleta_Categoria` · `UQ_Bicicleta_MarcaModelo` · `CK_Bicicleta_Precio` (`Precio > 0`) · `CK_Bicicleta_Stock` (`Stock >= 0`) · `CK_Bicicleta_StockMinimo` (`StockMinimo >= 0`)

---

## Tabla `Cliente`

Personas que realizan compras en la tienda.

| Campo | Tipo | Nulo | Clave | Valor por defecto | Descripción |
|---|---|---|---|---|---|
| `IdCliente` | `INT IDENTITY(1,1)` | No | PK | autoincremental | Identificador único del cliente. |
| `Cedula` | `CHAR(10)` | No | UK | — | Cédula ecuatoriana. Exactamente 10 dígitos. |
| `Nombres` | `VARCHAR(80)` | No | — | — | Nombres del cliente. |
| `Apellidos` | `VARCHAR(80)` | No | — | — | Apellidos del cliente. |
| `Telefono` | `VARCHAR(15)` | Sí | — | `NULL` | Teléfono de contacto. |
| `Correo` | `VARCHAR(120)` | Sí | — | `NULL` | Correo electrónico. Formato validado. |
| `Direccion` | `VARCHAR(200)` | Sí | — | `NULL` | Dirección de domicilio o entrega. |
| `Activo` | `BIT` | No | — | `1` | Baja lógica; conserva el historial de ventas. |
| `FechaRegistro` | `DATETIME2(0)` | No | — | `SYSDATETIME()` | Fecha de alta del cliente. |

**Restricciones:** `PK_Cliente` · `UQ_Cliente_Cedula` · `CK_Cliente_Cedula` (`NOT LIKE '%[^0-9]%' AND LEN = 10`) · `CK_Cliente_Correo` (`LIKE '%_@_%._%'`)

---

## Tabla `Venta`

Cabecera del documento de venta. Guarda los totales ya calculados.

| Campo | Tipo | Nulo | Clave | Valor por defecto | Descripción |
|---|---|---|---|---|---|
| `IdVenta` | `INT IDENTITY(1,1)` | No | PK | autoincremental | Número de la venta. |
| `IdCliente` | `INT` | No | FK → `Cliente` | — | Cliente que realiza la compra. |
| `Fecha` | `DATETIME2(0)` | No | — | `SYSDATETIME()` | Fecha y hora de emisión. |
| `Subtotal` | `DECIMAL(12,2)` | No | — | `0` | Suma de los subtotales del detalle, sin IVA. |
| `PorcentajeIva` | `DECIMAL(5,2)` | No | — | `15.00` | Tarifa de IVA aplicada en esa fecha. |
| `Iva` | `DECIMAL(12,2)` | No | — | `0` | Valor del IVA: `ROUND(Subtotal * PorcentajeIva / 100, 2)`. |
| `Total` | `DECIMAL(12,2)` | No | — | `0` | `Subtotal + Iva`. Valor efectivamente cobrado. |
| `Estado` | `VARCHAR(15)` | No | — | `'EMITIDA'` | `EMITIDA` o `ANULADA`. |

**Restricciones:** `PK_Venta` · `FK_Venta_Cliente` · `CK_Venta_Montos` (`Subtotal, Iva, Total >= 0`) · `CK_Venta_Estado` (`IN ('EMITIDA','ANULADA')`)

---

## Tabla `DetalleVenta`

Líneas del documento. Tabla asociativa entre `Venta` y `Bicicleta`.

| Campo | Tipo | Nulo | Clave | Valor por defecto | Descripción |
|---|---|---|---|---|---|
| `IdDetalle` | `INT IDENTITY(1,1)` | No | PK | autoincremental | Identificador de la línea. |
| `IdVenta` | `INT` | No | FK → `Venta` | — | Venta a la que pertenece. `ON DELETE CASCADE`. |
| `IdBicicleta` | `INT` | No | FK → `Bicicleta` | — | Producto vendido. |
| `Cantidad` | `INT` | No | — | — | Unidades vendidas. Debe ser > 0. |
| `PrecioUnitario` | `DECIMAL(12,2)` | No | — | — | Precio congelado al momento de la venta. |
| `Subtotal` | `DECIMAL(12,2)` | No | — | **calculado** | `Cantidad * PrecioUnitario`, columna `PERSISTED`. |

**Restricciones:** `PK_DetalleVenta` · `FK_DetalleVenta_Venta` (CASCADE) · `FK_DetalleVenta_Bicicleta` (RESTRICT) · `UQ_DetalleVenta_Item` (`IdVenta`, `IdBicicleta`) · `CK_DetalleVenta_Cantidad` (`> 0`) · `CK_DetalleVenta_Precio` (`> 0`)

> `UQ_DetalleVenta_Item` obliga a que un producto aparezca **una sola vez por venta**: si el cliente lleva 3 unidades, es una línea con `Cantidad = 3`, no tres líneas de 1.

---

## Vistas

### `vw_InventarioBicicletas`
Catálogo con el nombre de la categoría resuelto y una etiqueta de inventario calculada.

| Campo | Origen | Descripción |
|---|---|---|
| `IdBicicleta`, `Marca`, `Modelo`, `Precio`, `Stock`, `StockMinimo`, `Estado` | `Bicicleta` | Datos del producto. |
| `IdCategoria`, `Categoria` | `Categoria` | Categoría resuelta por JOIN. |
| `EstadoStock` | calculado | `AGOTADO` (Stock = 0) · `STOCK BAJO` (Stock ≤ StockMinimo) · `DISPONIBLE`. |

### `vw_HistorialVentas`
Cabeceras de venta con los datos del cliente y el número de ítems.

| Campo | Origen | Descripción |
|---|---|---|
| `IdVenta`, `Fecha`, `Subtotal`, `Iva`, `Total`, `Estado` | `Venta` | Datos del documento. |
| `IdCliente`, `Cedula`, `Cliente` | `Cliente` | `Cliente` = `Nombres + ' ' + Apellidos`. |
| `NumItems` | subconsulta | Número de líneas de la venta. |

---

## Tipo de tabla

### `TipoDetalleVenta`
Parámetro con valores de tabla (*table-valued parameter*). Permite enviar el detalle completo de una venta en **una sola llamada** desde la capa de datos, en lugar de un `INSERT` por línea.

| Campo | Tipo | Descripción |
|---|---|---|
| `IdBicicleta` | `INT NOT NULL` | Producto a vender. |
| `Cantidad` | `INT NOT NULL` | Unidades solicitadas. |

---

## Procedimientos almacenados

| Procedimiento | Parámetros | Descripción |
|---|---|---|
| `sp_RegistrarVenta` | `@IdCliente INT`, `@Detalle TipoDetalleVenta READONLY`, `@IdVenta INT OUTPUT` | Valida, inserta cabecera y detalle, descuenta stock y calcula totales en una única transacción atómica. Devuelve el número de venta generado. |
| `sp_AnularVenta` | `@IdVenta INT` | Marca la venta como `ANULADA` y devuelve las unidades al inventario. |
| `sp_BicicletasStockBajo` | — | Productos con `EstadoStock` en (`STOCK BAJO`, `AGOTADO`), ordenados por stock ascendente. |
| `sp_VentasPorCliente` | `@IdCliente INT` | Historial de compras de un cliente, más reciente primero. |
| `sp_BuscarBicicletas` | `@Texto VARCHAR(100) = NULL`, `@IdCategoria INT = NULL`, `@Marca VARCHAR(60) = NULL`, `@SoloDisponibles BIT = 0` | Búsqueda combinada del catálogo. Todos los filtros son opcionales y se combinan con AND. |
| `sp_ObtenerDetalleVenta` | `@IdVenta INT` | Líneas de una venta con la descripción del producto. |

---

## Códigos de error del negocio

Los procedimientos lanzan errores numerados con `THROW`. La capa de datos los captura como `SqlException` y la API los traduce a códigos HTTP:

| Código | Mensaje | HTTP |
|---|---|---|
| `50001` | El cliente no existe o se encuentra inactivo. | 400 |
| `50002` | La venta debe contener al menos un producto. | 400 |
| `50003` | Las cantidades deben ser mayores a cero. | 400 |
| `50004` | Una o más bicicletas no existen o están inactivas. | 400 |
| `50005` | Stock insuficiente para completar la venta. | 409 |
| `50006` | La venta no existe o ya fue anulada. | 404 |
