# Support Desk

A customer support dashboard for managing tickets, built with ASP.NET Core Web API and Vue 3.

Live Demo: [https://support-desk-frontend-rouge.vercel.app/](https://support-desk-frontend-rouge.vercel.app/)

## Screenshots

<img width="1892" height="900" alt="image" src="https://github.com/user-attachments/assets/046c15b5-5a16-4088-9cbd-b1ea2ea24999" />
<img width="1917" height="897" alt="image" src="https://github.com/user-attachments/assets/af0d087c-793e-4e8a-9738-3a16563d69a6" />
<img width="1887" height="892" alt="image" src="https://github.com/user-attachments/assets/28681ee6-8227-45d8-a2d6-bac6f8830215" />

## Features

- Browse, search, filter, and sort support tickets; the list criteria live in the URL, so they survive a reload and are restored when returning from a ticket.
- Create, edit, and delete tickets, with view, edit, and delete actions directly in the list.
- View ticket details, customer information, comments, and status history.
- Add comments and track status changes.
- Switch between light and dark themes; the selected theme is saved in the browser.
- Persistent SQLite storage with sample tickets for local development.

## Stack and architecture

- ASP.NET Core Web API with C#.
- Vue 3 and TypeScript SPA with Vue Router, Pinia, and Tailwind CSS.
- Persistence: SQLite with EF Core; migrations are applied at startup and 25 demo tickets are seeded only when the database is empty. Timestamps are stored as UTC ticks so SQLite can sort them natively.
- Backend layers: controllers, application services, and a domain-specific `ITicketRepository` backed by EF Core.
- Frontend boundaries: views coordinate route-level state and workflows; `AppShell` owns shared navigation, while focused components render filters, ticket rows, forms, comments, status history, and dialogs. Pinia owns shared ticket state and the API module owns HTTP communication.

### Design decisions and trade-offs

- `ITicketRepository` keeps application services independent of EF Core and gives the ticket domain a focused persistence contract. A generic repository or separate Unit of Work would add indirection without a concrete need in this small application.
- SQLite avoids an external database service while preserving data between runs. EF Core migrations keep the schema explicit and reproducible.
- Ticket search, filtering, sorting, and server-side pagination run in SQLite. The API returns 10 tickets per page by default and caps requested page sizes at 50.
- The status timeline records status transitions only; comments remain separate. Authentication is omitted and activity authors use a fixed mock identity, as permitted by the assignment.

## Extras beyond the brief

- Optimistic concurrency: two managers editing the same ticket cannot silently overwrite each other; the second save gets a conflict message and can load the latest version.
- Server-side pagination returns only the requested page while preserving filtered result counts; this avoids transferring the full ticket list as it grows.
- Dashboard filters, sorting, and page are kept in the URL: a manager can open a ticket and come back to the same filtered view, reload, or share the link.
- Dashboard summary cards show global ticket counts by status and high priority, and act as quick filters for queue triage.
- Status-change history makes ticket progress auditable in the detail view without mixing status events with customer comments.
- The public demo is deployed as separate frontend and API projects, with direct Vue Router links supported by the frontend deployment rewrite.

The demo API uses SQLite to stay within the assignment's lightweight-persistence scope. Vercel container storage is not durable or shared between instances, so demo changes can disappear or differ between instances; use a managed database before relying on persistent production data.

## What I would improve with more time

- Add automated frontend component and end-to-end tests for filtering, pagination, editing, and comments.
- Add authentication and authorization before handling real customer data.
- Add operational persistence and deployment safeguards, including a managed database, controlled migrations, and backups.

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

Vite serves the frontend at `http://localhost:5173` and proxies `/api` requests to `http://localhost:5131`.

The SQLite database is created at `support-desk-backend/App_Data/supportdesk.db`, relative to the API project regardless of the shell's working directory. Startup applies EF Core migrations and seeds 25 demo tickets only when the database has no tickets, so deleted tickets are not recreated on restart.

## API

- `GET /api/tickets`: filter by `status`, `priority`, and `search`; sort by `createdAt`, `priority`, or `status` using `sortDirection=asc|desc`; paginate with `page` (default `1`) and `pageSize` (default `10`, maximum `50`). The response contains `items`, `page`, `pageSize`, `totalCount`, `totalPages`, and global `summary` counts by status and high priority.
- `GET /api/tickets/{id}`: ticket details, comments, and status history.
- `POST /api/tickets`: create a ticket.
- `PUT /api/tickets/{id}`: update a ticket; status changes are recorded in its history. The body includes `expectedUpdatedAt` (the `updatedAt` value the client loaded); if the ticket changed in the meantime the API returns `409 Conflict` instead of overwriting the newer version.
- `DELETE /api/tickets/{id}`: delete a ticket and its related comments/history.
- `POST /api/tickets/{id}/comments`: add a comment with a server-assigned mock author and timestamp.

Errors use ASP.NET Core `ProblemDetails` responses (`ValidationProblemDetails` for `400`). The API currently uses a fixed mock author and has no authentication.

## Build and test

Build the API:

```powershell
cd support-desk-backend
dotnet build
```

Build and type-check the frontend:

```powershell
cd support-desk-frontend
npm run build
```

Run backend unit and HTTP integration tests (the tests use an isolated in-memory SQLite database):

```powershell
dotnet test .\SupportDesk.Api.Tests\SupportDesk.Api.Tests.csproj
```

Backend tests cover domain rules, layer dependency rules (architecture tests with NetArchTest), ticket filtering, ordering, and pagination, create/update/comment/delete rules, status history, request validation, and essential HTTP status codes. The frontend is type-checked during its build and its main workflows have been manually verified.
