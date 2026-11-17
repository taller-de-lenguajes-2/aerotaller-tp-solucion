// La cadena de conexión envuelta en un tipo propio (desafío 7, igual que en el tema 14).
// El contenedor resuelve las dependencias por TIPO: si un repositorio declarara un string simple como dependencia,
// no habría forma de determinar cuál proporcionar. Con ConexionDb no hay ambigüedad.
// Program.cs la crea una vez al arrancar y la registra como Singleton (es un dato que no cambia).
public record ConexionDb(string Cadena);
