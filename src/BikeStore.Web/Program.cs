using BikeStore.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

/* ----------------------------------------------------------------
   Cliente HTTP tipado hacia la API REST.
   Esta capa NO accede a la base de datos: toda la información se
   obtiene consumiendo BikeStore.API, que es lo que convierte a la
   solución en una verdadera arquitectura cliente-servidor.
   ---------------------------------------------------------------- */
var urlApi = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("Falta la configuración 'ApiSettings:BaseUrl'.");

builder.Services.AddHttpClient<ServicioApi>(cliente =>
{
    cliente.BaseAddress = new Uri(urlApi);
    cliente.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
