# Mini CRM

Prueba técnica de seguimiento comercial con ASP.NET Core 8 y React.

## Organización

- `backend/MiniCrm.Api`: endpoints y configuración HTTP.
- `backend/MiniCrm.Application`: servicios, DTOs y reglas de aplicación.
- `backend/MiniCrm.Domain`: entidades, enumeraciones y contratos.
- `backend/MiniCrm.Data`: persistencia con Entity Framework Core 8 y SQLite.
- `backend/tests`: pruebas de negocio.
- `frontend/src/modules`: clientes, gestiones, dashboard y componentes compartidos.

## Requisitos

- SDK de .NET 8.
- Node.js 22.13 o superior de la rama 22, o Node.js 24 y npm.

## Backend

```bash
cd backend
dotnet tool restore
dotnet restore
dotnet build
cd MiniCrm.Api
dotnet ef database update --project ../MiniCrm.Data --startup-project .
dotnet run
```

La API se ejecuta en `http://localhost:5080`.

La conexión está en `MiniCrm.Api/appsettings.json`, en `ConnectionStrings:MiniCrm`.
SQLite crea el archivo local `MiniCrm.Api/mini-crm.db`, que no se versiona.
Ejecutar los comandos de base de datos desde `MiniCrm.Api` mantiene esa ubicación.
Las migraciones se guardan en `MiniCrm.Data/Migrations`.

## Modelo de datos

- Un cliente tiene varias gestiones. El asesor se guarda como texto.
- Los identificadores son enteros generados por SQLite.
- El CUIT se guarda como texto y tiene un índice único.
- `ProximoContacto` usa `DateOnly`, porque representa una fecha sin hora.
- Las fechas de creación, actualización y gestión usan `DateTime` en UTC.

## API de clientes

| Método | Ruta | Operación |
| --- | --- | --- |
| GET | `/api/clientes` | Listar, buscar, filtrar y ordenar. |
| GET | `/api/clientes/{id}` | Consultar un cliente. |
| POST | `/api/clientes` | Crear y devolver `201` con su ubicación. |
| PUT | `/api/clientes/{id}` | Editar y devolver los datos actualizados. |

El listado acepta `busqueda` (nombre, CUIT o teléfono), `estado` y `orden`
(`asc` o `desc` por próximo contacto; por defecto `asc`). Los clientes sin
fecha quedan al final. Ejemplo: `/api/clientes?estado=3&orden=desc`.

Estados: `1` Prospecto, `2` Contactado, `3` Interesado, `4` No interesado,
`5` Cliente. Si no se informa el estado al crear, se utiliza Prospecto.

El nombre y el CUIT son obligatorios. Se valida el estado y, si se informa,
el formato del correo. El CUIT se guarda sin guiones ni espacios para evitar
duplicados por diferencias de formato; no se verifica su dígito de control.
La edición permite conservar el propio CUIT, pero no usar el de otro cliente.
Teléfono, correo y asesor vacíos se guardan como `null`.

El servicio devuelve `seguimientoVencido` si el próximo contacto es anterior
al día actual del servidor. Hoy y las fechas futuras no están vencidas.
La edición de los datos del cliente conserva su historial y próximo contacto;
la actualización de esa fecha corresponderá al registro de una gestión.

Los errores usan `ProblemDetails`: `400` para datos inválidos, `404` para
cliente inexistente y `409` para CUIT duplicado. Los errores de validación
incluyen `errors` con mensajes por campo.

Para probar la API, abrir `backend/MiniCrm.Api/MiniCrm.Api.http` en Visual Studio
con la API ejecutándose. Crear un cliente y ajustar `clienteId` al ID recibido.

## API de gestiones

| Método | Ruta | Operación |
| --- | --- | --- |
| GET | `/api/clientes/{id}/gestiones` | Consultar el historial completo, del más reciente al más antiguo. |
| POST | `/api/clientes/{id}/gestiones` | Registrar una gestión y devolver `201` con sus datos. |

El tipo de contacto, comentario y estado resultante son obligatorios. Tipos:
`1` Llamada, `2` WhatsApp, `3` Correo, `4` Reunión y `5` Otro. Los estados
usan los mismos valores que los clientes. La fecha y hora se generan en el
backend en UTC. El próximo contacto es opcional y utiliza `AAAA-MM-DD`.

Al registrar se actualizan el estado y la fecha de actualización del cliente.
Si se informa un próximo contacto, reemplaza la fecha anterior; si se omite
o se envía `null`, el cliente conserva la fecha que tenía. Las gestiones
anteriores se mantienen. El registro de la gestión y la actualización del
cliente se guardan juntos, en una sola llamada a `SaveChangesAsync`.

El historial contiene solo las gestiones del cliente solicitado. Si dos
gestiones tienen la misma fecha y hora, se ordenan por ID descendente.
Un cliente existente sin gestiones devuelve `[]`; uno inexistente devuelve
`404`. Los datos inválidos devuelven `400` con mensajes por campo.

Los ejemplos para registrar y consultar gestiones están en el mismo archivo
`MiniCrm.Api.http`. Este bloque utiliza las tablas y la migración existentes.

## API de resumen

`GET /api/dashboard/resumen` devuelve `200` con cuatro contadores:

- `totalClientes`: todos los clientes registrados.
- `prospectos`: clientes cuyo estado actual es Prospecto.
- `interesados`: clientes cuyo estado actual es Interesado.
- `seguimientosVencidos`: clientes con próximo contacto anterior a hoy.

Hoy y las fechas futuras no están vencidas; los clientes sin fecha tampoco.
Se usa el día actual del servidor, igual que en el listado de clientes.
Los contadores se calculan sobre los clientes, aunque tengan varias gestiones,
y reflejan el estado actual. Si no hay clientes, todos los valores son `0`.
El ejemplo de consulta está al final de `MiniCrm.Api.http`.

## Pruebas

Desde `backend`:

```bash
dotnet test
```

Las pruebas usan xUnit y SQLite en memoria con la migración real. Comprueban
CUIT único, validaciones, edición sin perder historial, búsquedas, filtros,
orden y seguimientos vencidos. También comprueban el registro e historial de
gestiones y que, si falla el guardado, no quede el cliente actualizado sin su
gestión. Las pruebas del resumen comprueban los contadores, los límites de
fecha y los cambios de estado producidos por gestiones. No modifican la base
de datos de la aplicación.

## Frontend

```bash
cd frontend
npm ci
```

Copiar `.env.example` como `.env` y ejecutar:

```bash
npm run dev
```

La aplicación se ejecuta en `http://localhost:5173`.
`VITE_API_URL` configura la URL base de la API.

```bash
npm run lint
npm run build
```

## Estado

Completados: estructura, modelo y migración inicial; alta, edición, listado
y detalle de clientes; búsqueda, filtro por estado, orden por próximo contacto,
validaciones, errores controlados y pruebas de negocio; registro e historial
de gestiones con actualización del cliente; resumen general de los cuatro
indicadores.

Pendientes: pantallas y seed.

## Uso de IA

Utilicé ChatGPT (Codex) como herramienta de apoyo para:

- Redactar y actualizar este README.
- Definir la organización de carpetas y capas a partir de proyectos anteriores.
- Asistir en la implementación del modelo de datos y de las API de clientes,
  gestiones y resumen general.
- Guiarme en la generación y aplicación de la migración inicial y en la
  creación de la base de datos SQLite, siguiendo mis indicaciones.
- Generar las pruebas automatizadas de negocio.
- Generar el archivo MiniCrm.Api.http para probar la API.

Definí el alcance y las tecnologías, y revisé las decisiones de organización
y funcionamiento durante el desarrollo. Las pruebas automatizadas y el archivo
.http fueron generados por la herramienta; no los escribí manualmente.
