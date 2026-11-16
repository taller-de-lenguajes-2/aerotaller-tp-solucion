# TP integrador interactivo "AeroTaller" — diseño

> Corregido contra el material real de la cátedra (temas 00–17). Detalle por tema en [MAPA_DE_TEMAS.md](./MAPA_DE_TEMAS.md).
> Objetivo: entrenamiento para el **segundo parcial** (práctico en máquina) y base del proyecto final.

## Decisiones

| Tema | Decisión | Por qué |
|---|---|---|
| Stack del TP | C# / .NET 10, ASP.NET Core MVC, ADO.NET con `Microsoft.Data.Sqlite`, Serilog | es lo que enseña la cátedra (sin Entity Framework) |
| Arquitectura pedida | MVC + ViewModels + validación + capa de servicios + repositorios con interfaces + DI (`ConexionDb`) + excepciones/logs + login con sesión y filtros | lo que evalúa el parcial |
| Dominio | aerolínea acotada, paralela a la clínica (`Pasajero/Vuelo/Reserva` ↔ `Paciente/Medico/Turno`) | mismas habilidades en un dominio nuevo |
| Repositorios falsos | `OrigenDeDatos = Sqlite / Falso` cambia solo los registros de `Program.cs` | mostrar en vivo para qué sirve la inyección de dependencias |
| Tutorial | app local: backend ASP.NET Web API + front Vue 3 sin build, progreso en SQLite | se corre con `dotnet run`, sin Node ni internet |
| Testing | sin xUnit (no se dicta); un **verificador** de punta a punta | corrige como un humano: navega, envía formularios, mira la base |
| Entrega | repo template para GitHub Classroom + autograding con el mismo verificador | el alumno ve localmente lo mismo que lo corrige |
| Animaciones | sobrias: avión en el plan de vuelo, despegue al completar, revelado de pasos; sin sonido | pedido explícito |

## Formato de cada desafío

🎯 Misión · 📚 Repaso (con link a la diapositiva) · 💻 Paso a paso (se revela de a uno; código completo plegado) ·
🔴 Breakpoints · 🔀 Alternativas · ⚠️ Errores típicos · 🔎 Para investigar · ✅ Checklist · 🛠️ Verificación ·
📝 Commit y push (con estado del repo git) · ❓ Cuestionario de 3 preguntas (al menos una sale del depurador) · 🛫 Despegar.

Cada 3 desafíos, un checkpoint **"¿Cómo vamos?"**: resumen automático (hecho / falta / sin verificar) + bitácora
(qué hice, qué falta, dudas para la consulta). Las dudas se copian con un botón para llevarlas a la consulta.

## Desafíos

| # | Escala | Temas | Qué queda andando |
|---|---|---|---|
| 0 | Check-in | 00 · 10 | repo clonado, app corriendo, primer breakpoint, primer push |
| 1 | La base | 07 · 08 | `aerotaller.sql` + `CrearBaseSiNoExiste`, consultas en DB Browser |
| 2 | Repositorios | 01 · 02 · 08 · 09 | modelos con composición, repositorios ADO.NET parametrizados |
| CP1 | ¿Cómo vamos? | | |
| 3 | Controladores y vistas | 10 · 11 | tablero, vuelos, pasajeros con búsqueda |
| 4 | ViewModels | 12 | mapa de asientos (matriz), formulario de reserva con combos |
| 5 | Validación | 13 | atributos, `ModelState`, `[FechaFutura]`, reglas con la base |
| CP2 | ¿Cómo vamos? | | |
| 6 | Servicios | 04 · 09 · 16 | reglas en `ReservaService`, `ReglaDeNegocioException`, cancelar |
| 7 | Inyección de dependencias | 14 | interfaces, `ConexionDb`, repositorios falsos, tiempos de vida |
| 8 | Excepciones y logs | 16 · 17 | índice único + `catch when`, Serilog, página de error |
| CP3 | ¿Cómo vamos? | | |
| 9 | Autenticación | 15 | login, sesión, `[RequiereLogin]`, `[RequiereRol]` |
| 10 | Simulacro (2 h 30 min) | todos | equipaje de punta a punta, sin guía; se mide por porcentaje de completitud |

## Insignias (primera versión, editable en `insignias.json`)

Una por escala + logros: primera vez / piloto de pruebas (quiz al primer intento), ojo de halcón / cazabugs
(preguntas del depurador), bitácora al día (3 checkpoints), lector de manuales / biblioteca de a bordo (lecturas),
commit a tiempo / push perfecto (repo al día al verificar), certificado por la torre (verificador en verde),
consulta registrada (anotar una duda), perseverancia (aprobar tras 3+ intentos), a horario (simulacro en tiempo),
comandante (todo completo).
