using System.Globalization;
using Microsoft.Data.Sqlite;
using Serilog;

// Fechas y moneda en castellano, en cualquier máquina.
// Ojo con los decimales: un <input type="number"> SIEMPRE manda "18.5" (con punto), y con es-AR puro
// el model binding lo leería como 185. Por eso los números (no la moneda) usan punto decimal.
var cultura = (CultureInfo)CultureInfo.GetCultureInfo("es-AR").Clone();
cultura.NumberFormat.NumberDecimalSeparator = ".";
cultura.NumberFormat.NumberGroupSeparator = "";
CultureInfo.DefaultThreadCurrentCulture = cultura;

var builder = WebApplication.CreateBuilder(args);

// ── Logs (desafío 8): Serilog reemplaza al logger por defecto y lee su configuración de appsettings.json ──
builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration));

// ── Datos (desafío 7): qué implementación se usa para cada interfaz ──
// "OrigenDeDatos" sale de appsettings.json, o de la línea de comandos: dotnet run --OrigenDeDatos=Falso
// Fijate que SOLO cambia este bloque: controladores, servicios y vistas no se enteran.
string origenDeDatos = builder.Configuration["OrigenDeDatos"] ?? "Sqlite";

if (origenDeDatos == "Falso")
{
    // Todo en memoria: una sola BaseFalsa para toda la app (si fuera Scoped, se perderían los datos en cada request)
    builder.Services.AddSingleton<BaseFalsa>();
    builder.Services.AddScoped<IAeropuertoRepository, AeropuertoRepositoryFalso>();
    builder.Services.AddScoped<IVueloRepository, VueloRepositoryFalso>();
    builder.Services.AddScoped<IPasajeroRepository, PasajeroRepositoryFalso>();
    builder.Services.AddScoped<IReservaRepository, ReservaRepositoryFalso>();
    builder.Services.AddScoped<IUsuarioRepository, UsuarioRepositoryFalso>();
    builder.Services.AddScoped<IEquipajeRepository, EquipajeRepositoryFalso>();
}
else
{
    // La cadena se lee UNA vez, al arrancar. Si falta, la app no levanta (mejor que un error raro después)
    string cadenaConexion = builder.Configuration.GetConnectionString("AeroTaller")
        ?? throw new InvalidOperationException("Falta la cadena de conexión 'AeroTaller' en appsettings.json");
    CrearBaseSiNoExiste(cadenaConexion);

    // La cadena, envuelta en un tipo propio: un solo objeto para toda la aplicación
    builder.Services.AddSingleton(new ConexionDb(cadenaConexion));

    // Repositorios: uno nuevo por request. Reciben ConexionDb, que también está en el contenedor
    builder.Services.AddScoped<IAeropuertoRepository, AeropuertoRepository>();
    builder.Services.AddScoped<IVueloRepository, VueloRepository>();
    builder.Services.AddScoped<IPasajeroRepository, PasajeroRepository>();
    builder.Services.AddScoped<IReservaRepository, ReservaRepository>();
    builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    builder.Services.AddScoped<IEquipajeRepository, EquipajeRepository>();
}

// ── Servicios (desafío 6): las reglas del negocio. Dependen de las INTERFACES de los repositorios ──
builder.Services.AddScoped<IReservaService, ReservaService>();
builder.Services.AddScoped<IPasajeroService, PasajeroService>();
builder.Services.AddScoped<IVueloService, VueloService>();
builder.Services.AddScoped<IEquipajeService, EquipajeService>();

// ── Sesión y login (desafío 9) ──
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);   // sin actividad por 20 minutos, la sesión se pierde
    options.Cookie.HttpOnly = true;                   // JavaScript no puede leer la cookie de sesión
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();            // para que AutenticacionService llegue a la sesión
builder.Services.AddScoped<IAutenticacionService, AutenticacionService>();

// MVC: controladores que devuelven vistas
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── El pipeline: el orden importa ──
app.UseSerilogRequestLogging();           // una línea de log por request (antes del manejador de errores)
app.UseExceptionHandler("/Home/Error");   // si algo explota: se loguea y se muestra Views/Home/Error.cshtml
app.UseStatusCodePagesWithReExecute("/Home/NoEncontrado", "?codigo={0}");   // 404 con la misma estética
app.UseStaticFiles();                     // sirve lo que está en wwwroot (el CSS)
app.UseSession();                         // antes de los controladores: así las acciones pueden usar la sesión

// Ruteo convencional: /Vuelos/Detalle/3 → VuelosController.Detalle(3)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Logger.LogInformation("AeroTaller arrancó con datos de origen {OrigenDeDatos}", origenDeDatos);
app.Run();

// Si aerotaller.db no existe la arma con aerotaller.sql (desafío 1).
static void CrearBaseSiNoExiste(string cadenaConexion)
{
    var archivo = new SqliteConnectionStringBuilder(cadenaConexion).DataSource;
    if (File.Exists(archivo)) return;

    using var conexion = new SqliteConnection(cadenaConexion);
    conexion.Open();
    using var comando = conexion.CreateCommand();
    comando.CommandText = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "aerotaller.sql"));
    comando.ExecuteNonQuery();
}
