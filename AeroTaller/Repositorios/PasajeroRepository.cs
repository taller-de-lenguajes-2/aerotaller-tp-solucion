using Microsoft.Data.Sqlite;

// El CRUD completo de una tabla (desafío 2): Listar, Buscar, Obtener, Insertar, Actualizar, Eliminar.
public class PasajeroRepository(ConexionDb conexionDb) : IPasajeroRepository
{
    private readonly string cadenaConexion = conexionDb.Cadena;

    private const string Select = "SELECT Id, Nombre, Dni, Email FROM Pasajero";

    public List<Pasajero> Listar()
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand(Select + " ORDER BY Nombre", conexion);
        return LeerPasajeros(comando);
    }

    // Los % van en el VALOR del parámetro, no en el SQL
    public List<Pasajero> Buscar(string texto)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand(Select + " WHERE Nombre LIKE @texto OR Dni LIKE @texto ORDER BY Nombre", conexion);
        comando.Parameters.AddWithValue("@texto", $"%{texto}%");
        return LeerPasajeros(comando);
    }

    public Pasajero? Obtener(int id)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand(Select + " WHERE Id = @id", conexion);
        comando.Parameters.AddWithValue("@id", id);
        return LeerPasajeros(comando).FirstOrDefault();
    }

    // "excepto": al editar, el propio pasajero no cuenta como duplicado
    public bool ExisteDni(string dni, int? excepto = null)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand("SELECT COUNT(*) FROM Pasajero WHERE Dni = @dni AND Id <> @excepto", conexion);
        comando.Parameters.AddWithValue("@dni", dni);
        comando.Parameters.AddWithValue("@excepto", excepto ?? 0);
        return Convert.ToInt32(comando.ExecuteScalar()) > 0;
    }

    public int Insertar(Pasajero pasajero)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        var sql = """
            INSERT INTO Pasajero (Nombre, Dni, Email)
            VALUES (@nombre, @dni, @email)
            RETURNING Id
            """;
        using var comando = new SqliteCommand(sql, conexion);
        comando.Parameters.AddWithValue("@nombre", pasajero.Nombre);
        comando.Parameters.AddWithValue("@dni", pasajero.Dni);
        comando.Parameters.AddWithValue("@email", (object?)pasajero.Email ?? DBNull.Value);   // null de C# → NULL de SQL

        return Convert.ToInt32(comando.ExecuteScalar());
    }

    // ExecuteNonQuery devuelve cuántas filas cambiaron: 0 = no existía ese Id
    public bool Actualizar(Pasajero pasajero)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        var sql = "UPDATE Pasajero SET Nombre = @nombre, Dni = @dni, Email = @email WHERE Id = @id";
        using var comando = new SqliteCommand(sql, conexion);
        comando.Parameters.AddWithValue("@nombre", pasajero.Nombre);
        comando.Parameters.AddWithValue("@dni", pasajero.Dni);
        comando.Parameters.AddWithValue("@email", (object?)pasajero.Email ?? DBNull.Value);
        comando.Parameters.AddWithValue("@id", pasajero.Id);

        return comando.ExecuteNonQuery() > 0;
    }

    // Si el pasajero tiene reservas, la clave foránea impide borrarlo:
    // SQLite lanza SqliteException y el repositorio NO la captura (su manejo no le corresponde).
    // Eso se resuelve en el servicio (desafío 6).
    public bool Eliminar(int id)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand("DELETE FROM Pasajero WHERE Id = @id", conexion);
        comando.Parameters.AddWithValue("@id", id);
        return comando.ExecuteNonQuery() > 0;
    }

    private static List<Pasajero> LeerPasajeros(SqliteCommand comando)
    {
        var pasajeros = new List<Pasajero>();

        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            pasajeros.Add(LeerPasajero(lector, 0));
        }
        return pasajeros;
    }

    public static Pasajero LeerPasajero(SqliteDataReader lector, int desde) => new()
    {
        Id = lector.GetInt32(desde),
        Nombre = lector.GetString(desde + 1),
        Dni = lector.GetString(desde + 2),
        Email = lector.IsDBNull(desde + 3) ? null : lector.GetString(desde + 3)
    };
}
