# Mini CRM

Prueba técnica de seguimiento comercial con ASP.NET Core 8 y React.

## Organización

- `backend/MiniCrm.Api`: endpoints y configuración HTTP.
- `backend/MiniCrm.Application`: servicios, DTOs y reglas de aplicación.
- `backend/MiniCrm.Domain`: entidades, enumeraciones y contratos.
- `backend/MiniCrm.Data`: persistencia con Entity Framework Core 8 y SQLite.
- `backend/tests`: espacio para las pruebas de negocio.
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

La API se ejecuta en `http://localhost:5080`. Todavía no hay endpoints de negocio.

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

Preparados: estructura, entidades, enumeraciones, contexto y migración inicial.
Pendientes: endpoints y reglas de clientes y gestiones, historial, resumen,
validaciones de la API, pantallas, seed y prueba automatizada de negocio.

## Uso de IA

ChatGPT asistió en la estructura, el modelo y la configuración de persistencia.
