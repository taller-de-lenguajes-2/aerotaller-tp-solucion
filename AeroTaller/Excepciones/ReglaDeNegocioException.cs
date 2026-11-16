// Una excepción propia (desafío 6, como SaldoInsuficienteException del tema 16).
// La lanza un SERVICIO cuando una regla del negocio no se cumple ("ese asiento ya está ocupado").
// Lleva el nombre del campo del formulario, así el controlador puede mostrar el error al lado del input.
public class ReglaDeNegocioException(string campo, string mensaje, Exception? causa = null)
    : Exception(mensaje, causa)
{
    // "" = error general del formulario (se muestra en el asp-validation-summary)
    public string Campo { get; } = campo;
}
