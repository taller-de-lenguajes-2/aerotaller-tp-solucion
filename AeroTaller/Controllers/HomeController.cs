using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class HomeController(IVueloRepository vuelos) : Controller
{
    // El tablero: ocupación de los próximos vuelos (la consulta con GROUP BY)
    public IActionResult Index()
    {
        List<OcupacionVuelo> ocupacion = vuelos.ListarOcupacion();
        return View(ocupacion);
    }

    // A esta acción llega UseExceptionHandler("/Home/Error") cuando algo explota (desafío 8).
    // El error ya quedó en el log; acá solo se muestra un mensaje digno y el Id para buscarlo.
    public IActionResult Error()
    {
        return View(new ErrorViewModel(Activity.Current?.Id ?? HttpContext.TraceIdentifier));
    }

    // UseStatusCodePagesWithReExecute manda acá los 404 (por ejemplo /Vuelos/Detalle/999)
    public IActionResult NoEncontrado(int codigo)
    {
        Response.StatusCode = codigo;
        return View(codigo);
    }
}
