# AeroTaller — solución de referencia

La aerolínea del TP integrador, terminada: lo que queda al final de los desafíos 0 a 9 más el simulacro (equipaje).
**No la mires antes de intentar cada desafío**: la idea es que la uses para comparar, no para copiar.

## Cómo correrla

```bash
cd AeroTaller
dotnet run
```

Se abre en `http://localhost:5210`. La primera vez crea `aerotaller.db` con `aerotaller.sql`
(si querés empezar de cero, borrá `aerotaller.db` con la app apagada).

**Usuarios de prueba** (en la tabla `Usuario`, en texto plano solo para el TP):

| Usuario | Clave | Rol | Puede |
|---|---|---|---|
| `mostrador` | `vuelos2026` | Mostrador | pasajeros, reservas, equipaje |
| `admin` | `aero2026` | Admin | todo lo anterior + crear vuelos + eliminar pasajeros |

Sin iniciar sesión se pueden ver los vuelos y el tablero.

### Con repositorios falsos (sin base de datos)

```bash
dotnet run --launch-profile datos-falsos
# o también:
dotnet run -- --OrigenDeDatos=Falso
```

Arriba aparece un aviso amarillo: todo está en memoria (`Repositorios/Falsos/`). Mirá `Program.cs`: lo único que cambia
es el bloque de registros del contenedor. Controladores, servicios y vistas son exactamente los mismos.

## Mapa del código

| Carpeta / archivo | Qué hay | Desafío |
|---|---|---|
| `aerotaller.sql` | tablas, claves foráneas, índice único parcial, datos con fechas relativas a hoy | 1 |
| `Program.cs` | `CrearBaseSiNoExiste`, contenedor, Serilog, pipeline | 1 · 7 · 8 · 9 |
| `Models/` | dominio: `Vuelo`, `Pasajero`, `Reserva`, `Equipaje`, `Usuario` | 2 |
| `Repositorios/` | ADO.NET con parámetros; `Interfaces.cs`; `ConexionDb` | 2 · 7 |
| `Repositorios/Falsos/` | las mismas interfaces en memoria | 7 |
| `Controllers/` | controladores flacos: reciben, llaman al servicio, eligen vista | 3 · 6 |
| `Views/` | layout, tag helpers, mapa de asientos, formularios | 3 |
| `ViewModels/` | un ViewModel por pantalla | 4 |
| `Validaciones/FechaFuturaAttribute.cs` | atributo propio | 5 |
| `Servicios/` | reglas del negocio + `AutenticacionService` | 6 · 9 |
| `Excepciones/ReglaDeNegocioException.cs` | excepción propia con el campo del formulario | 6 · 8 |
| `Filtros/` | `[RequiereLogin]`, `[RequiereRol]` | 9 |
| `logs/` | se crea al correr: un archivo por día | 8 |

## Cosas para probar con el depurador

- **Choque de reservas**: poné un breakpoint en `reservas.Insertar(reserva)` de `ReservaService.Reservar` y otro en el
  `catch (SqliteException ex) when …`. Reservá un asiento libre; cuando frene, desde DB Browser insertá (y escribí los cambios)
  una reserva confirmada para ese mismo vuelo y asiento —"el otro mostrador"—. Seguí con F5: el índice único rechaza el
  INSERT y caés en el `catch`. (El verificador del desafío 8 prueba lo mismo con 8 reservas simultáneas.)
- **La página de error**: corré con la base en solo lectura y probá reservar:
  `dotnet run -- "--ConnectionStrings:AeroTaller=Data Source=aerotaller.db;Mode=ReadOnly"`.
  Buscá el código que muestra la página en `logs/`.
- **El filtro de login**: breakpoint en `RequiereLoginAttribute.OnActionExecuting` y entrá a `/Reservas` sin sesión.
