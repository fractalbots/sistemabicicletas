using BikeStore.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

/* ----------------------------------------------------------------
   Cliente HTTP hacia la API REST.
   La aplicacion web NO conoce la cadena de conexion ni toca SQL
   Server: toda la informacion llega a traves de estos servicios.
   La direccion de la API se configura en appsettings.json.
   ---------------------------------------------------------------- */
var urlApi = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException(
        "Falta la clave 'ApiSettings:BaseUrl' en appsettings.json.");

builder.Services.AddHttpClient<BikeStoreApiClient>(cliente =>
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

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
