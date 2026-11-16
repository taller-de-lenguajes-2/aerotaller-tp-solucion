using Microsoft.AspNetCore.Mvc;

// Simulacro de parcial: agregar equipaje a una reserva.
[RequiereLogin]
public class EquipajeController(IReservaRepository reservas, IEquipajeService equipajeService) : Controller
{
    // GET /Equipaje/Crear?reservaId=7
    [HttpGet]
    public IActionResult Crear(int reservaId)
    {
        Reserva? reserva = reservas.Obtener(reservaId);
        if (reserva is null) return NotFound();

        return View(new EquipajeCrearViewModel { ReservaId = reservaId, Reserva = new ReservaListadoViewModel(reserva) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(EquipajeCrearViewModel modelo)
    {
        if (ModelState.IsValid)
        {
            try
            {
                equipajeService.Agregar(modelo.ReservaId, modelo.Tipo!.Value, modelo.PesoKg!.Value);
                TempData["Mensaje"] = "Equipaje agregado.";
                return RedirectToAction("Detalle", "Reservas", new { id = modelo.ReservaId });
            }
            catch (ReglaDeNegocioException ex)
            {
                ModelState.AddModelError(ex.Campo, ex.Message);
            }
        }

        Reserva? reserva = reservas.Obtener(modelo.ReservaId);
        if (reserva is null) return NotFound();
        modelo.Reserva = new ReservaListadoViewModel(reserva);
        return View(modelo);
    }
}
