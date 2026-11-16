using System.ComponentModel.DataAnnotations;

// El mismo formulario sirve para crear (Id null) y para editar (Id con valor)
public class PasajeroFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Nombre y apellido")]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre tiene que tener entre {2} y {1} caracteres.")]
    public string Nombre { get; set; } = "";

    [Display(Name = "DNI")]
    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El DNI son 7 u 8 números, sin puntos.")]
    public string Dni { get; set; } = "";

    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Ese email no es válido.")]
    public string? Email { get; set; }

    public bool EsNuevo => Id is null;

    public PasajeroFormViewModel() { }

    // Entidad → ViewModel (para editar)
    public PasajeroFormViewModel(Pasajero pasajero)
    {
        Id = pasajero.Id;
        Nombre = pasajero.Nombre;
        Dni = pasajero.Dni;
        Email = pasajero.Email;
    }

    // ViewModel → entidad: el controlador traduce; el repositorio nunca ve un ViewModel
    public Pasajero APasajero() => new()
    {
        Id = Id ?? 0,
        Nombre = Nombre.Trim(),
        Dni = Dni,
        Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim()
    };
}

public class PasajeroDetalleViewModel(Pasajero pasajero, List<Reserva> reservas)
{
    public int Id { get; } = pasajero.Id;
    public string Nombre { get; } = pasajero.Nombre;
    public string Dni { get; } = pasajero.Dni;
    public string Email { get; } = pasajero.Email ?? "—";
    public List<ReservaListadoViewModel> Reservas { get; } = reservas.Select(r => new ReservaListadoViewModel(r)).ToList();
    public bool PuedeEliminarse => Reservas.Count == 0;
}
