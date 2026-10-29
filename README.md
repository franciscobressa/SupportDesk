# Support Desk

A customer support dashboard for managing tickets, built with ASP.NET Core Web API and Vue 3.

## Features

- Browse, search, filter, and sort support tickets.
- Create, edit, and delete tickets.
- View ticket details, customer information, comments, and status history.
- Add comments and track status changes.
- Persistent SQLite storage with sample tickets for local development.

## Stack and architecture

- ASP.NET Core Web API with C#.
- Vue 3 and TypeScript SPA with Vue Router, Pinia, and Tailwind CSS.
- Persistence: SQLite with EF Core; migrations are applied at startup and sample tickets are seeded into an empty database.
- Backend layers: controllers, application services, and a domain-specific `ITicketRepository` backed by EF Core.
- Frontend boundaries: views coordinate route-level state and workflows; `AppShell` owns shared navigation, while focused components render filters, ticket rows, forms, comments, status history, and dialogs. Pinia owns shared ticket state and the API module owns HTTP communication.

See [`ARCHITECTURE_DECISIONS.md`](./ARCHITECTURE_DECISIONS.md) for the reasoning behind the frontend and backend design, trade-offs, and potential evolution paths.

### Design decisions and trade-offs

- `ITicketRepository` keeps application services independent of EF Core and gives the ticket domain a focused persistence contract. A generic repository or separate Unit of Work would add indirection without a concrete need in this small application.
- SQLite avoids an external database service while preserving data between runs. EF Core migrations keep the schema explicit and reproducible.
- The take-home uses a small seeded ticket set, so list filtering and ordering run in the service without pagination. A larger dataset should move those operations into database queries and add pagination.
- The status timeline records status transitions only; comments remain separate. Authentication is omitted and activity authors use a fixed mock identity, as permitted by the assignment.

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

The SQLite database is created at `support-desk-backend/App_Data/supportdesk.db`, relative to the API project regardless of the shell's working directory. Startup applies EF Core migrations and inserts sample tickets only when the database has no tickets.

### Deploy the API to Vercel

The API can run on Vercel's beta Container Runtime using Docker. Create a separate Vercel project for this repository and set its **Root Directory** to `support-desk-backend`; Vercel detects the `Dockerfile` there, so a `vercel.json` build configuration is not required for this standalone API.

Set these project environment variables in Vercel:

```text
PORT=8080
Cors__AllowedOrigins__0=https://<your-frontend-domain>
```

Set `VITE_API_BASE_URL` in the frontend project's build environment to `https://<your-api-domain>/api`. The allowed CORS origin must exactly match the browser-facing frontend origin.

**Demo limitation:** this deployment intentionally keeps SQLite to avoid changing the approved take-home database design. Vercel container filesystems are not persistent, and scaled instances do not share their local SQLite files. Ticket changes may disappear after an instance is replaced and different instances may show different seeded data. Do not use this deployment for durable or real customer data; use managed PostgreSQL before production.

Build and run the container locally from the backend directory:

```powershell
cd support-desk-backend
docker build -t supportdesk-api .
docker run --rm -e PORT=8080 -p 8080:8080 supportdesk-api
```

The Vercel Container Runtime is currently in beta and has platform limits, including a 4.5 MB request/response body limit. These deployment instructions prepare the project but do not indicate that it has been deployed.

### Configure the API URL

The frontend reads `VITE_API_BASE_URL` when Vite starts or builds the app. By default, it uses `/api` and the local Vite proxy. To point the browser directly at another API, copy `.env.example` to `.env.local` inside `support-desk-frontend` and set the API base URL, including the `/api` path:

```env
VITE_API_BASE_URL=https://api.example.com/api
```

Restart the Vite dev server after changing `.env.local`. Production builds must receive the appropriate value at build time. Because `VITE_*` values are included in browser assets, do not put secrets in them. A direct URL on another origin also requires the API to allow the frontend origin through CORS. The local API currently allows `http://localhost:5173`.

## API

- `GET /api/tickets`: filter by `status`, `priority`, and `search`; sort by `createdAt`, `priority`, or `status` using `sortDirection=asc|desc`.
- `GET /api/tickets/{id}`: ticket details, comments, and status history.
- `POST /api/tickets`: create a ticket.
- `PUT /api/tickets/{id}`: update a ticket; status changes are recorded in its history.
- `DELETE /api/tickets/{id}`: delete a ticket and its related comments/history.
- `POST /api/tickets/{id}/comments`: add a comment with a server-assigned mock author and timestamp.

Errors use ASP.NET Core validation problem responses. The API currently uses a fixed mock author and has no authentication.

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

Backend tests cover ticket filtering and ordering, create/update/comment/delete rules, status history, request validation, and essential HTTP status codes. The frontend is type-checked during its build and its main workflows have been manually verified.
