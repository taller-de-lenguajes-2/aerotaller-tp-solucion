using System.Globalization;
using Microsoft.Data.Sqlite;

// La tabla Vuelo tiene DOS claves foráneas a Aeropuerto (origen y destino):
// para armar el objeto completo hace falta un JOIN doble, con un alias para cada aeropuerto.
public class VueloRepository(ConexionDb conexionDb) : IVueloRepository
{
    private readonly string cadenaConexion = conexionDb.Cadena;

    // Las fechas se guardan como texto: 'AAAA-MM-DD HH:MM'
    public const string FormatoFecha = "yyyy-MM-dd HH:mm";

    private const string SelectConJoin = """
        SELECT v.Id, v.Numero, v.Salida, v.DuracionMinutos, v.Filas, v.Precio,
               o.Id, o.Codigo, o.Ciudad, o.Nombre,
               d.Id, d.Codigo, d.Ciudad, d.Nombre
        FROM Vuelo v
        INNER JOIN Aeropuerto o ON v.OrigenId  = o.Id
        INNER JOIN Aeropuerto d ON v.DestinoId = d.Id
        """;

    public List<Vuelo> Listar()
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand(SelectConJoin + " ORDER BY v.Salida", conexion);
        return LeerVuelos(comando);
    }

    // Filtros opcionales: el WHERE se arma solo con los que vinieron, pero SIEMPRE con parámetros
    public List<Vuelo> Buscar(int? destinoId, DateTime? desde)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        var condiciones = new List<string>();
        using var comando = new SqliteCommand { Connection = conexion };

        if (destinoId is not null)
        {
            condiciones.Add("v.DestinoId = @destinoId");
            comando.Parameters.AddWithValue("@destinoId", destinoId);
        }
        if (desde is not null)
        {
            // Como la fecha es texto con formato AAAA-MM-DD, comparar textos = comparar fechas
            condiciones.Add("v.Salida >= @desde");
            comando.Parameters.AddWithValue("@desde", desde.Value.ToString(FormatoFecha, CultureInfo.InvariantCulture));
        }

        string where = condiciones.Count > 0 ? " WHERE " + string.Join(" AND ", condiciones) : "";
        comando.CommandText = SelectConJoin + where + " ORDER BY v.Salida";
        return LeerVuelos(comando);
    }

    public Vuelo? Obtener(int id)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand(SelectConJoin + " WHERE v.Id = @id", conexion);
        comando.Parameters.AddWithValue("@id", id);
        return LeerVuelos(comando).FirstOrDefault();
    }

    public bool ExisteNumero(string numero)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        using var comando = new SqliteCommand("SELECT COUNT(*) FROM Vuelo WHERE Numero = @numero", conexion);
        comando.Parameters.AddWithValue("@numero", numero);
        return Convert.ToInt32(comando.ExecuteScalar()) > 0;
    }

    // INSERT … RETURNING Id: ExecuteScalar devuelve el Id que generó la base
    public int Insertar(Vuelo vuelo)
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        var sql = """
            INSERT INTO Vuelo (Numero, OrigenId, DestinoId, Salida, DuracionMinutos, Filas, Precio)
            VALUES (@numero, @origenId, @destinoId, @salida, @duracion, @filas, @precio)
            RETURNING Id
            """;
        using var comando = new SqliteCommand(sql, conexion);
        comando.Parameters.AddWithValue("@numero", vuelo.Numero);
        comando.Parameters.AddWithValue("@origenId", vuelo.Origen.Id);
        comando.Parameters.AddWithValue("@destinoId", vuelo.Destino.Id);
        comando.Parameters.AddWithValue("@salida", vuelo.Salida.ToString(FormatoFecha, CultureInfo.InvariantCulture));
        comando.Parameters.AddWithValue("@duracion", vuelo.DuracionMinutos);
        comando.Parameters.AddWithValue("@filas", vuelo.Filas);
        comando.Parameters.AddWithValue("@precio", vuelo.Precio);

        return Convert.ToInt32(comando.ExecuteScalar());
    }

    // LEFT JOIN + GROUP BY: cuántas reservas confirmadas tiene cada vuelo futuro.
    // LEFT para que también aparezcan los vuelos SIN reservas (con 0).
    // La condición del estado va en el ON, no en el WHERE: en el WHERE descartaría esos vuelos.
    public List<OcupacionVuelo> ListarOcupacion()
    {
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();

        var sql = """
            SELECT v.Id, v.Numero, o.Codigo || ' → ' || d.Codigo AS Ruta, v.Salida,
                   v.Filas * 6 AS Capacidad, COUNT(r.Id) AS Ocupados
            FROM Vuelo v
            INNER JOIN Aeropuerto o ON v.OrigenId  = o.Id
            INNER JOIN Aeropuerto d ON v.DestinoId = d.Id
            LEFT JOIN Reserva r ON r.VueloId = v.Id AND r.Estado = 'Confirmada'
            WHERE v.Salida >= @ahora
            GROUP BY v.Id, v.Numero, Ruta, v.Salida, v.Filas
            ORDER BY v.Salida
            """;
        using var comando = new SqliteCommand(sql, conexion);
        comando.Parameters.AddWithValue("@ahora", DateTime.Now.ToString(FormatoFecha, CultureInfo.InvariantCulture));

        var resultado = new List<OcupacionVuelo>();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            resultado.Add(new OcupacionVuelo(
                lector.GetInt32(0),
                lector.GetString(1),
                lector.GetString(2),
                LeerFecha(lector, 3),
                lector.GetInt32(4),
                lector.GetInt32(5)));
        }
        return resultado;
    }

    private static List<Vuelo> LeerVuelos(SqliteCommand comando)
    {
        var vuelos = new List<Vuelo>();

        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            vuelos.Add(LeerVuelo(lector, 0));
        }
        return vuelos;
    }

    // Lee un vuelo (con sus dos aeropuertos) a partir de la columna "desde".
    // Es public static porque ReservaRepository reutiliza el mismo bloque de columnas en su JOIN.
    public static Vuelo LeerVuelo(SqliteDataReader lector, int desde) => new()
    {
        Id = lector.GetInt32(desde),
        Numero = lector.GetString(desde + 1),
        Salida = LeerFecha(lector, desde + 2),
        DuracionMinutos = lector.GetInt32(desde + 3),
        Filas = lector.GetInt32(desde + 4),
        Precio = lector.GetDecimal(desde + 5),
        Origen = new Aeropuerto
        {
            Id = lector.GetInt32(desde + 6),
            Codigo = lector.GetString(desde + 7),
            Ciudad = lector.GetString(desde + 8),
            Nombre = lector.GetString(desde + 9)
        },
        Destino = new Aeropuerto
        {
            Id = lector.GetInt32(desde + 10),
            Codigo = lector.GetString(desde + 11),
            Ciudad = lector.GetString(desde + 12),
            Nombre = lector.GetString(desde + 13)
        }
    };

    public static DateTime LeerFecha(SqliteDataReader lector, int columna) =>
        DateTime.ParseExact(lector.GetString(columna), FormatoFecha, CultureInfo.InvariantCulture);
}
