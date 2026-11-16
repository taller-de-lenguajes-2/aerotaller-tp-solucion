// Iniciar y cerrar sesión, y saber quién está conectado (desafío 9, igual que en el tema 15).
// Guarda los datos del usuario en la SESIÓN (en el servidor); el navegador solo tiene la cookie con el Id de sesión.
public class AutenticacionService(
    IUsuarioRepository usuarios,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AutenticacionService> logger) : IAutenticacionService
{
    // Las claves de la sesión, en un solo lugar
    public const string ClaveUsuario = "Usuario";
    public const string ClaveNombre = "Nombre";
    public const string ClaveRol = "Rol";

    private ISession Sesion => httpContextAccessor.HttpContext!.Session;

    // Autenticación: ¿es quien dice ser?
    public bool Login(string nombreUsuario, string clave)
    {
        Usuario? usuario = usuarios.Obtener(nombreUsuario);
        if (usuario is null || usuario.Clave != clave)
        {
            // Se registra el intento, NUNCA la clave que escribió
            logger.LogWarning("Login fallido para el usuario {NombreUsuario}", nombreUsuario);
            return false;
        }

        Sesion.SetString(ClaveUsuario, usuario.NombreUsuario);
        Sesion.SetString(ClaveNombre, usuario.Nombre);
        Sesion.SetString(ClaveRol, usuario.Rol);

        logger.LogInformation("{NombreUsuario} inició sesión ({Rol})", usuario.NombreUsuario, usuario.Rol);
        return true;
    }

    public void Logout() => Sesion.Clear();

    public bool EstaAutenticado() => Sesion.GetString(ClaveUsuario) is not null;

    // Autorización: ¿qué puede hacer?
    public bool TieneRol(string rol) => Sesion.GetString(ClaveRol) == rol;
}
