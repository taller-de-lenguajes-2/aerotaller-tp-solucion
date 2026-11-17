using Microsoft.Data.Sqlite;

// Las reglas para reservar y cancelar (desafíos 6 y 8).
// Recibe tres repositorios y un logger: el contenedor los crea y los inyecta por el constructor (desafío 7).
public class ReservaService(
    IReservaRepository reservas,
    IVueloRepository vuelos,
    IPasajeroRepository pasajeros,
    ILogger<ReservaService> logger) : IReservaService
{
    // SQLITE_CONSTRAINT: se violó una restricción de la base (UNIQUE, FOREIGN KEY, CHECK…)
    private const int ErrorDeRestriccion = 19;

    public Reserva Reservar(int vueloId, int pasajeroId, string asiento)
    {
        asiento = asiento.Trim().ToUpperInvariant();          // "12a " → "12A"

        Vuelo vuelo = vuelos.Obtener(vueloId)
            ?? throw new ReglaDeNegocioException("VueloId", "Ese vuelo no existe.");
        Pasajero pasajero = pasajeros.Obtener(pasajeroId)
            ?? throw new ReglaDeNegocioException("PasajeroId", "Ese pasajero no existe.");

        // Regla 1: no se reserva un vuelo que ya salió
        if (vuelo.YaSalio)
            throw new ReglaDeNegocioException("VueloId", $"El vuelo {vuelo.Numero} ya salió.");

        // Regla 2: el asiento tiene que existir en ESTE avión
        if (!vuelo.TieneAsiento(asiento))
            throw new ReglaDeNegocioException("Asiento",
                $"El asiento {asiento} no existe en este avión (filas 1 a {vuelo.Filas}, letras A a F).");

        // Regla 3: sin overbooking
        if (reservas.ContarConfirmadas(vueloId) >= vuelo.Capacidad)
            throw new ReglaDeNegocioException("VueloId", $"El vuelo {vuelo.Numero} está completo.");

        // Regla 4: el asiento tiene que estar libre
        if (reservas.AsientosOcupados(vueloId).Contains(asiento))
            throw new ReglaDeNegocioException("Asiento", $"El asiento {asiento} ya está ocupado.");

        // Regla 5: un pasajero no reserva dos veces el mismo vuelo
        if (reservas.PasajeroTieneReserva(vueloId, pasajeroId))
            throw new ReglaDeNegocioException("PasajeroId", $"{pasajero.Nombre} ya tiene una reserva en este vuelo.");

        var reserva = new Reserva
        {
            Vuelo = vuelo,
            Pasajero = pasajero,
            Asiento = asiento,
            Fecha = DateTime.Now,
            Estado = EstadoReserva.Confirmada
        };

        try
        {
            reserva.Id = reservas.Insertar(reserva);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == ErrorDeRestriccion)
        {
            // Dos personas eligieron el mismo asiento al mismo tiempo: ambas operaciones superaron la regla 4,
            // pero el índice único de la base solo admite la primera.
            logger.LogWarning("Choque de reservas: el asiento {Asiento} del vuelo {Numero} se ocupó recién", asiento, vuelo.Numero);
            throw new ReglaDeNegocioException("Asiento", $"El asiento {asiento} acaba de reservarse en otra operación. Debe elegirse otro asiento.", ex);
        }

        // Log estructurado: {ReservaId} es un dato con nombre, no texto concatenado con $"..."
        logger.LogInformation("Reserva {ReservaId} confirmada: vuelo {Numero}, asiento {Asiento}, pasajero {PasajeroId}",
            reserva.Id, vuelo.Numero, asiento, pasajero.Id);
        return reserva;
    }

    public void Cancelar(int reservaId)
    {
        Reserva reserva = reservas.Obtener(reservaId)
            ?? throw new ReglaDeNegocioException("", "Esa reserva no existe.");

        if (reserva.Estado == EstadoReserva.Cancelada)
            throw new ReglaDeNegocioException("", "La reserva ya estaba cancelada.");
        if (reserva.Vuelo.YaSalio)
            throw new ReglaDeNegocioException("", $"El vuelo {reserva.Vuelo.Numero} ya salió: no se puede cancelar.");

        reservas.CambiarEstado(reservaId, EstadoReserva.Cancelada);
        logger.LogInformation("Reserva {ReservaId} cancelada (vuelo {Numero}, asiento {Asiento})",
            reservaId, reserva.Vuelo.Numero, reserva.Asiento);
    }
}
