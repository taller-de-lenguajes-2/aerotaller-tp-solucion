// Reglas para dar de alta un vuelo (lo usa el Admin).
public class VueloService(
    IVueloRepository vuelos,
    IAeropuertoRepository aeropuertos,
    ILogger<VueloService> logger) : IVueloService
{
    public int Crear(string numero, int origenId, int destinoId, DateTime salida, int duracionMinutos, int filas, decimal precio)
    {
        numero = numero.Trim().ToUpperInvariant();

        if (origenId == destinoId)
            throw new ReglaDeNegocioException("DestinoId", "El destino tiene que ser distinto del origen.");

        Aeropuerto origen = aeropuertos.Obtener(origenId)
            ?? throw new ReglaDeNegocioException("OrigenId", "Ese aeropuerto no existe.");
        Aeropuerto destino = aeropuertos.Obtener(destinoId)
            ?? throw new ReglaDeNegocioException("DestinoId", "Ese aeropuerto no existe.");

        if (vuelos.ExisteNumero(numero))
            throw new ReglaDeNegocioException("Numero", $"Ya existe un vuelo {numero}.");

        var vuelo = new Vuelo
        {
            Numero = numero,
            Origen = origen,
            Destino = destino,
            Salida = salida,
            DuracionMinutos = duracionMinutos,
            Filas = filas,
            Precio = precio
        };
        vuelo.Id = vuelos.Insertar(vuelo);

        logger.LogInformation("Vuelo {Numero} creado: {Origen} → {Destino}, sale {Salida}",
            vuelo.Numero, origen.Codigo, destino.Codigo, vuelo.Salida.ToString("dd/MM/yyyy HH:mm"));
        return vuelo.Id;
    }
}
