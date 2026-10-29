# Support Desk

A customer support dashboard for managing tickets, built with ASP.NET Core Web API and Vue 3.

## Features

- Browse, search, filter, and sort support tickets.
- Create, edit, and delete tickets.
- View ticket details, customer information, comments, and status history.
- Add comments and track status changes.
- Shared PostgreSQL persistence with 25 sample tickets for local development.

## Stack and architecture

- ASP.NET Core Web API with C#.
- Vue 3 and TypeScript SPA with Vue Router, Pinia, and Tailwind CSS.
- Persistence: PostgreSQL with EF Core and versioned migrations; migrations are applied explicitly before deployment and seed 25 demo tickets.
- Backend layers: controllers, application services, and a domain-specific `ITicketRepository` backed by EF Core.
- Frontend boundaries: views coordinate route-level state and workflows; `AppShell` owns shared navigation, while focused components render filters, ticket rows, forms, comments, status history, and dialogs. Pinia owns shared ticket state and the API module owns HTTP communication.

See [`ARCHITECTURE_DECISIONS.md`](./ARCHITECTURE_DECISIONS.md) for the reasoning behind the frontend and backend design, trade-offs, and potential evolution paths.

### Design decisions and trade-offs

- `ITicketRepository` keeps application services independent of EF Core and gives the ticket domain a focused persistence contract. A generic repository or separate Unit of Work would add indirection without a concrete need in this small application.
- PostgreSQL gives local development and all deployed API instances a shared, durable data store. EF Core migrations keep schema changes explicit; a managed database and credentials add setup and operational responsibilities.
- The take-home uses a small seeded ticket set, so list filtering and ordering run in the service without pagination. A larger dataset should move those operations into database queries and add pagination.
- The status timeline records status transitions only; comments remain separate. Authentication is omitted and activity authors use a fixed mock identity, as permitted by the assignment.

## Prerequisites

- .NET 10 SDK
- Node.js and npm
- Docker Desktop with Docker Compose

## Run locally

Start PostgreSQL from the backend directory:

```powershell
cd support-desk-backend
Copy-Item .env.example .env
docker compose up -d

dotnet user-secrets init --project .\SupportDesk.Api.csproj
dotnet user-secrets set "ConnectionStrings:SupportDesk" "Host=localhost;Port=5432;Database=supportdesk;Username=supportdesk;Password=<local-password-from-.env>" --project .\SupportDesk.Api.csproj
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update --project .\SupportDesk.Api.csproj
```

Use the `POSTGRES_PASSWORD` value from your local `.env` file for the connection string. The `.env` file is local-only and ignored by Git. User Secrets stores the API connection string outside the repository. The migrations create the schema and insert 25 fictional demo tickets, including sample comments and status history. Migrations are not applied automatically when the API starts.

Start the API in the same terminal:

```powershell
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

To stop the local database while retaining its data, run `docker compose down` from `support-desk-backend`.

### Deploy the API to Vercel

The API can run on Vercel's beta Container Runtime using Docker. Create a separate Vercel project for this repository and set its **Root Directory** to `support-desk-backend`; Vercel detects the `Dockerfile` there, so a `vercel.json` build configuration is not required for this standalone API.

Set these project environment variables in Vercel:

```text
PORT=8080
ConnectionStrings__SupportDesk=<private Neon PostgreSQL connection string>
```

Store `ConnectionStrings__SupportDesk` as a private **Secret** in the backend Vercel project. Use the pooled connection string supplied by Neon for the Vercel deployment. Do not put database credentials in frontend variables, source control, or chat.

Before the first API deployment, set `ConnectionStrings__SupportDesk` securely in the environment used for the migration command and run `dotnet ef database update --project .\SupportDesk.Api.csproj` from `support-desk-backend`. Repeat this step for future schema migrations before deploying code that depends on them. The application does not run schema changes on startup, avoiding concurrent migration attempts when Vercel starts multiple instances.

Set `VITE_API_BASE_URL` in the frontend project's build environment to `https://<your-api-domain>/api`. CORS currently allows requests from any origin for this public take-home demo, so no CORS environment variable is needed. Do not use this policy for an API with authentication or private data.

Neon provides shared persistence across API instances. The 25 demo tickets are inserted once by the versioned seed migration; they are not recreated or reset on each startup.

The earlier SQLite deployment's data is not copied automatically: its instances had separate local files, so they did not provide one consistent dataset to migrate. A new PostgreSQL database receives the 25 versioned demo tickets instead.

Build and run the container locally from the backend directory:

```powershell
cd support-desk-backend
docker build -t supportdesk-api .
docker run --rm -e PORT=8080 -p 8080:8080 supportdesk-api
```

The Vercel Container Runtime is currently in beta and has platform limits, including a 4.5 MB request/response body limit. The current Vercel deployment must be updated after these changes are committed and pushed; this documentation does not indicate that Neon has been provisioned or the production database has been migrated.

### Configure the API URL

The frontend reads `VITE_API_BASE_URL` when Vite starts or builds the app. By default, it uses `/api` and the local Vite proxy. To point the browser directly at another API, copy `.env.example` to `.env.local` inside `support-desk-frontend` and set the API base URL, including the `/api` path:

```env
VITE_API_BASE_URL=https://api.example.com/api
```

Restart the Vite dev server after changing `.env.local`. Production builds must receive the appropriate value at build time. Because `VITE_*` values are included in browser assets, do not put secrets in them. The API currently allows browser requests from any origin; restrict this policy to known frontend origins before adding authentication or handling private data.

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

Run backend unit and HTTP integration tests (HTTP tests start an isolated PostgreSQL container with Testcontainers; Docker must be running):

```powershell
dotnet test .\SupportDesk.Api.Tests\SupportDesk.Api.Tests.csproj
```

Backend tests cover ticket filtering and ordering, create/update/comment/delete rules, status history, request validation, and essential HTTP status codes. The frontend is type-checked during its build and its main workflows have been manually verified.
