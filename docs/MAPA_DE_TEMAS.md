# Mapa de temas — Taller de Lenguajes 2 (2026) → TP AeroTaller

Fuente: presentaciones `tema-XX-*/index.html` y proyectos de ejemplo de este repo.

**Stack de la cátedra:** .NET 10 (C# 12, primary constructors) · ASP.NET Core MVC (`dotnet new mvc`) · ADO.NET con
`Microsoft.Data.Sqlite` (sin Entity Framework) · SQLite + DB Browser for SQLite · Serilog (`Serilog.AspNetCore`) ·
VS Code como IDE principal · Git + GitHub (Classroom).

**Orden de dictado** (difiere de la numeración): 01·02 → 03·04 → 05 → 06 → 16 → 07 → 08·09 → 10·11 → 12·13·15 → 14 → 17.

| Tema | Conceptos clave | Nombres/convenciones de la cátedra | Ejemplo de la cátedra | Desafío del TP |
|---|---|---|---|---|
| 01–02 POO | abstracción, encapsulamiento, composición/agregación | — | el bar | 2 (`Reserva` compone `Vuelo` y `Pasajero`) |
| 03–04 Herencia, interfaces | polimorfismo, interfaz = contrato | `I…` | — | 6–7 (interfaces de repositorios y servicios) |
| 05 API REST | HTTP, verbos, códigos, URL | — | mozo del bar | opcional (`/api/vuelos`) |
| 06 Web API | controlador, acciones, ruteo, `From…` | `[ApiController]`, `ControllerBase`, `CreatedAtAction` | `PacientesController` con lista estática | opcional |
| 07 Bases de datos | PK, FK, integridad, SQL (SELECT, LIKE, JOIN, GROUP BY, INSERT/UPDATE/DELETE) | tablas en singular PascalCase, fechas TEXT `'AAAA-MM-DD HH:MM'`, `PRAGMA foreign_keys = ON` | `clinica.sql` | 1 |
| 08 ADO.NET | Connection/Command/DataReader, parámetros, inyección SQL | `using var conexion…; conexion.Open()`, `AddWithValue("@id", …)`, `INSERT … RETURNING Id`, `IsDBNull`, `ConnectionStrings` en appsettings | consola + API | 2 |
| 09 Repositorios | SRP, un repo por entidad, capas | `Repositorios/XRepository.cs`, `Listar/Obtener/Buscar/Insertar/Actualizar/Eliminar`, `LeerX(lector)` | `PacienteRepository`, `PacienteRepositoryJson` | 2 · 6 |
| 10 MVC | patrón, viaje del request, ruteo convencional | `AddControllersWithViews`, `MapControllerRoute("default", …)`, `@model List<Paciente>` | `ClinicaMvc` | 0 · 3 |
| 11 Layout y vistas | Razor, tag helpers, formularios, PRG | `asp-for`, `asp-action`, `asp-route-id`, `[ValidateAntiForgeryToken]`, `RedirectToAction(nameof(Detalle), new { id })` | `Pacientes/Crear` | 3 |
| 12 ViewModels | Model vs VM, overposting, anticorrupción | `ViewModels/`, `XCrearViewModel`, `XListadoViewModel`, `SelectListItem`, `CargarListas(modelo)` | `TurnoCrearViewModel` | 4 |
| 13 Validación | atributos, `ModelState`, reglas con la base, cliente vs servidor | `[Required(ErrorMessage=…)]`, `int?` en combos, `FechaFuturaAttribute`, `AddModelError(nameof(…))`, `_ValidationScriptsPartial` | turnos | 5 |
| 14 Inyección de dependencias | DIP, contenedor, tiempos de vida, probar sin base | `record ConexionDb(string Cadena)` Singleton, `AddScoped<IX, X>()`, `TurnoRepositoryFalso` | clínica con interfaces, `/TiemposDeVida` | 7 |
| 15 Cookies y sesión | autenticación vs autorización, cookie, sesión, filtros | `AutenticacionService`, `LoginViewModel`, `[RequiereLogin]`, `[RequiereRol("Admin")]`, vista `SinPermiso`, `Url.IsLocalUrl` | login de la clínica (usuarios en memoria) | 9 |
| 16 Excepciones | try/catch/finally, `when`, `throw;`, guardas, excepción propia, manejador global | `ThrowIfNull`, `SaldoInsuficienteException`, `ManejadorGlobal : IExceptionHandler` | consola + Web API | 8 |
| 17 Logs | `ILogger<T>`, niveles, log estructurado, Serilog a archivo | plantillas `{ProductoId}` (sin `$`), `LogError(ex, …)`, registrar en el log una sola vez, nunca claves/cadenas | `EjemploLogs` (Web API) | 8 |

## Lo que el TP agrega y la cátedra no dictó

- **Depuración con breakpoints**: ninguna diapositiva la cubre. El desafío 0 la presenta en VS Code (C# Dev Kit, F5, F10/F11, Variables, Watch, Call Stack).
- **Capa de servicios con reglas de negocio**: la diapositiva de capas del tema 09 la nombra (Controladores → Servicios → Repositorios), pero los ejemplos solo tienen `AutenticacionService`. El TP la hace explícita.
- **Excepciones y logs en MVC**: los temas 16–17 lo muestran sobre Web API (`ProblemDetails`). En MVC se adapta a `UseExceptionHandler("/Home/Error")` + vista de error; se marca como "adaptación".
- **Usuarios en SQLite**: en el tema 15 están en memoria con clave en texto plano; el TP los traslada a una tabla y deja `PasswordHasher<T>` como "para investigar".

## Sin cubrir en el TP

- Testing con xUnit: la cátedra no lo dicta. Solo aparece la prueba manual con un repositorio falso (tema 14), que el TP incluye como desafío opcional.
