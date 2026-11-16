# AeroTaller — solución de referencia (uso interno de la cátedra)

🔒 Repo **privado**. La plantilla que reciben los alumnos está en
[taller-de-lenguajes-2/aerotaller-tp](https://github.com/taller-de-lenguajes-2/aerotaller-tp).

| Carpeta | Qué es |
|---|---|
| [`AeroTaller/`](./AeroTaller) | la aplicación terminada: desafíos 0–10 (incluye el simulacro de equipaje). Ver su [README](./AeroTaller/README.md) |
| [`docs/PROPUESTA.md`](./docs/PROPUESTA.md) | diseño del TP: decisiones, formato de cada desafío, desafíos, insignias |
| [`docs/MAPA_DE_TEMAS.md`](./docs/MAPA_DE_TEMAS.md) | relación con las diapositivas (temas 00–17) y lo que el TP agrega |

## Correr la solución

```bash
cd AeroTaller
dotnet run                               # http://localhost:5210 · admin/aero2026 · mostrador/vuelos2026
dotnet run --launch-profile datos-falsos # mismos controladores y servicios, repositorios en memoria
```

## Validarla con el verificador de la plantilla

Con los dos repos clonados uno al lado del otro:

```bash
cd aerotaller-tp
dotnet run --project verificador -- todos --sin-git --proyecto ../aerotaller-tp-solucion/AeroTaller
```

Resultado esperado: **11/11 desafíos en verde**.

## Corregir a un alumno

La corrección automática de Classroom corre el verificador que está **en el repo del alumno**. Para descartar que lo haya
modificado, correr el verificador **oficial** (el de la plantilla) contra su proyecto:

```bash
git clone <repo-del-alumno> alumno
cd aerotaller-tp
dotnet run --project verificador -- todos --proyecto ../alumno/AeroTaller
```

Y mirar el historial de commits del alumno (`git log --stat`): un commit por desafío es lo esperable.

## Configurar la tarea en GitHub Classroom

1. Nueva tarea → *Template repository*: `taller-de-lenguajes-2/aerotaller-tp`.
2. La corrección automática ya viene en `.github/workflows/classroom.yml` (11 tests, 100 puntos). No hace falta cargar tests a mano.
3. Visibilidad de los repos de alumnos: privada.
