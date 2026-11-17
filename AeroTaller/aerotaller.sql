-- ============================================================
-- aerotaller.sql — base del TP AeroTaller
-- Aeropuertos, vuelos, pasajeros, reservas, usuarios y equipaje.
-- Puede ejecutarse completo en DB Browser for SQLite
-- (pestaña "Ejecutar SQL") o con:  sqlite3 aerotaller.db < aerotaller.sql
-- La aplicación lo ejecuta automáticamente la primera vez (CrearBaseSiNoExiste en Program.cs).
-- ============================================================

PRAGMA foreign_keys = ON;

DROP TABLE IF EXISTS Equipaje;
DROP TABLE IF EXISTS Reserva;
DROP TABLE IF EXISTS Vuelo;
DROP TABLE IF EXISTS Pasajero;
DROP TABLE IF EXISTS Aeropuerto;
DROP TABLE IF EXISTS Usuario;

CREATE TABLE Aeropuerto (
    Id     INTEGER PRIMARY KEY AUTOINCREMENT,
    Codigo TEXT NOT NULL UNIQUE,          -- código IATA: 'TUC', 'AEP'…
    Ciudad TEXT NOT NULL,
    Nombre TEXT NOT NULL
);

CREATE TABLE Vuelo (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    Numero          TEXT NOT NULL UNIQUE,  -- 'AT1001'
    OrigenId        INTEGER NOT NULL,
    DestinoId       INTEGER NOT NULL,
    Salida          TEXT NOT NULL,         -- 'AAAA-MM-DD HH:MM' (hora local)
    DuracionMinutos INTEGER NOT NULL,
    Filas           INTEGER NOT NULL,      -- cada fila tiene 6 asientos: A B C | D E F
    Precio          REAL NOT NULL,
    FOREIGN KEY (OrigenId)  REFERENCES Aeropuerto(Id),
    FOREIGN KEY (DestinoId) REFERENCES Aeropuerto(Id),
    CHECK (OrigenId <> DestinoId)
);

CREATE TABLE Pasajero (
    Id     INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL,
    Dni    TEXT NOT NULL UNIQUE,
    Email  TEXT
);

CREATE TABLE Reserva (
    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
    VueloId    INTEGER NOT NULL,
    PasajeroId INTEGER NOT NULL,
    Asiento    TEXT NOT NULL,              -- '12A'
    Fecha      TEXT NOT NULL,              -- fecha en que se realizó la reserva
    Estado     TEXT NOT NULL DEFAULT 'Confirmada' CHECK (Estado IN ('Confirmada', 'Cancelada')),
    FOREIGN KEY (VueloId)    REFERENCES Vuelo(Id),
    FOREIGN KEY (PasajeroId) REFERENCES Pasajero(Id)
);

-- Defensa en profundidad: aunque el servicio ya lo controla, la BASE no admite
-- dos reservas confirmadas en el mismo asiento del mismo vuelo (índice único parcial).
-- Si dos personas reservan a la vez, una de las dos operaciones produce una SqliteException.
CREATE UNIQUE INDEX UX_Reserva_AsientoConfirmado
    ON Reserva (VueloId, Asiento) WHERE Estado = 'Confirmada';

-- Usuarios del sistema (no confundir con Pasajero: los pasajeros no inician sesión).
-- Atención: clave en texto plano SOLO para el TP (igual que en el tema 15). Ver "Para investigar" del desafío 9.
CREATE TABLE Usuario (
    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
    NombreUsuario TEXT NOT NULL UNIQUE,
    Nombre        TEXT NOT NULL,
    Clave         TEXT NOT NULL,
    Rol           TEXT NOT NULL CHECK (Rol IN ('Admin', 'Mostrador'))
);

-- Simulacro de parcial: equipaje despachado por reserva
CREATE TABLE Equipaje (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
    ReservaId INTEGER NOT NULL,
    Tipo      TEXT NOT NULL CHECK (Tipo IN ('Mano', 'Bodega')),
    PesoKg    REAL NOT NULL CHECK (PesoKg > 0),
    FOREIGN KEY (ReservaId) REFERENCES Reserva(Id)
);

-- ── Datos de ejemplo ──────────────────────────────────────────

INSERT INTO Aeropuerto (Codigo, Ciudad, Nombre) VALUES
    ('TUC', 'San Miguel de Tucumán', 'Teniente Benjamín Matienzo'),
    ('AEP', 'Buenos Aires',          'Aeroparque Jorge Newbery'),
    ('COR', 'Córdoba',               'Ingeniero Ambrosio Taravella'),
    ('MDZ', 'Mendoza',               'El Plumerillo'),
    ('BRC', 'San Carlos de Bariloche', 'Teniente Luis Candelaria'),
    ('IGR', 'Puerto Iguazú',         'Cataratas del Iguazú'),
    ('SLA', 'Salta',                 'Martín Miguel de Güemes'),
    ('USH', 'Ushuaia',               'Malvinas Argentinas');

-- Las fechas se calculan a partir de HOY, de modo que siempre haya vuelos futuros para reservar.
-- strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '+3 days', 'start of day', '+7 hours', '+30 minutes')
--   → dentro de 3 días a las 07:30, en el formato 'AAAA-MM-DD HH:MM'
INSERT INTO Vuelo (Numero, OrigenId, DestinoId, Salida, DuracionMinutos, Filas, Precio) VALUES
    ('AT0999', 1, 2, strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '-1 days', 'start of day', '+7 hours', '+30 minutes'), 125, 20, 98000),
    ('AT1001', 1, 2, strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '+12 days', 'start of day', '+7 hours', '+30 minutes'), 125, 20, 98000),
    ('AT1002', 2, 1, strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '+12 days', 'start of day', '+18 hours'),                125, 20, 98000),
    ('AT2010', 2, 5, strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '+15 days', 'start of day', '+10 hours', '+15 minutes'), 140, 25, 154000),
    ('AT2020', 2, 8, strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '+17 days', 'start of day', '+6 hours'),                 215, 25, 231000),
    ('AT3005', 3, 4, strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '+14 days', 'start of day', '+21 hours', '+40 minutes'), 75, 15, 72000),
    ('AT3110', 1, 6, strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '+19 days', 'start of day', '+23 hours', '+30 minutes'), 110, 12, 119000),
    ('AT4001', 7, 3, strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '+13 days', 'start of day', '+9 hours'),                 80, 15, 69000),
    ('AT0500', 1, 7, strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '+11 days', 'start of day', '+12 hours'),                40, 1, 45000);
-- AT0999 ya salió (no se puede reservar) · AT0500 es una avioneta de 6 asientos casi llena · AT3110 llega al día siguiente

INSERT INTO Pasajero (Nombre, Dni, Email) VALUES
    ('Ana Gómez',      '30111222', 'ana.gomez@correo.com'),
    ('Bruno Díaz',     '28999000', NULL),
    ('Carla Ruiz',     '35444555', 'carla.ruiz@correo.com'),
    ('Diego Herrera',  '33777888', NULL),
    ('Elena Paz',      '40123123', 'elena.paz@correo.com'),
    ('Facundo López',  '41222333', 'facu.lopez@correo.com'),
    ('Gabriela Sosa',  '37555666', NULL),
    ('Hernán Molina',  '29888111', 'hmolina@correo.com');

INSERT INTO Reserva (VueloId, PasajeroId, Asiento, Fecha, Estado) VALUES
    (1, 1, '3A',  strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '-10 days'), 'Confirmada'),
    (2, 1, '3A',  strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '-6 days'),  'Confirmada'),
    (2, 2, '3B',  strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '-6 days'),  'Confirmada'),
    (2, 3, '10F', strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '-4 days'),  'Confirmada'),
    (2, 4, '1C',  strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '-3 days'),  'Cancelada'),
    (3, 1, '3A',  strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '-6 days'),  'Confirmada'),
    (4, 5, '7D',  strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '-2 days'),  'Confirmada'),
    (4, 6, '7E',  strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '-2 days'),  'Confirmada'),
    (5, 7, '12A', strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '-1 days'),  'Confirmada'),
    (6, 8, '2F',  strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '-5 days'),  'Confirmada'),
    (9, 2, '1A',  strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '-8 days'),  'Confirmada'),
    (9, 3, '1B',  strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '-8 days'),  'Confirmada'),
    (9, 4, '1C',  strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '-7 days'),  'Confirmada'),
    (9, 5, '1D',  strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '-7 days'),  'Confirmada'),
    (9, 6, '1E',  strftime('%Y-%m-%d %H:%M', 'now', 'localtime', '-2 days'),  'Confirmada');

INSERT INTO Usuario (NombreUsuario, Nombre, Clave, Rol) VALUES
    ('admin',     'Administración', 'aero2026',   'Admin'),
    ('mostrador', 'Marta',          'vuelos2026', 'Mostrador');

INSERT INTO Equipaje (ReservaId, Tipo, PesoKg) VALUES
    (2, 'Bodega', 18.5),
    (2, 'Mano',   7),
    (3, 'Bodega', 22),
    (7, 'Bodega', 15.2);
