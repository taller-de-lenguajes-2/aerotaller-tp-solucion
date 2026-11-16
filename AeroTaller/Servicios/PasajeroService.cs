// Reglas de los pasajeros: DNI único y no borrar a quien tiene reservas.
public class PasajeroService(
    IPasajeroRepository pasajeros,
    IReservaRepository reservas,
    ILogger<PasajeroService> logger) : IPasajeroService
{
    public int Registrar(Pasajero pasajero)
    {
        if (pasajeros.ExisteDni(pasajero.Dni))
            throw new ReglaDeNegocioException("Dni", $"Ya hay un pasajero con el DNI {pasajero.Dni}.");

        pasajero.Id = pasajeros.Insertar(pasajero);
        logger.LogInformation("Pasajero {PasajeroId} registrado", pasajero.Id);   // sin DNI ni email en el log: son datos personales
        return pasajero.Id;
    }

    public void Actualizar(Pasajero pasajero)
    {
        if (pasajeros.ExisteDni(pasajero.Dni, excepto: pasajero.Id))
            throw new ReglaDeNegocioException("Dni", $"Ya hay otro pasajero con el DNI {pasajero.Dni}.");

        if (!pasajeros.Actualizar(pasajero))
            throw new ReglaDeNegocioException("", "Ese pasajero no existe.");

        logger.LogInformation("Pasajero {PasajeroId} actualizado", pasajero.Id);
    }

    public void Eliminar(int id)
    {
        // La regla se controla ANTES de ir a la base. Si igual se escapara, la clave foránea
        // de SQLite haría de red de seguridad (con un SqliteException).
        int cantidad = reservas.ListarPorPasajero(id).Count;
        if (cantidad > 0)
            throw new ReglaDeNegocioException("", $"No se puede eliminar: el pasajero tiene {cantidad} reserva(s).");

        if (!pasajeros.Eliminar(id))
            throw new ReglaDeNegocioException("", "Ese pasajero no existe.");

        logger.LogInformation("Pasajero {PasajeroId} eliminado", id);
    }
}
