using System.Globalization;
using Microsoft.Data.Sqlite;

// La reserva reúne todo: vuelo (con sus dos aeropuertos) + pasajero → JOIN de 4 tablas (5 alias).
public class ReservaRepository(ConexionDb conexionDb) : IReservaRepository
{
    private readonly string cadenaConexion = conexionDb.Cadena;

    // Columnas 0–3: reserva · 4–17: vuelo y aeropuertos (mismo orden que VueloRepository) · 18–21: pasajero
    private const string SelectConJoin = """
        SELECT r.Id, r.Asiento, r.Fecha, r.Estado,
               v.Id, v.Numero, v.Salida, v.DuracionMinutos, v.Filas, v.Precio,
               o.Id, o.Codigo, o.Ciudad, o.Nombre,
               d.Id, d.Codigo, d.Ciudad, d.Nombre,
               p.Id, p.Nombre, p.Dni, p.Email
        FROM Reserva r
        INNER JOIN Vuelo v      ON r.VueloId    = v.Id
        INNER JOIN Aeropuerto o ON v.OrigenId   = o.Id
        INNER JOIN Aeropuerto d ON v.DestinoId  = d.Id
        INNER JOIN Pasajero p   ON r.PasajeroId = p.Id
        """;

    public List<Reserva> Listar(EstadoReserva? estado)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand { Connection = conexion };
        comando.CommandText = SelectConJoin;
        if (estado is not null)
        {
            comando.CommandText += " WHERE r.Estado = @estado";
            comando.Parameters.AddWithValue("@estado", estado.ToString());   // el enum se guarda como texto
        }
        comando.CommandText += " ORDER BY v.Salida, r.Asiento";
        return LeerReservas(comando);
    }

    public List<Reserva> ListarPorPasajero(int pasajeroId)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand(SelectConJoin + " WHERE r.PasajeroId = @pasajeroId ORDER BY v.Salida", conexion);
        comando.Parameters.AddWithValue("@pasajeroId", pasajeroId);
        return LeerReservas(comando);
    }

    public Reserva? Obtener(int id)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand(SelectConJoin + " WHERE r.Id = @id", conexion);
        comando.Parameters.AddWithValue("@id", id);
        return LeerReservas(comando).FirstOrDefault();
    }

    public List<string> AsientosOcupados(int vueloId)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand(
            "SELECT Asiento FROM Reserva WHERE VueloId = @vueloId AND Estado = 'Confirmada'", conexion);
        comando.Parameters.AddWithValue("@vueloId", vueloId);

        var asientos = new List<string>();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            asientos.Add(lector.GetString(0));
        }
        return asientos;
    }

    public bool PasajeroTieneReserva(int vueloId, int pasajeroId)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand("""
            SELECT COUNT(*) FROM Reserva
            WHERE VueloId = @vueloId AND PasajeroId = @pasajeroId AND Estado = 'Confirmada'
            """, conexion);
        comando.Parameters.AddWithValue("@vueloId", vueloId);
        comando.Parameters.AddWithValue("@pasajeroId", pasajeroId);
        return Convert.ToInt32(comando.ExecuteScalar()) > 0;
    }

    public int ContarConfirmadas(int vueloId)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand(
            "SELECT COUNT(*) FROM Reserva WHERE VueloId = @vueloId AND Estado = 'Confirmada'", conexion);
        comando.Parameters.AddWithValue("@vueloId", vueloId);
        return Convert.ToInt32(comando.ExecuteScalar());
    }

    public int Insertar(Reserva reserva)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        var sql = """
            INSERT INTO Reserva (VueloId, PasajeroId, Asiento, Fecha, Estado)
            VALUES (@vueloId, @pasajeroId, @asiento, @fecha, @estado)
            RETURNING Id
            """;
        using var comando = new SqliteCommand(sql, conexion);
        comando.Parameters.AddWithValue("@vueloId", reserva.Vuelo.Id);
        comando.Parameters.AddWithValue("@pasajeroId", reserva.Pasajero.Id);
        comando.Parameters.AddWithValue("@asiento", reserva.Asiento);
        comando.Parameters.AddWithValue("@fecha", reserva.Fecha.ToString(VueloRepository.FormatoFecha, CultureInfo.InvariantCulture));
        comando.Parameters.AddWithValue("@estado", reserva.Estado.ToString());

        return Convert.ToInt32(comando.ExecuteScalar());
    }

    public bool CambiarEstado(int id, EstadoReserva estado)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand("UPDATE Reserva SET Estado = @estado WHERE Id = @id", conexion);
        comando.Parameters.AddWithValue("@estado", estado.ToString());
        comando.Parameters.AddWithValue("@id", id);
        return comando.ExecuteNonQuery() > 0;
    }

    private static List<Reserva> LeerReservas(SqliteCommand comando)
    {
        var reservas = new List<Reserva>();

        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            reservas.Add(new Reserva
            {
                Id = lector.GetInt32(0),
                Asiento = lector.GetString(1),
                Fecha = VueloRepository.LeerFecha(lector, 2),
                Estado = Enum.Parse<EstadoReserva>(lector.GetString(3)),   // texto → enum
                Vuelo = VueloRepository.LeerVuelo(lector, 4),
                Pasajero = PasajeroRepository.LeerPasajero(lector, 18)
            });
        }
        return reservas;
    }
}
