using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

// Vuelos: cualquiera puede verlos; solo el Admin los crea.
// El controlador recibe lo que necesita por el constructor (desafío 7): nunca hace "new" de un repositorio.
public class VuelosController(
    IVueloRepository vuelos,
    IAeropuertoRepository aeropuertos,
    IReservaRepository reservas,
    IEquipajeRepository equipajes,
    IVueloService vueloService) : Controller
{
    // GET /Vuelos?DestinoId=2&Desde=2026-12-01
    // Los parámetros del query string llegan solos al ViewModel (model binding)
    public IActionResult Index(VueloIndexViewModel filtro)
    {
        List<Vuelo> encontrados = vuelos.Buscar(filtro.DestinoId, filtro.Desde);

        // Una sola consulta con GROUP BY para la ocupación de todos (no una consulta por vuelo)
        Dictionary<int, int> ocupados = vuelos.ListarOcupacion().ToDictionary(o => o.VueloId, o => o.Ocupados);

        filtro.Resultados = encontrados
            .Select(v => new VueloListadoViewModel(v, ocupados.GetValueOrDefault(v.Id)))
            .ToList();
        filtro.Destinos = ListaDeAeropuertos();
        return View(filtro);
    }

    // GET /Vuelos/Detalle/3 → id = 3 (ruteo convencional)
    public IActionResult Detalle(int id)
    {
        Vuelo? vuelo = vuelos.Obtener(id);
        if (vuelo is null) return NotFound();

        var modelo = new VueloDetalleViewModel(vuelo, reservas.AsientosOcupados(id), equipajes.PesoEnBodega(id));
        return View(modelo);
    }

    [HttpGet]
    [RequiereRol(Roles.Admin)]
    public IActionResult Crear()
    {
        var modelo = new VueloCrearViewModel { Aeropuertos = ListaDeAeropuertos() };
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequiereRol(Roles.Admin)]
    public IActionResult Crear(VueloCrearViewModel modelo)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Pasó [Required]: en este punto los int? ya tienen valor
                int id = vueloService.Crear(modelo.Numero, modelo.OrigenId!.Value, modelo.DestinoId!.Value,
                    modelo.Salida!.Value, modelo.DuracionMinutos!.Value, modelo.Filas!.Value, modelo.Precio!.Value);

                TempData["Mensaje"] = $"Vuelo {modelo.Numero.ToUpperInvariant()} creado.";
                return RedirectToAction(nameof(Detalle), new { id });       // POST → Redirect → GET
            }
            catch (ReglaDeNegocioException ex)
            {
                ModelState.AddModelError(ex.Campo, ex.Message);
            }
        }

        // Algo falló: las listas no vuelven en el POST, hay que cargarlas otra vez
        modelo.Aeropuertos = ListaDeAeropuertos();
        return View(modelo);
    }

    private List<SelectListItem> ListaDeAeropuertos() =>
        aeropuertos.Listar()
            .Select(a => new SelectListItem($"{a.Ciudad} ({a.Codigo})", a.Id.ToString()))
            .ToList();
}
