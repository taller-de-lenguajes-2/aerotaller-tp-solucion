// Una "base de datos" en memoria para los repositorios falsos (desafío 7).
// Se registra como Singleton: las listas se conservan mientras la aplicación está en ejecución (al reiniciarla, vuelven a estos datos).
// Todos los repositorios falsos reciben ESTA misma instancia por el constructor,
// de modo que una reserva creada con ReservaRepositoryFalso resulta visible desde VueloRepositoryFalso.
public class BaseFalsa
{
    public List<Aeropuerto> Aeropuertos { get; }
    public List<Vuelo> Vuelos { get; }
    public List<Pasajero> Pasajeros { get; }
    public List<Reserva> Reservas { get; }
    public List<Usuario> Usuarios { get; }
    public List<Equipaje> Equipajes { get; } = [];

    // Las listas no son seguras para varios requests a la vez: todos los repositorios falsos usan este candado
    public object Candado { get; } = new();

    public BaseFalsa()
    {
        var tuc = new Aeropuerto { Id = 1, Codigo = "TUC", Ciudad = "San Miguel de Tucumán", Nombre = "Teniente Benjamín Matienzo" };
        var aep = new Aeropuerto { Id = 2, Codigo = "AEP", Ciudad = "Buenos Aires", Nombre = "Aeroparque Jorge Newbery" };
        var brc = new Aeropuerto { Id = 3, Codigo = "BRC", Ciudad = "San Carlos de Bariloche", Nombre = "Teniente Luis Candelaria" };
        Aeropuertos = [tuc, aep, brc];

        DateTime hoy = DateTime.Today;
        Vuelos =
        [
            new Vuelo { Id = 1, Numero = "FK0001", Origen = tuc, Destino = aep, Salida = hoy.AddDays(1).AddHours(8), DuracionMinutos = 125, Filas = 4, Precio = 1 },
            new Vuelo { Id = 2, Numero = "FK0002", Origen = aep, Destino = brc, Salida = hoy.AddDays(2).AddHours(10), DuracionMinutos = 140, Filas = 4, Precio = 1 }
        ];

        Pasajeros =
        [
            new Pasajero { Id = 1, Nombre = "Pasajera de Prueba", Dni = "11111111", Email = "prueba@falso.com" },
            new Pasajero { Id = 2, Nombre = "Pasajero de Prueba", Dni = "22222222" }
        ];

        Reservas =
        [
            new Reserva { Id = 1, Vuelo = Vuelos[0], Pasajero = Pasajeros[0], Asiento = "1A", Fecha = hoy }
        ];

        Usuarios =
        [
            new Usuario { Id = 1, NombreUsuario = "admin", Nombre = "Admin (memoria)", Clave = "aero2026", Rol = Roles.Admin },
            new Usuario { Id = 2, NombreUsuario = "mostrador", Nombre = "Mostrador (memoria)", Clave = "vuelos2026", Rol = Roles.Mostrador }
        ];
    }

    // El "AUTOINCREMENT" de la memoria
    public static int SiguienteId<T>(List<T> lista, Func<T, int> id) => lista.Count == 0 ? 1 : lista.Max(id) + 1;
}
