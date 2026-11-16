using Microsoft.AspNetCore.Mvc;

// Igual que en el tema 15, pero el servicio se recibe por su interfaz (desafío 9)
public class LoginController(IAutenticacionService autenticacion) : Controller
{
    // GET /Login?returnUrl=/Reservas
    [HttpGet]
    public IActionResult Index(string? returnUrl)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(LoginViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        if (!autenticacion.Login(modelo.NombreUsuario, modelo.Clave))
        {
            // No indicar cuál de los dos es incorrecto: no se dan pistas a quien prueba usuarios
            ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            return View(modelo);
        }

        // Solo se vuelve a una dirección del PROPIO sitio (evita redirecciones a otro sitio)
        if (Url.IsLocalUrl(modelo.ReturnUrl)) return Redirect(modelo.ReturnUrl);
        return RedirectToAction("Index", "Home");
    }

    // Cerrar sesión cambia el estado: es POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Salir()
    {
        autenticacion.Logout();
        return RedirectToAction("Index", "Home");
    }
}
