using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

// Un filtro: se ejecuta ANTES de la acción (desafío 9, igual que en el tema 15).
// Si no hay sesión iniciada, interrumpe el request y redirige al login, conservando la dirección solicitada.
// Se aplica sobre un controlador (todas sus acciones) o sobre una acción.
public class RequiereLoginAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var sesion = context.HttpContext.Session;
        if (sesion.GetString(AutenticacionService.ClaveUsuario) is null)
        {
            var pedido = context.HttpContext.Request;
            context.Result = new RedirectToActionResult("Index", "Login",
                new { returnUrl = pedido.Path + pedido.QueryString });
        }
    }
}

// Autorización: además de haber iniciado sesión, se requiere uno de estos roles.
// Uso: [RequiereRol(Roles.Admin)]  o  [RequiereRol(Roles.Admin, Roles.Mostrador)]
public class RequiereRolAttribute(params string[] roles) : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var sesion = context.HttpContext.Session;
        if (sesion.GetString(AutenticacionService.ClaveUsuario) is null)
        {
            var pedido = context.HttpContext.Request;
            context.Result = new RedirectToActionResult("Index", "Login",
                new { returnUrl = pedido.Path + pedido.QueryString });
        }
        else if (!roles.Contains(sesion.GetString(AutenticacionService.ClaveRol)))
        {
            context.Result = new ViewResult { ViewName = "SinPermiso" };
        }
    }
}
