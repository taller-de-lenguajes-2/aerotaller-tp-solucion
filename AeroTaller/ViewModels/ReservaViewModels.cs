using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

public class ReservaListadoViewModel(Reserva reserva)
{
    public int Id { get; } = reserva.Id;
    public int VueloId { get; } = reserva.Vuelo.Id;
    public string Vuelo { get; } = reserva.Vuelo.Numero;
    public string Ruta { get; } = $"{reserva.Vuelo.Origen.Codigo} → {reserva.Vuelo.Destino.Codigo}";
    public string Salida { get; } = reserva.Vuelo.Salida.ToString("dd/MM HH:mm");
    public int PasajeroId { get; } = reserva.Pasajero.Id;
    public string Pasajero { get; } = reserva.Pasajero.Nombre;
    public string Asiento { get; } = reserva.Asiento;
    public string Estado { get; } = reserva.Estado.ToString();
    public bool Cancelada { get; } = reserva.Estado == EstadoReserva.Cancelada;
    public bool PuedeCancelarse { get; } = reserva.Estado == EstadoReserva.Confirmada && !reserva.Vuelo.YaSalio;
}

public class ReservaIndexViewModel
{
    public EstadoReserva? Estado { get; set; }
    public List<ReservaListadoViewModel> Reservas { get; set; } = [];
}

public class ReservaCrearViewModel
{
    // int? para que la ausencia de selección llegue como null y la detecte [Required]
    [Display(Name = "Vuelo")]
    [Required(ErrorMessage = "El vuelo es obligatorio.")]
    public int? VueloId { get; set; }

    [Display(Name = "Pasajero")]
    [Required(ErrorMessage = "El pasajero es obligatorio.")]
    public int? PasajeroId { get; set; }

    [Display(Name = "Asiento")]
    [Required(ErrorMessage = "El asiento es obligatorio.")]
    [RegularExpression(@"^\s*\d{1,2}[A-Fa-f]\s*$", ErrorMessage = "El asiento es la fila y una letra de la A a la F (ej.: 12C).")]
    public string Asiento { get; set; } = "";

    // Las listas no las envía el usuario: no se validan (y deben recargarse si el POST falla)
    [ValidateNever] public List<SelectListItem> Vuelos { get; set; } = [];
    [ValidateNever] public List<SelectListItem> Pasajeros { get; set; } = [];
}

// Simulacro: detalle de la reserva con su equipaje
public class ReservaDetalleViewModel(Reserva reserva, List<Equipaje> equipaje)
{
    public ReservaListadoViewModel Reserva { get; } = new(reserva);
    public List<Equipaje> Equipaje { get; } = equipaje;
    public decimal TotalKg { get; } = equipaje.Sum(e => e.PesoKg);
}

public class EquipajeCrearViewModel
{
    public int ReservaId { get; set; }

    [Display(Name = "Tipo")]
    [Required(ErrorMessage = "El tipo es obligatorio.")]
    public TipoEquipaje? Tipo { get; set; }

    [Display(Name = "Peso (kg)")]
    [Required(ErrorMessage = "El peso es obligatorio.")]
    [Range(0.1, 50, ErrorMessage = "El peso va de {1} a {2} kg.")]
    public decimal? PesoKg { get; set; }

    [ValidateNever] public ReservaListadoViewModel? Reserva { get; set; }
}
