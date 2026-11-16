using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

// El controlador queda "flaco" (desafío 6): recibe el formulario, llama al servicio y elige la vista.
// Las reglas (asiento libre, vuelo lleno…) NO están acá: están en ReservaService.
[RequiereLogin]
public class ReservasController(
    IReservaRepository reservas,
    IVueloRepository vuelos,
    IPasajeroRepository pasajeros,
    IEquipajeRepository equipajes,
    IReservaService reservaService) : Controller
{
    // GET /Reservas?estado=Cancelada
    public IActionResult Index(EstadoReserva? estado)
    {
        var modelo = new ReservaIndexViewModel
        {
            Estado = estado,
            Reservas = reservas.Listar(estado).Select(r => new ReservaListadoViewModel(r)).ToList()
        };
        return View(modelo);
    }

    public IActionResult Detalle(int id)
    {
        Reserva? reserva = reservas.Obtener(id);
        if (reserva is null) return NotFound();

        return View(new ReservaDetalleViewModel(reserva, equipajes.ListarPorReserva(id)));
    }

    // GET /Reservas/Crear?vueloId=2&asiento=5C  (desde el mapa de asientos)
    [HttpGet]
    public IActionResult Crear(int? vueloId, string? asiento)
    {
        var modelo = new ReservaCrearViewModel { VueloId = vueloId, Asiento = asiento ?? "" };
        CargarListas(modelo);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(ReservaCrearViewModel modelo)
    {
        // 1 · Los atributos del ViewModel ya corrieron: si algo falló, ModelState lo tiene
        if (!ModelState.IsValid)
        {
            CargarListas(modelo);
            return View(modelo);
        }

        // 2 · Las reglas que necesitan la base las decide el servicio
        try
        {
            Reserva reserva = reservaService.Reservar(modelo.VueloId!.Value, modelo.PasajeroId!.Value, modelo.Asiento);
            TempData["Mensaje"] = $"Reserva confirmada: {reserva.Vuelo.Numero}, asiento {reserva.Asiento}.";
            return RedirectToAction(nameof(Detalle), new { id = reserva.Id });
        }
        catch (ReglaDeNegocioException ex)
        {
            // El servicio dice en qué campo está el problema: el error aparece al lado de ese input
            ModelState.AddModelError(ex.Campo, ex.Message);
            CargarListas(modelo);
            return View(modelo);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancelar(int id)
    {
        try
        {
            reservaService.Cancelar(id);
            TempData["Mensaje"] = "Reserva cancelada.";
        }
        catch (ReglaDeNegocioException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Detalle), new { id });
    }

    private void CargarListas(ReservaCrearViewModel modelo)
    {
        modelo.Vuelos = vuelos.Listar()
            .Where(v => !v.YaSalio)
            .Select(v => new SelectListItem($"{v.Numero} · {v.Origen.Codigo} → {v.Destino.Codigo} · {v.Salida:dd/MM HH:mm}", v.Id.ToString()))
            .ToList();
        modelo.Pasajeros = pasajeros.Listar()
            .Select(p => new SelectListItem($"{p.Nombre} ({p.Dni})", p.Id.ToString()))
            .ToList();
    }
}
