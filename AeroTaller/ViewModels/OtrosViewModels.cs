using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Display(Name = "Usuario")]
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    public string NombreUsuario { get; set; } = "";

    [Display(Name = "Contraseña")]
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]                       // asp-for genera type="password"
    public string Clave { get; set; } = "";

    // A dónde volver después de iniciar sesión
    public string? ReturnUrl { get; set; }
}

// La pantalla de error: solo el Id para buscar el request en el log (nunca el detalle técnico)
public record ErrorViewModel(string? RequestId);
