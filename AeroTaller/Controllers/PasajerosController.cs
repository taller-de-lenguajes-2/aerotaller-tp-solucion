using Microsoft.AspNetCore.Mvc;

[RequiereLogin]   // todas las acciones requieren sesión iniciada
public class PasajerosController(
    IPasajeroRepository pasajeros,
    IReservaRepository reservas,
    IPasajeroService pasajeroService) : Controller
{
    // GET /Pasajeros?buscar=gom
    public IActionResult Index(string? buscar)
    {
        List<Pasajero> lista = string.IsNullOrWhiteSpace(buscar) ? pasajeros.Listar() : pasajeros.Buscar(buscar);
        ViewData["Buscar"] = buscar;
        return View(lista);
    }

    public IActionResult Detalle(int id)
    {
        Pasajero? pasajero = pasajeros.Obtener(id);
        if (pasajero is null) return NotFound();

        return View(new PasajeroDetalleViewModel(pasajero, reservas.ListarPorPasajero(id)));
    }

    [HttpGet]
    public IActionResult Crear() => View("Formulario", new PasajeroFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(PasajeroFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View("Formulario", modelo);

        try
        {
            int id = pasajeroService.Registrar(modelo.APasajero());
            TempData["Mensaje"] = "Pasajero registrado.";
            return RedirectToAction(nameof(Detalle), new { id });
        }
        catch (ReglaDeNegocioException ex)
        {
            ModelState.AddModelError(ex.Campo, ex.Message);
            return View("Formulario", modelo);
        }
    }

    [HttpGet]
    public IActionResult Editar(int id)
    {
        Pasajero? pasajero = pasajeros.Obtener(id);
        if (pasajero is null) return NotFound();

        return View("Formulario", new PasajeroFormViewModel(pasajero));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(int id, PasajeroFormViewModel modelo)
    {
        modelo.Id = id;     // el Id lo aporta la ruta, no un input que pueda modificarse
        if (!ModelState.IsValid) return View("Formulario", modelo);

        try
        {
            pasajeroService.Actualizar(modelo.APasajero());
            TempData["Mensaje"] = "Cambios guardados.";
            return RedirectToAction(nameof(Detalle), new { id });
        }
        catch (ReglaDeNegocioException ex)
        {
            ModelState.AddModelError(ex.Campo, ex.Message);
            return View("Formulario", modelo);
        }
    }

    // Borrar modifica datos: es POST (nunca un enlace GET) y solo puede hacerlo el Admin
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequiereRol(Roles.Admin)]
    public IActionResult Eliminar(int id)
    {
        try
        {
            pasajeroService.Eliminar(id);
            TempData["Mensaje"] = "Pasajero eliminado.";
            return RedirectToAction(nameof(Index));
        }
        catch (ReglaDeNegocioException ex)
        {
            // No hay formulario donde mostrarlo: el mensaje viaja en TempData hasta el próximo request
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Detalle), new { id });
        }
    }
}
