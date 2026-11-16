// El dominio de AeroTaller (desafío 2).
// Son clases "de negocio": no saben nada de SQL, de HTML ni de formularios.

public class Aeropuerto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";      // IATA: "TUC"
    public string Ciudad { get; set; } = "";
    public string Nombre { get; set; } = "";

    public override string ToString() => $"{Codigo} · {Ciudad}";
}

public class Vuelo
{
    // Cada fila del avión tiene 6 asientos: A B C | pasillo | D E F
    public const string LetrasAsiento = "ABCDEF";

    public int Id { get; set; }
    public string Numero { get; set; } = "";       // "AT1001"

    // Composición con Aeropuerto: el vuelo "tiene" un origen y un destino (no sus Id sueltos)
    public Aeropuerto Origen { get; set; } = null!;
    public Aeropuerto Destino { get; set; } = null!;

    public DateTime Salida { get; set; }
    public int DuracionMinutos { get; set; }
    public int Filas { get; set; }
    public decimal Precio { get; set; }

    // Propiedades calculadas: no se guardan en la base, se deducen de las otras
    public DateTime Llegada => Salida.AddMinutes(DuracionMinutos);
    public int Capacidad => Filas * LetrasAsiento.Length;
    public bool YaSalio => Salida <= DateTime.Now;
    public bool LlegaAlOtroDia => Llegada.Date > Salida.Date;

    // "12A" es un asiento de este avión si la fila existe y la letra es A–F
    public bool TieneAsiento(string asiento)
    {
        if (string.IsNullOrWhiteSpace(asiento) || asiento.Length < 2) return false;

        char letra = char.ToUpperInvariant(asiento[^1]);
        bool filaValida = int.TryParse(asiento[..^1], out int fila) && fila >= 1 && fila <= Filas;
        return filaValida && LetrasAsiento.Contains(letra);
    }
}

public class Pasajero
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Dni { get; set; } = "";
    public string? Email { get; set; }
}

// Un enum en vez de un string suelto: el compilador no deja escribir "Confirmda"
public enum EstadoReserva
{
    Confirmada,
    Cancelada
}

public class Reserva
{
    public int Id { get; set; }
    public Vuelo Vuelo { get; set; } = null!;
    public Pasajero Pasajero { get; set; } = null!;
    public string Asiento { get; set; } = "";
    public DateTime Fecha { get; set; }
    public EstadoReserva Estado { get; set; } = EstadoReserva.Confirmada;
}

// Simulacro de parcial
public enum TipoEquipaje
{
    Mano,
    Bodega
}

public class Equipaje
{
    public const decimal PesoMaximoBodega = 23;
    public const decimal PesoMaximoMano = 10;
    public const int PiezasMaximasBodega = 2;

    public int Id { get; set; }
    public int ReservaId { get; set; }
    public TipoEquipaje Tipo { get; set; }
    public decimal PesoKg { get; set; }
}

// El resultado de una consulta con GROUP BY: no es una tabla, es un "reporte"
public record OcupacionVuelo(int VueloId, string Numero, string Ruta, DateTime Salida, int Capacidad, int Ocupados)
{
    public int Porcentaje => Capacidad == 0 ? 0 : Ocupados * 100 / Capacidad;
}
