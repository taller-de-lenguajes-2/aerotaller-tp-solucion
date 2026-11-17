// La capa de servicios (desafío 6): las REGLAS DEL NEGOCIO se concentran aquí.
// El controlador recibe el formulario y selecciona la vista; el repositorio ejecuta el SQL;
// el servicio evalúa si una operación está permitida ("¿ese asiento está libre?").
// Si una regla no se cumple, el servicio lanza ReglaDeNegocioException.

public interface IReservaService
{
    Reserva Reservar(int vueloId, int pasajeroId, string asiento);
    void Cancelar(int reservaId);
}

public interface IPasajeroService
{
    int Registrar(Pasajero pasajero);
    void Actualizar(Pasajero pasajero);
    void Eliminar(int id);
}

public interface IVueloService
{
    int Crear(string numero, int origenId, int destinoId, DateTime salida, int duracionMinutos, int filas, decimal precio);
}

public interface IEquipajeService
{
    int Agregar(int reservaId, TipoEquipaje tipo, decimal pesoKg);
}

public interface IAutenticacionService
{
    bool Login(string nombreUsuario, string clave);
    void Logout();
    bool EstaAutenticado();
    bool TieneRol(string rol);
}
