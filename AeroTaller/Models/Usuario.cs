// Quien usa el sistema (desafío 9). No confundir con Pasajero: los pasajeros no inician sesión.
public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Clave { get; set; } = "";     // atención: en texto plano solo para el TP (igual que en el tema 15)
    public string Rol { get; set; } = "";       // Roles.Admin o Roles.Mostrador
}

// Los nombres de los roles en un solo lugar: evita inconsistencias como "admin" en un lugar y "Admin" en otro
public static class Roles
{
    public const string Admin = "Admin";
    public const string Mostrador = "Mostrador";
}
