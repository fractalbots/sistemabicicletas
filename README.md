# BikeStore — Sistema de gestión para tienda de bicicletas

Proyecto final de **Sistemas Cliente Servidor**. Solución web en ASP.NET Core con API RESTful y base de datos SQL Server, desarrollada bajo una arquitectura por capas.

---

## Estructura del repositorio

```
sistemabicicletas/
├── database/
│   ├── 01_BikeStoreDB.sql          Base, tablas, vistas, procedimientos y datos de prueba
│   ├── 02_ObjetosAdicionales.sql   Índices, validaciones y procedimientos de búsqueda
│   └── 03_ConsultasVerificacion.sql Pruebas de los 10 escenarios del enunciado
├── docs/
│   ├── modelo-entidad-relacion.md  MER, cardinalidades y decisiones de diseño
│   ├── diccionario-de-datos.md     Diccionario completo de tablas, vistas y procedimientos
│   └── arquitectura-de-datos.md    Diagrama de capas y recorrido de una venta
├── src/
│   ├── BikeStore.Domain/           Entidades, DTOs e interfaces
│   ├── BikeStore.Data/             EF Core, repositorios, procedimientos almacenados
│   ├── BikeStore.API/              API RESTful
│   └── BikeStore.Web/              Sitio web ASP.NET Core MVC
└── BikeStore.slnx
```

---

## Requisitos

- .NET SDK 8.0 o superior
- SQL Server 2019+ o SQL Server LocalDB (viene con Visual Studio)
- SQL Server Management Studio o Azure Data Studio

---

## 1. Crear la base de datos

Abrir SSMS, conectarse al servidor y ejecutar **en este orden**:

```
database/01_BikeStoreDB.sql
database/02_ObjetosAdicionales.sql
```

> El script 01 **elimina `BikeStoreDB` si ya existe** y la vuelve a crear desde cero. No ejecutarlo sobre una base con datos que se quieran conservar.

Para comprobar que quedó bien y generar las capturas del informe:

```
database/03_ConsultasVerificacion.sql
```

Ese script recorre los diez escenarios funcionales del enunciado y revierte con `ROLLBACK` todo lo que escribe, así que se puede correr las veces que haga falta.

---

## 2. Configurar la cadena de conexión

En `src/BikeStore.API/appsettings.json`:

```json
"ConnectionStrings": {
  "BikeStoreDB": "Server=(localdb)\\MSSQLLocalDB;Database=BikeStoreDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
```

Si se usa una instancia con usuario y contraseña en lugar de LocalDB:

```
Server=localhost;Database=BikeStoreDB;User Id=sa;Password=TU_CLAVE;TrustServerCertificate=True
```

> No subir contraseñas reales al repositorio. Para eso están los *user secrets*: `dotnet user-secrets set "ConnectionStrings:BikeStoreDB" "..."`.

---

## 3. Levantar la solución

```bash
# API RESTful      -> https://localhost:7001
cd src/BikeStore.API
dotnet run

# Sitio web        -> https://localhost:7002
cd src/BikeStore.Web
dotnet run
```

La API expone su documentación interactiva en `/swagger`.

`BikeStore.Web` consume la API por HTTP y necesita conocer su dirección, en `src/BikeStore.Web/appsettings.json`:

```json
"ApiSettings": {
  "BaseUrl": "https://localhost:7001/"
}
```

---

## Servicios web

| Método | Endpoint | Descripción |
|---|---|---|
| `GET` | `/api/bicicletas` | Consultar todas las bicicletas |
| `GET` | `/api/bicicletas/{id}` | Consultar una bicicleta |
| `GET` | `/api/bicicletas/buscar` | Buscar por texto, categoría y marca |
| `GET` | `/api/bicicletas/inventario` | Catálogo con estado de stock calculado |
| `GET` | `/api/bicicletas/stock-bajo` | Inventario por reponer |
| `POST` | `/api/bicicletas` | Registrar una bicicleta |
| `PUT` | `/api/bicicletas/{id}` | Actualizar una bicicleta |
| `DELETE` | `/api/bicicletas/{id}` | Eliminar una bicicleta |
| `GET/POST/PUT/DELETE` | `/api/categorias` | CRUD de categorías |
| `GET/POST/PUT/DELETE` | `/api/clientes` | CRUD de clientes |
| `GET` | `/api/clientes/cedula/{cedula}` | Buscar cliente por cédula |
| `GET` | `/api/clientes/{id}/ventas` | Ventas de un cliente |
| `GET` | `/api/ventas` | Historial de ventas |
| `GET` | `/api/ventas/{id}` | Detalle de una venta |
| `POST` | `/api/ventas` | Registrar una venta |
| `POST` | `/api/ventas/{id}/anular` | Anular una venta y devolver el stock |

---

## Modelo de datos

Cinco tablas: `Categoria`, `Bicicleta`, `Cliente`, `Venta` y `DetalleVenta`.

El diagrama entidad-relación, las cardinalidades y las decisiones de diseño están en [`docs/modelo-entidad-relacion.md`](docs/modelo-entidad-relacion.md). El detalle campo por campo, en [`docs/diccionario-de-datos.md`](docs/diccionario-de-datos.md).

**Reglas que la base de datos garantiza por sí sola:**

- El stock nunca queda negativo (`CHECK (Stock >= 0)`).
- Una venta se registra completa o no se registra: cabecera, detalle y descuento de inventario ocurren en una sola transacción.
- El precio de una venta ya emitida no cambia aunque cambie el precio del catálogo.
- No se elimina una categoría con bicicletas, ni un cliente con historial de compras.

---

## Códigos de error

Los procedimientos almacenados devuelven errores numerados que la API traduce a HTTP:

| Código | Situación | HTTP |
|---|---|---|
| `50001` | Cliente inexistente o inactivo | 400 |
| `50002` | Venta sin productos | 400 |
| `50003` | Cantidad menor o igual a cero | 400 |
| `50004` | Bicicleta inexistente o inactiva | 400 |
| `50005` | Stock insuficiente | 409 |
| `50006` | Venta inexistente o ya anulada | 404 |
| `50007` | Venta solicitada no existe | 404 |
