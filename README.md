# Support Desk

Take-home assignment: a customer support dashboard built with ASP.NET Core Web API and Vue 3.

**Status:** project foundation; ticket workflows are not implemented yet.

## Stack and architecture

- ASP.NET Core Web API with C#.
- Vue 3 SPA with Vue Router, Pinia, and Tailwind CSS.
- Planned persistence: SQLite with EF Core.
- Backend layers: controllers, application services, and a domain-specific `ITicketRepository` backed by EF Core.

The design and acceptance criteria are documented in [`specs/`](./specs/). The implementation will follow those specs after approval.

## Prerequisites

- .NET 10 SDK
- Node.js and npm

## Run locally

Start the API:

```powershell
cd support-desk-backend
dotnet run --launch-profile http
```

The API listens at `http://localhost:5131`. Its OpenAPI document is available at `http://localhost:5131/openapi/v1.json` in Development.

In another terminal, start the SPA:

```powershell
cd support-desk-frontend
npm install
npm run dev
```

Vite serves the frontend at `http://localhost:5173`. The API allows requests from that local development origin.

## Development

Build the backend:

```powershell
cd support-desk-backend
dotnet build
```

Build the frontend:

```powershell
cd support-desk-frontend
npm run build
```
