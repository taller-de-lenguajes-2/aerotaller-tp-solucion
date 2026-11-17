using System.ComponentModel.DataAnnotations;

// Un atributo de validación propio (desafío 5, igual que en el tema 13).
// Se escribe una vez y se usa en cualquier propiedad DateTime.
// Este se ejecuta solo en el servidor (no tiene versión en JavaScript).
public class FechaFuturaAttribute : ValidationAttribute
{
    public FechaFuturaAttribute()
        : base("{0}: tiene que ser posterior a este momento.") { }

    public override bool IsValid(object? value)
    {
        // El caso null lo resuelve [Required]: aquí solo se evalúa la fecha si existe
        return value is not DateTime fecha || fecha > DateTime.Now;
    }
}
