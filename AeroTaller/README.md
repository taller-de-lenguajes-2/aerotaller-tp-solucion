# AeroTaller — solución de referencia

La aerolínea del TP integrador, completa: el resultado de los desafíos 0 a 9 más el simulacro (equipaje).
Es material de referencia para la cátedra: se utiliza para comparar resoluciones y no está destinado a distribuirse a los alumnos.

## Ejecución

```bash
cd AeroTaller
dotnet run
```

Se abre en `http://localhost:5210`. La primera vez, la aplicación crea `aerotaller.db` a partir de `aerotaller.sql`
(para empezar de cero, borrar `aerotaller.db` con la aplicación detenida).

**Usuarios de prueba** (en la tabla `Usuario`, en texto plano solo para el TP):

| Usuario | Clave | Rol | Permisos |
|---|---|---|---|
| `mostrador` | `vuelos2026` | Mostrador | pasajeros, reservas, equipaje |
| `admin` | `aero2026` | Admin | todo lo anterior + crear vuelos + eliminar pasajeros |

Sin iniciar sesión pueden consultarse los vuelos y el tablero.

### Con repositorios falsos (sin base de datos)

```bash
dotnet run --launch-profile datos-falsos
# o también:
dotnet run -- --OrigenDeDatos=Falso
```

En la parte superior aparece un aviso amarillo: los datos están en memoria (`Repositorios/Falsos/`). En `Program.cs` lo
único que cambia es el bloque de registros del contenedor; controladores, servicios y vistas son exactamente los mismos.

## Mapa del código

| Carpeta / archivo | Contenido | Desafío |
|---|---|---|
| `aerotaller.sql` | tablas, claves foráneas, índice único parcial, datos con fechas relativas a hoy | 1 |
| `Program.cs` | `CrearBaseSiNoExiste`, contenedor, Serilog, pipeline | 1 · 7 · 8 · 9 |
| `Models/` | dominio: `Vuelo`, `Pasajero`, `Reserva`, `Equipaje`, `Usuario` | 2 |
| `Repositorios/` | ADO.NET con parámetros; `Interfaces.cs`; `ConexionDb` | 2 · 7 |
| `Repositorios/Falsos/` | las mismas interfaces en memoria | 7 |
| `Controllers/` | controladores delgados: reciben el request, invocan al servicio, seleccionan la vista | 3 · 6 |
| `Views/` | layout, tag helpers, mapa de asientos, formularios | 3 |
| `ViewModels/` | un ViewModel por pantalla | 4 |
| `Validaciones/FechaFuturaAttribute.cs` | atributo propio | 5 |
| `Servicios/` | reglas del negocio + `AutenticacionService` | 6 · 9 |
| `Excepciones/ReglaDeNegocioException.cs` | excepción propia con el campo del formulario | 6 · 8 |
| `Filtros/` | `[RequiereLogin]`, `[RequiereRol]` | 9 |
| `logs/` | se crea al ejecutar la aplicación: un archivo por día | 8 |

## Pruebas con el depurador

- **Choque de reservas**: colocar un punto de interrupción en `reservas.Insertar(reserva)` de `ReservaService.Reservar` y
  otro en el `catch (SqliteException ex) when …`. Reservar un asiento libre; con la ejecución detenida, insertar desde
  DB Browser (y escribir los cambios) una reserva confirmada para el mismo vuelo y asiento. Al continuar con F5, el índice
  único rechaza el INSERT y la ejecución entra en el `catch`. (El verificador del desafío 8 prueba lo mismo con 8 reservas
  simultáneas.)
- **Página de error**: ejecutar con la base en solo lectura e intentar una reserva:
  `dotnet run -- "--ConnectionStrings:AeroTaller=Data Source=aerotaller.db;Mode=ReadOnly"`.
  El código que muestra la página se encuentra en `logs/`.
- **Filtro de login**: punto de interrupción en `RequiereLoginAttribute.OnActionExecuting` y acceso a `/Reservas` sin sesión.
