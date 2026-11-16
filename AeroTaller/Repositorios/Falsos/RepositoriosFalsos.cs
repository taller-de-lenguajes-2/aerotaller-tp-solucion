// Implementaciones "falsas" de las mismas interfaces (desafío 7): sin SQL, sin archivo .db.
// Los servicios y los controladores NO se enteran del cambio: dependen de la interfaz.
// Se activan con "OrigenDeDatos": "Falso" en appsettings.json (o: dotnet run --OrigenDeDatos=Falso).
//
// Para qué sirven:
//   · probar la app sin base (o con la base rota),
//   · mostrar que cambiar de implementación es cambiar el REGISTRO en Program.cs, no el resto del código.

public class AeropuertoRepositoryFalso(BaseFalsa db) : IAeropuertoRepository
{
    public List<Aeropuerto> Listar()
    {
        lock (db.Candado) return db.Aeropuertos.OrderBy(a => a.Ciudad).ToList();
    }

    public Aeropuerto? Obtener(int id)
    {
        lock (db.Candado) return db.Aeropuertos.FirstOrDefault(a => a.Id == id);
    }
}

public class VueloRepositoryFalso(BaseFalsa db) : IVueloRepository
{
    public List<Vuelo> Listar()
    {
        lock (db.Candado) return db.Vuelos.OrderBy(v => v.Salida).ToList();
    }

    public List<Vuelo> Buscar(int? destinoId, DateTime? desde)
    {
        lock (db.Candado)
        {
            return db.Vuelos
                .Where(v => destinoId is null || v.Destino.Id == destinoId)
                .Where(v => desde is null || v.Salida >= desde)
                .OrderBy(v => v.Salida)
                .ToList();
        }
    }

    public Vuelo? Obtener(int id)
    {
        lock (db.Candado) return db.Vuelos.FirstOrDefault(v => v.Id == id);
    }

    public bool ExisteNumero(string numero)
    {
        lock (db.Candado) return db.Vuelos.Any(v => v.Numero == numero);
    }

    public int Insertar(Vuelo vuelo)
    {
        lock (db.Candado)
        {
            vuelo.Id = BaseFalsa.SiguienteId(db.Vuelos, v => v.Id);
            db.Vuelos.Add(vuelo);
            return vuelo.Id;
        }
    }

    // El mismo "GROUP BY" del repositorio SQLite, hecho con LINQ
    public List<OcupacionVuelo> ListarOcupacion()
    {
        lock (db.Candado)
        {
            return db.Vuelos
                .Where(v => !v.YaSalio)
                .OrderBy(v => v.Salida)
                .Select(v => new OcupacionVuelo(
                    v.Id, v.Numero, $"{v.Origen.Codigo} → {v.Destino.Codigo}", v.Salida, v.Capacidad,
                    db.Reservas.Count(r => r.Vuelo.Id == v.Id && r.Estado == EstadoReserva.Confirmada)))
                .ToList();
        }
    }
}

public class PasajeroRepositoryFalso(BaseFalsa db) : IPasajeroRepository
{
    public List<Pasajero> Listar()
    {
        lock (db.Candado) return db.Pasajeros.OrderBy(p => p.Nombre).ToList();
    }

    public List<Pasajero> Buscar(string texto)
    {
        lock (db.Candado)
        {
            return db.Pasajeros
                .Where(p => p.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase) || p.Dni.Contains(texto))
                .OrderBy(p => p.Nombre)
                .ToList();
        }
    }

    public Pasajero? Obtener(int id)
    {
        lock (db.Candado) return db.Pasajeros.FirstOrDefault(p => p.Id == id);
    }

    public bool ExisteDni(string dni, int? excepto = null)
    {
        lock (db.Candado) return db.Pasajeros.Any(p => p.Dni == dni && p.Id != excepto);
    }

    public int Insertar(Pasajero pasajero)
    {
        lock (db.Candado)
        {
            pasajero.Id = BaseFalsa.SiguienteId(db.Pasajeros, p => p.Id);
            db.Pasajeros.Add(pasajero);
            return pasajero.Id;
        }
    }

    public bool Actualizar(Pasajero pasajero)
    {
        lock (db.Candado)
        {
            int i = db.Pasajeros.FindIndex(p => p.Id == pasajero.Id);
            if (i < 0) return false;
            db.Pasajeros[i] = pasajero;
            return true;
        }
    }

    public bool Eliminar(int id)
    {
        lock (db.Candado) return db.Pasajeros.RemoveAll(p => p.Id == id) > 0;
    }
}

public class ReservaRepositoryFalso(BaseFalsa db) : IReservaRepository
{
    public List<Reserva> Listar(EstadoReserva? estado)
    {
        lock (db.Candado)
        {
            return db.Reservas
                .Where(r => estado is null || r.Estado == estado)
                .OrderBy(r => r.Vuelo.Salida).ThenBy(r => r.Asiento)
                .ToList();
        }
    }

    public List<Reserva> ListarPorPasajero(int pasajeroId)
    {
        lock (db.Candado) return db.Reservas.Where(r => r.Pasajero.Id == pasajeroId).OrderBy(r => r.Vuelo.Salida).ToList();
    }

    public Reserva? Obtener(int id)
    {
        lock (db.Candado) return db.Reservas.FirstOrDefault(r => r.Id == id);
    }

    public List<string> AsientosOcupados(int vueloId)
    {
        lock (db.Candado)
        {
            return db.Reservas
                .Where(r => r.Vuelo.Id == vueloId && r.Estado == EstadoReserva.Confirmada)
                .Select(r => r.Asiento)
                .ToList();
        }
    }

    public bool PasajeroTieneReserva(int vueloId, int pasajeroId)
    {
        lock (db.Candado)
        {
            return db.Reservas.Any(r => r.Vuelo.Id == vueloId && r.Pasajero.Id == pasajeroId
                                        && r.Estado == EstadoReserva.Confirmada);
        }
    }

    public int ContarConfirmadas(int vueloId)
    {
        lock (db.Candado) return db.Reservas.Count(r => r.Vuelo.Id == vueloId && r.Estado == EstadoReserva.Confirmada);
    }

    public int Insertar(Reserva reserva)
    {
        lock (db.Candado)
        {
            reserva.Id = BaseFalsa.SiguienteId(db.Reservas, r => r.Id);
            db.Reservas.Add(reserva);
            return reserva.Id;
        }
    }

    public bool CambiarEstado(int id, EstadoReserva estado)
    {
        lock (db.Candado)
        {
            Reserva? reserva = db.Reservas.FirstOrDefault(r => r.Id == id);
            if (reserva is null) return false;
            reserva.Estado = estado;
            return true;
        }
    }
}

public class UsuarioRepositoryFalso(BaseFalsa db) : IUsuarioRepository
{
    public Usuario? Obtener(string nombreUsuario)
    {
        lock (db.Candado) return db.Usuarios.FirstOrDefault(u => u.NombreUsuario == nombreUsuario);
    }
}

public class EquipajeRepositoryFalso(BaseFalsa db) : IEquipajeRepository
{
    public List<Equipaje> ListarPorReserva(int reservaId)
    {
        lock (db.Candado) return db.Equipajes.Where(e => e.ReservaId == reservaId).ToList();
    }

    public int Insertar(Equipaje equipaje)
    {
        lock (db.Candado)
        {
            equipaje.Id = BaseFalsa.SiguienteId(db.Equipajes, e => e.Id);
            db.Equipajes.Add(equipaje);
            return equipaje.Id;
        }
    }

    public decimal PesoEnBodega(int vueloId)
    {
        lock (db.Candado)
        {
            var reservasDelVuelo = db.Reservas
                .Where(r => r.Vuelo.Id == vueloId && r.Estado == EstadoReserva.Confirmada)
                .Select(r => r.Id)
                .ToHashSet();
            return db.Equipajes
                .Where(e => e.Tipo == TipoEquipaje.Bodega && reservasDelVuelo.Contains(e.ReservaId))
                .Sum(e => e.PesoKg);
        }
    }
}
