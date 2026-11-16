// Las abstracciones de las que dependen los servicios y los controladores (desafío 7).
// Dicen QUÉ se puede hacer con los datos; las clases que las implementan dicen CÓMO
// (con SQLite en Repositorios/, en memoria en Repositorios/Falsos/).

public interface IAeropuertoRepository
{
    List<Aeropuerto> Listar();
    Aeropuerto? Obtener(int id);
}

public interface IVueloRepository
{
    List<Vuelo> Listar();
    List<Vuelo> Buscar(int? destinoId, DateTime? desde);
    Vuelo? Obtener(int id);
    bool ExisteNumero(string numero);
    int Insertar(Vuelo vuelo);
    List<OcupacionVuelo> ListarOcupacion();
}

public interface IPasajeroRepository
{
    List<Pasajero> Listar();
    List<Pasajero> Buscar(string texto);
    Pasajero? Obtener(int id);
    bool ExisteDni(string dni, int? excepto = null);
    int Insertar(Pasajero pasajero);
    bool Actualizar(Pasajero pasajero);
    bool Eliminar(int id);
}

public interface IReservaRepository
{
    List<Reserva> Listar(EstadoReserva? estado);
    List<Reserva> ListarPorPasajero(int pasajeroId);
    Reserva? Obtener(int id);
    List<string> AsientosOcupados(int vueloId);
    bool PasajeroTieneReserva(int vueloId, int pasajeroId);
    int ContarConfirmadas(int vueloId);
    int Insertar(Reserva reserva);
    bool CambiarEstado(int id, EstadoReserva estado);
}

public interface IUsuarioRepository
{
    Usuario? Obtener(string nombreUsuario);
}

public interface IEquipajeRepository
{
    List<Equipaje> ListarPorReserva(int reservaId);
    int Insertar(Equipaje equipaje);
    decimal PesoEnBodega(int vueloId);
}
