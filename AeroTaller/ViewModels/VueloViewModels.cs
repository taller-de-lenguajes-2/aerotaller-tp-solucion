using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

// ViewModels de la pantalla de vuelos (desafío 4).
// Un ViewModel por pantalla: contiene EXACTAMENTE los datos que la vista muestra, ya formateados.

// Una fila del listado
public class VueloListadoViewModel(Vuelo vuelo, int ocupados)
{
    public int Id { get; } = vuelo.Id;
    public string Numero { get; } = vuelo.Numero;
    public string Ruta { get; } = $"{vuelo.Origen.Codigo} → {vuelo.Destino.Codigo}";
    public string Ciudades { get; } = $"{vuelo.Origen.Ciudad} → {vuelo.Destino.Ciudad}";
    public string Salida { get; } = vuelo.Salida.ToString("ddd dd/MM HH:mm");
    public string Llegada { get; } = vuelo.Llegada.ToString("HH:mm") + (vuelo.LlegaAlOtroDia ? " (+1)" : "");
    public string Duracion { get; } = $"{vuelo.DuracionMinutos / 60} h {vuelo.DuracionMinutos % 60:00} min";
    public string Precio { get; } = vuelo.Precio.ToString("C0");
    public int Libres { get; } = vuelo.Capacidad - ocupados;
    public bool YaSalio { get; } = vuelo.YaSalio;
}

// La pantalla de búsqueda: el filtro (lo que envía el usuario) + los resultados
public class VueloIndexViewModel
{
    [Display(Name = "Destino")]
    public int? DestinoId { get; set; }

    [Display(Name = "Desde")]
    [DataType(DataType.Date)]
    public DateTime? Desde { get; set; }

    [ValidateNever] public List<SelectListItem> Destinos { get; set; } = [];
    [ValidateNever] public List<VueloListadoViewModel> Resultados { get; set; } = [];
}

// Un asiento del mapa
public record AsientoViewModel(string Codigo, bool Ocupado);

// El detalle con el mapa de asientos: una matriz Filas × 6 ya construida, de modo que la vista solo deba dibujarla
public class VueloDetalleViewModel
{
    public int Id { get; }
    public string Numero { get; }
    public string Ruta { get; }
    public string Salida { get; }
    public string Llegada { get; }
    public string Precio { get; }
    public int Capacidad { get; }
    public int Ocupados { get; }
    public bool PuedeReservar { get; }
    public decimal KilosEnBodega { get; }

    // Cada elemento es una fila del avión; cada fila tiene 6 asientos (A–F)
    public List<List<AsientoViewModel>> Mapa { get; } = [];

    public VueloDetalleViewModel(Vuelo vuelo, List<string> asientosOcupados, decimal kilosEnBodega)
    {
        Id = vuelo.Id;
        Numero = vuelo.Numero;
        Ruta = $"{vuelo.Origen} → {vuelo.Destino}";
        Salida = vuelo.Salida.ToString("dddd dd/MM/yyyy HH:mm");
        Llegada = vuelo.Llegada.ToString("dddd dd/MM/yyyy HH:mm");
        Precio = vuelo.Precio.ToString("C0");
        Capacidad = vuelo.Capacidad;
        Ocupados = asientosOcupados.Count;
        PuedeReservar = !vuelo.YaSalio && Ocupados < Capacidad;
        KilosEnBodega = kilosEnBodega;

        var ocupados = asientosOcupados.ToHashSet();   // HashSet: "¿está?" sin recorrer toda la lista
        for (int fila = 1; fila <= vuelo.Filas; fila++)
        {
            var asientos = new List<AsientoViewModel>();
            foreach (char letra in Vuelo.LetrasAsiento)
            {
                string codigo = $"{fila}{letra}";
                asientos.Add(new AsientoViewModel(codigo, ocupados.Contains(codigo)));
            }
            Mapa.Add(asientos);
        }
    }
}

// El formulario de alta (solo Admin)
public class VueloCrearViewModel
{
    [Display(Name = "Número de vuelo")]
    [Required(ErrorMessage = "El número es obligatorio.")]
    [RegularExpression(@"^[Aa][Tt]\d{4}$", ErrorMessage = "El número es AT seguido de 4 dígitos (ej.: AT1234).")]
    public string Numero { get; set; } = "";

    [Display(Name = "Origen")]
    [Required(ErrorMessage = "El origen es obligatorio.")]
    public int? OrigenId { get; set; }

    [Display(Name = "Destino")]
    [Required(ErrorMessage = "El destino es obligatorio.")]
    public int? DestinoId { get; set; }

    [Display(Name = "Salida")]
    [Required(ErrorMessage = "La fecha de salida es obligatoria.")]
    [FechaFutura]
    public DateTime? Salida { get; set; } = DateTime.Today.AddDays(7).AddHours(8);

    [Display(Name = "Duración (minutos)")]
    [Required(ErrorMessage = "La duración es obligatoria.")]
    [Range(20, 1200, ErrorMessage = "La duración va de {1} a {2} minutos.")]
    public int? DuracionMinutos { get; set; }

    [Display(Name = "Filas de asientos")]
    [Required(ErrorMessage = "La cantidad de filas es obligatoria.")]
    [Range(1, 40, ErrorMessage = "El avión tiene entre {1} y {2} filas.")]
    public int? Filas { get; set; }

    [Display(Name = "Precio")]
    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(1000, 5000000, ErrorMessage = "El precio va de {1} a {2}.")]
    public decimal? Precio { get; set; }

    [ValidateNever] public List<SelectListItem> Aeropuertos { get; set; } = [];
}
