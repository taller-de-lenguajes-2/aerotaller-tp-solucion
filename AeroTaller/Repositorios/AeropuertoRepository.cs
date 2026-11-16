using Microsoft.Data.Sqlite;

// Todo el SQL de la tabla Aeropuerto vive acá (desafío 2).
public class AeropuertoRepository(ConexionDb conexionDb) : IAeropuertoRepository
{
    private readonly string cadenaConexion = conexionDb.Cadena;

    public List<Aeropuerto> Listar()
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand("SELECT Id, Codigo, Ciudad, Nombre FROM Aeropuerto ORDER BY Ciudad", conexion);
        using var lector = comando.ExecuteReader();

        var aeropuertos = new List<Aeropuerto>();
        while (lector.Read())
        {
            aeropuertos.Add(LeerAeropuerto(lector));
        }
        return aeropuertos;
    }

    public Aeropuerto? Obtener(int id)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand("SELECT Id, Codigo, Ciudad, Nombre FROM Aeropuerto WHERE Id = @id", conexion);
        comando.Parameters.AddWithValue("@id", id);

        using var lector = comando.ExecuteReader();
        return lector.Read() ? LeerAeropuerto(lector) : null;
    }

    private static Aeropuerto LeerAeropuerto(SqliteDataReader lector) => new()
    {
        Id = lector.GetInt32(0),
        Codigo = lector.GetString(1),
        Ciudad = lector.GetString(2),
        Nombre = lector.GetString(3)
    };
}
