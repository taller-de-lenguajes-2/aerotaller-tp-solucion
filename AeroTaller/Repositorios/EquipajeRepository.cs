using Microsoft.Data.Sqlite;

// Simulacro de parcial: el equipaje de cada reserva.
public class EquipajeRepository(ConexionDb conexionDb) : IEquipajeRepository
{
    private readonly string cadenaConexion = conexionDb.Cadena;

    public List<Equipaje> ListarPorReserva(int reservaId)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand(
            "SELECT Id, ReservaId, Tipo, PesoKg FROM Equipaje WHERE ReservaId = @reservaId ORDER BY Id", conexion);
        comando.Parameters.AddWithValue("@reservaId", reservaId);

        var piezas = new List<Equipaje>();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            piezas.Add(new Equipaje
            {
                Id = lector.GetInt32(0),
                ReservaId = lector.GetInt32(1),
                Tipo = Enum.Parse<TipoEquipaje>(lector.GetString(2)),
                PesoKg = lector.GetDecimal(3)
            });
        }
        return piezas;
    }

    public int Insertar(Equipaje equipaje)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand("""
            INSERT INTO Equipaje (ReservaId, Tipo, PesoKg)
            VALUES (@reservaId, @tipo, @peso)
            RETURNING Id
            """, conexion);
        comando.Parameters.AddWithValue("@reservaId", equipaje.ReservaId);
        comando.Parameters.AddWithValue("@tipo", equipaje.Tipo.ToString());
        comando.Parameters.AddWithValue("@peso", equipaje.PesoKg);

        return Convert.ToInt32(comando.ExecuteScalar());
    }

    // SUM con JOIN: los kilos que van en la bodega de un vuelo (solo reservas confirmadas).
    // COALESCE: si no hay equipaje, SUM devuelve NULL; así devuelve 0.
    public decimal PesoEnBodega(int vueloId)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand("""
            SELECT COALESCE(SUM(e.PesoKg), 0)
            FROM Equipaje e
            INNER JOIN Reserva r ON e.ReservaId = r.Id
            WHERE r.VueloId = @vueloId AND r.Estado = 'Confirmada' AND e.Tipo = 'Bodega'
            """, conexion);
        comando.Parameters.AddWithValue("@vueloId", vueloId);
        return Convert.ToDecimal(comando.ExecuteScalar());
    }
}
