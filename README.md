# Mini CRM

Prueba técnica de seguimiento comercial con ASP.NET Core 8 y React.

El backend y los datos de ejemplo están implementados. El frontend permite
consultar, crear y editar clientes, ver el resumen general y acceder al detalle
de cada cliente para registrar gestiones y consultar su historial completo.

## Organización

- `backend/MiniCrm.Api`: endpoints y configuración HTTP.
- `backend/MiniCrm.Application`: servicios, DTOs y reglas de aplicación.
- `backend/MiniCrm.Domain`: entidades, enumeraciones y contratos.
- `backend/MiniCrm.Data`: persistencia con Entity Framework Core 8 y SQLite.
- `backend/tests`: pruebas de negocio.
- `frontend/src/modules`: clientes, gestiones, dashboard y componentes compartidos.

## Requisitos

- Git para clonar el repositorio.
- SDK de .NET 8.
- Node.js 22.13 o superior de la rama 22, o Node.js 24 y npm.

## Obtener el proyecto

```bash
git clone https://github.com/facu030/mini-crm.git
cd mini-crm
```

Los comandos del backend parten de esta raíz. Para el frontend, abrir una
segunda terminal en la misma raíz y seguir su sección. Se necesita conexión
a Internet para descargar las dependencias de NuGet y npm la primera vez.

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

Mantener una sola instancia de la API ejecutándose. Si se inicia desde la
terminal, Visual Studio puede usarse para enviar las solicitudes de
`MiniCrm.Api.http`. Para detener la API de la terminal, presionar `Ctrl+C`.

La conexión está en `MiniCrm.Api/appsettings.json`, en `ConnectionStrings:MiniCrm`.
SQLite crea el archivo local `MiniCrm.Api/mini-crm.db`, que no se versiona.
Ejecutar los comandos de base de datos desde `MiniCrm.Api` mantiene esa ubicación.
Las migraciones se guardan en `MiniCrm.Data/Migrations`.

## Datos de ejemplo

Después de aplicar la migración, al iniciar la API en modo `Development`
se cargan automáticamente cinco clientes ficticios y cinco gestiones, solo
si no hay ningún cliente. El perfil de ejecución incluido usa ese modo.
Si la base ya contiene clientes, se conserva completa, sin agregar ejemplos
ni restablecer cambios. Reiniciar la API tampoco duplica los datos.

| Cliente | Estado inicial | Próximo contacto | Gestiones |
| --- | --- | --- | --- |
| Almacén Norte | Prospecto | Sin fecha | 0 |
| Ferretería Centro | Contactado | Ayer | 1 |
| Librería Sur | Interesado | Hace dos días | 2 |
| Tienda Oeste | No interesado | Sin fecha | 1 |
| Panadería Este | Cliente | Dentro de tres días | 1 |

Las fechas se calculan al cargar los ejemplos. El resumen inicial devuelve
`totalClientes: 5`, `prospectos: 1`, `interesados: 1` y `seguimientosVencidos: 2`.
La Librería conserva el próximo contacto del correo anterior, porque su
última gestión por WhatsApp no informa una nueva fecha.

Para probar los ejemplos sin tocar una base existente, detener la API y,
desde `backend/MiniCrm.Api`, usar otra base local. En Git Bash:

```bash
ConnectionStrings__MiniCrm="Data Source=mini-crm-demo.db" dotnet ef database update --project ../MiniCrm.Data --startup-project .
ConnectionStrings__MiniCrm="Data Source=mini-crm-demo.db" dotnet run
```

Estos comandos usan `mini-crm-demo.db`; al ejecutar normalmente se vuelve
a la conexión de `appsettings.json`. No se versionan las bases de datos.

## Modelo de datos

- Un cliente tiene varias gestiones. El asesor se guarda como texto.
- Los identificadores son enteros generados por SQLite.
- El CUIT se guarda como texto y tiene un índice único.
- `ProximoContacto` usa `DateOnly`, porque representa una fecha sin hora.
- Las fechas de creación, actualización y gestión usan `DateTime` en UTC.

El alta guarda el estado inicial del cliente. El alta y la edición de sus datos
no crean una gestión: el historial comienza al registrar un contacto con su
tipo, comentario y estado resultante desde el detalle.

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

El nombre y el CUIT son obligatorios. El CUIT debe tener 11 dígitos numéricos.
Se valida el estado y, si se informa, el formato del correo. El CUIT se guarda
sin guiones ni espacios para evitar
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
con la API ejecutándose y enviar las solicitudes de cada bloque. El archivo
incluye listado, detalle, alta, edición, gestiones, historial, resumen y ejemplos
de validación y cliente inexistente. Repetir el alta de ejemplo permite probar
el rechazo del CUIT duplicado. Al crear un cliente, ajustar `clienteId` al ID
recibido para que las siguientes solicitudes trabajen sobre ese cliente.

También se puede comprobar el arranque desde una terminal:

```bash
curl http://localhost:5080/api/dashboard/resumen
```

En una base nueva, después del seed, debe devolver los cuatro valores indicados
en la sección de datos de ejemplo.

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
CUIT único y de 11 dígitos numéricos, validaciones, edición sin perder historial, búsquedas, filtros,
orden y seguimientos vencidos. También comprueban el registro e historial de
gestiones y que, si falla el guardado, no quede el cliente actualizado sin su
gestión. Las pruebas del resumen comprueban los contadores, los límites de
fecha y los cambios de estado producidos por gestiones. Las pruebas de datos
iniciales comprueban su coherencia y que no se dupliquen ni alteren los datos
existentes. No modifican la base de datos de la aplicación.

## Frontend

Se utiliza JavaScript, React, Tailwind CSS y Axios. SweetAlert2 se utiliza
para confirmar el guardado y mostrar mensajes de éxito. En una segunda terminal,
desde la raíz del proyecto:

```bash
cd frontend
npm ci
cp .env.example .env
npm run dev
```

La aplicación se ejecuta en `http://localhost:5173`. El comando `cp` funciona
en Git Bash; también se puede copiar `.env.example` y nombrar la copia `.env`.
`VITE_API_URL` configura la URL base de la API; por defecto es
`http://localhost:5080/api`. Reiniciar Vite si se cambia esa variable.

Mantener la API en ejecución en la primera terminal. Si ya está configurada,
para iniciarla desde la raíz del proyecto:

```bash
cd backend/MiniCrm.Api
dotnet run
```

El puerto del frontend es fijo para coincidir con la configuración de CORS
de la API. Si el puerto `5173` está ocupado, detener la instancia anterior.

### Organización por funcionalidades

- `src/modules/clientes/pages`: pantallas y estado del listado y del detalle.
- `src/modules/clientes/components`: filtros, tabla, etiqueta de estado y
  datos del cliente; formulario compartido para alta y edición.
- `src/modules/clientes/services`: solicitudes de clientes con Axios.
- `src/modules/clientes/constants`: estados y filtros iniciales.
- `src/modules/gestiones`: formulario, historial, tipos de contacto y solicitudes
  de gestiones con Axios.
- `src/modules/dashboard`: componente del resumen y su solicitud a la API.
- `src/modules/shared`: instancia de Axios, botones, campos de formulario, mensajes y funciones
  para mostrar fechas y errores.

`App` contiene la estructura visual y muestra la pantalla de clientes.
Los componentes reciben los datos y acciones mediante props. Se utilizan
`useState` y `useEffect` para el estado y la carga de datos.
La navegación entre listado, formulario y detalle se resuelve con estado local
en la pantalla de clientes.

### Pantalla de clientes

La tabla muestra nombre o razón social, CUIT, teléfono, correo, estado,
asesor, próximo contacto y última actualización. Los seguimientos vencidos
se resaltan y muestran el texto `Vencido`, usando el valor calculado por la API.

Para buscar, escribir un nombre, CUIT o teléfono; opcionalmente seleccionar
un estado y el orden del próximo contacto, y presionar `Buscar` o Enter.
`Limpiar` restaura todos los estados y el orden ascendente. `Actualizar`
vuelve a consultar con los filtros aplicados. Las búsquedas, filtros y orden
se resuelven en el backend; los clientes sin próximo contacto quedan al final.

El resumen muestra total de clientes, prospectos, interesados y seguimientos
vencidos de toda la base. Filtrar la tabla no cambia esos totales generales.

La pantalla informa cuando está cargando, cuando no hay clientes o resultados,
y cuando ocurre un error. Permite reintentar una consulta fallida. Si cambia
la consulta, se cancela la anterior para evitar que una respuesta vieja
reemplace los resultados actuales. La tabla permite desplazamiento horizontal
cuando no entra completa en la pantalla.

Las fechas de próximo contacto se muestran como `DD/MM/AAAA`, conservando
el día recibido. Las fechas con hora se muestran en la zona horaria del navegador.

### Alta y edición de clientes

`Nuevo cliente` abre un formulario vacío con estado Prospecto. `Editar`, en
cada fila, abre el mismo formulario con los datos actuales del cliente. Incluye
nombre, CUIT, teléfono, correo, estado y asesor responsable.

Se validan nombre y CUIT obligatorios, CUIT de 11 dígitos numéricos después
de quitar guiones y espacios, y el formato del correo si se informa.
El selector permite los cinco estados definidos. El backend vuelve a validar
los datos y controla que el CUIT no pertenezca a otro cliente. Los mensajes de
validación se muestran junto al campo; los errores de conexión y guardado se
muestran en el formulario, conservando lo que se escribió.
Los errores de campo se mantienen mientras no se corrijan, aunque se edite
otro campo o se vuelva a enviar el formulario. El CUIT duplicado se muestra
solo junto al CUIT y se conserva mientras ese valor no cambie.

SweetAlert2 pide confirmación antes de enviar el alta o la edición. Cancelar
esa confirmación conserva el formulario y no envía la solicitud. Durante el
guardado se deshabilitan los campos y botones para evitar envíos repetidos.
Después de guardar se muestra un mensaje de éxito, se vuelve al listado y
se consultan nuevamente la tabla y el resumen, conservando los filtros aplicados.
El botón `Cancelar` del formulario vuelve al listado sin guardar.

La edición envía solo los datos del cliente. El historial y el próximo contacto
se conservan en el backend, según las reglas ya implementadas.

### Detalle y gestiones

`Ver detalle`, en cada fila, abre los datos del cliente, el formulario de nueva
gestión y su historial completo. Se muestran estado actual, asesor, próximo
contacto, fecha de creación y última actualización. Si el seguimiento está
vencido, se identifica con el texto `Vencido`.

El formulario permite elegir Llamada, WhatsApp, Correo, Reunión u Otro,
escribir un comentario, seleccionar el estado resultante e informar una fecha
opcional de próximo contacto. El comentario es obligatorio y los selectores
usan los valores permitidos por la API. La fecha y hora de la gestión se
generan en el backend. Dejar vacía la próxima fecha conserva la fecha actual
del cliente; no la borra.

SweetAlert2 pide confirmación antes del registro y muestra el mensaje de éxito.
Mientras se procesa, se deshabilitan el formulario y el botón para volver al
listado. Cancelar la confirmación conserva lo escrito. Si falla el registro,
se conservan los datos y se muestran los errores junto a los campos o un
mensaje general cuando corresponda.

Después de registrar se vuelven a consultar el cliente y el historial para
mostrar su estado, próxima fecha y última actualización. El formulario queda
vacío para una nueva gestión, con el estado actual seleccionado. El historial
incluye fecha y hora, tipo, comentario, estado resultante y próximo contacto,
en el orden de más reciente a más antiguo que devuelve la API. Las gestiones
anteriores permanecen visibles. Si aún no hay gestiones, se informa claramente.

La carga del detalle tiene mensajes de carga, error y reintento. `Volver al
listado` actualiza la tabla y los contadores, conservando los filtros aplicados.

Para verificar este recorrido: abrir un cliente, registrar una gestión con
nuevo estado y próxima fecha, comprobar el detalle y la primera fila del
historial, y volver al listado para revisar la tabla y el resumen. Registrar
otra gestión sin próxima fecha permite comprobar que se conserva la anterior
y que ambas gestiones aparecen en el historial.

Para verificar el frontend, desde `frontend`:

```bash
npm run lint
npm run build
```

## Estado

Completados en el backend: estructura, modelo y migración inicial; alta, edición, listado
y detalle de clientes; búsqueda, filtro por estado, orden por próximo contacto,
validaciones, errores controlados y pruebas de negocio; registro e historial
de gestiones con actualización del cliente; resumen general de los cuatro
indicadores; datos iniciales de ejemplo.

Completados en el frontend: pantalla principal integrada con la API, listado
de clientes, búsqueda por nombre/CUIT/teléfono, filtro por estado, orden por
próximo contacto, identificación de vencidos, resumen general y mensajes de
carga, error y ausencia de resultados; alta y edición con un formulario
reutilizable, validaciones, confirmación y mensajes de éxito; detalle del
cliente, registro de gestiones e historial completo, con actualización del
cliente y del resumen.

Funcionalidades obligatorias pendientes: ninguna. Las mejoras opcionales del
enunciado se dejaron fuera del alcance de esta entrega.

## Uso de IA

Utilicé ChatGPT (Codex) como herramienta de apoyo para:

- Redactar y actualizar este README.
- Definir la organización de carpetas y capas a partir de proyectos anteriores.
- Asistir en la implementación del modelo de datos y de las API de clientes,
  gestiones y resumen general.
- Generar la carga inicial de clientes y gestiones ficticios.
- Guiarme en la generación y aplicación de la migración inicial y en la
  creación de la base de datos SQLite, siguiendo mis indicaciones.
- Generar las pruebas automatizadas de negocio.
- Generar el archivo MiniCrm.Api.http para probar la API.
- Generar los bloques de listado, alta, edición y detalle de clientes, registro
  e historial de gestiones del frontend, con componentes por funcionalidad
  y componentes compartidos, siguiendo el alcance y las tecnologías que indiqué.

Definí el alcance y las tecnologías, y revisé las decisiones de organización
y funcionamiento durante el desarrollo. Probé los flujos localmente y pedí
ajustes para conservar los errores por campo y rechazar CUIT con letras.
Las pruebas automatizadas y el archivo .http fueron generados por la
herramienta; no los escribí manualmente.
