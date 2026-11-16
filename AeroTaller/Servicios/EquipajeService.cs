// Simulacro de parcial: reglas del equipaje.
//  · Solo se agrega equipaje a una reserva confirmada de un vuelo que no salió.
//  · Bodega: hasta 23 kg por pieza y hasta 2 piezas por reserva.
//  · Mano: hasta 10 kg y una sola pieza.
public class EquipajeService(
    IEquipajeRepository equipajes,
    IReservaRepository reservas,
    ILogger<EquipajeService> logger) : IEquipajeService
{
    public int Agregar(int reservaId, TipoEquipaje tipo, decimal pesoKg)
    {
        Reserva reserva = reservas.Obtener(reservaId)
            ?? throw new ReglaDeNegocioException("", "Esa reserva no existe.");

        if (reserva.Estado != EstadoReserva.Confirmada)
            throw new ReglaDeNegocioException("", "La reserva está cancelada.");
        if (reserva.Vuelo.YaSalio)
            throw new ReglaDeNegocioException("", "El vuelo ya salió.");

        List<Equipaje> piezas = equipajes.ListarPorReserva(reservaId);

        if (tipo == TipoEquipaje.Bodega)
        {
            if (pesoKg > Equipaje.PesoMaximoBodega)
                throw new ReglaDeNegocioException("PesoKg", $"En bodega cada pieza puede pesar hasta {Equipaje.PesoMaximoBodega} kg.");
            if (piezas.Count(p => p.Tipo == TipoEquipaje.Bodega) >= Equipaje.PiezasMaximasBodega)
                throw new ReglaDeNegocioException("Tipo", $"La reserva ya tiene {Equipaje.PiezasMaximasBodega} piezas en bodega.");
        }
        else
        {
            if (pesoKg > Equipaje.PesoMaximoMano)
                throw new ReglaDeNegocioException("PesoKg", $"El equipaje de mano puede pesar hasta {Equipaje.PesoMaximoMano} kg.");
            if (piezas.Any(p => p.Tipo == TipoEquipaje.Mano))
                throw new ReglaDeNegocioException("Tipo", "La reserva ya tiene su equipaje de mano.");
        }

        int id = equipajes.Insertar(new Equipaje { ReservaId = reservaId, Tipo = tipo, PesoKg = pesoKg });
        logger.LogInformation("Equipaje {EquipajeId} ({Tipo}, {PesoKg} kg) agregado a la reserva {ReservaId}",
            id, tipo, pesoKg, reservaId);
        return id;
    }
}
