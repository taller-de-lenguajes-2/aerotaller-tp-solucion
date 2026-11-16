using Microsoft.Data.Sqlite;

// En el tema 15 los usuarios estaban en una lista en memoria; acá están en la tabla Usuario (desafío 9).
public class UsuarioRepository(ConexionDb conexionDb) : IUsuarioRepository
{
    private readonly string cadenaConexion = conexionDb.Cadena;

    public Usuario? Obtener(string nombreUsuario)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand(
            "SELECT Id, NombreUsuario, Nombre, Clave, Rol FROM Usuario WHERE NombreUsuario = @usuario", conexion);
        comando.Parameters.AddWithValue("@usuario", nombreUsuario);

        using var lector = comando.ExecuteReader();
        if (!lector.Read()) return null;

        return new Usuario
        {
            Id = lector.GetInt32(0),
            NombreUsuario = lector.GetString(1),
            Nombre = lector.GetString(2),
            Clave = lector.GetString(3),
            Rol = lector.GetString(4)
        };
    }
}
