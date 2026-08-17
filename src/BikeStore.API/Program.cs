using BikeStore.Data;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

/* ----------------------------------------------------------------
   Capa de datos: EF Core + repositorios (ver DependencyInjection.cs)
   ---------------------------------------------------------------- */
var cadenaConexion = builder.Configuration.GetConnectionString("BikeStoreDB")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'BikeStoreDB'.");

builder.Services.AddCapaDatos(cadenaConexion);

/* ----------------------------------------------------------------
   Controladores y documentación Swagger / OpenAPI
   ---------------------------------------------------------------- */
builder.Services.AddControllers()
    .AddJsonOptions(opciones =>
    {
        // Evita bucles infinitos al serializar navegaciones bidireccionales
        // (Bicicleta -> Categoria -> Bicicletas -> ...).
        opciones.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opciones.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "BikeStore API",
        Description = "API REST para la gestión de la tienda de bicicletas. " +
                      "Proyecto final de Sistemas Cliente Servidor.",
        Contact = new OpenApiContact { Name = "Equipo BikeStore" }
    });

    // Incluye los comentarios XML de los controladores en Swagger.
    var archivoXml = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var rutaXml = Path.Combine(AppContext.BaseDirectory, archivoXml);
    if (File.Exists(rutaXml))
        opciones.IncludeXmlComments(rutaXml);
});

/* ----------------------------------------------------------------
   CORS: permite que BikeStore.Web (cliente MVC) consuma esta API
   ---------------------------------------------------------------- */
builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy("PermitirWeb", politica =>
        politica.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opciones =>
    {
        opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "BikeStore API v1");
        opciones.RoutePrefix = string.Empty;   // Swagger queda en la raíz del sitio
    });
}

app.UseCors("PermitirWeb");
app.UseAuthorization();
app.MapControllers();

app.Run();
