# Mini CRM

Prueba técnica de seguimiento comercial con ASP.NET Core 8 y React.
Esta primera rama contiene la estructura y la configuración inicial.

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
dotnet restore
dotnet build
dotnet run --project MiniCrm.Api
```

La API se ejecuta en `http://localhost:5080`. Todavía no hay endpoints de negocio.
El contexto, las migraciones y los datos iniciales se incorporarán con el modelo.

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

Estructura inicial preparada. Pendientes: clientes, gestiones, historial, resumen,
validaciones, persistencia, seed y pruebas de negocio.

## Uso de IA

ChatGPT/Codex asistió en la estructura y configuración inicial siguiendo las
decisiones de alcance y arquitectura del candidato. Los repositorios de referencia
se usaron para consultar su organización, sin trasladar funcionalidades.
