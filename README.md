# Support Desk

Take-home assignment: a customer support dashboard built with ASP.NET Core Web API and Vue 3.

**Status:** checkpoints 1–4 complete; the ticket API, Vue/TypeScript workflows, and focused backend tests are implemented.

## Stack and architecture

- ASP.NET Core Web API with C#.
- Vue 3 and TypeScript SPA with Vue Router, Pinia, and Tailwind CSS.
- Persistence: SQLite with EF Core; migrations are applied at startup and sample tickets are seeded into an empty database.
- Backend layers: controllers, application services, and a domain-specific `ITicketRepository` backed by EF Core.
- Frontend boundaries: views coordinate route-level state and workflows; `AppShell` owns shared navigation, while focused components render filters, ticket rows, forms, comments, status history, and dialogs. Pinia owns shared ticket state and the API module owns HTTP communication.

The approved design and acceptance criteria are documented in [`specs/`](./specs/). Source code, identifiers, comments, user-facing application text, and test names are in English.

For interview preparation, [`ARCHITECTURE_DECISIONS.md`](./ARCHITECTURE_DECISIONS.md) explains the frontend and backend choices, alternatives, trade-offs, and likely evolution paths.

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

## API

- `GET /api/tickets`: filter by `status`, `priority`, and `search`; sort by `createdAt`, `priority`, or `status` using `sortDirection=asc|desc`.
- `GET /api/tickets/{id}`: ticket details, comments, and status history.
- `POST /api/tickets`: create a ticket.
- `PUT /api/tickets/{id}`: update a ticket; status changes are recorded in its history.
- `DELETE /api/tickets/{id}`: delete a ticket and its related comments/history.
- `POST /api/tickets/{id}/comments`: add a comment with a server-assigned mock author and timestamp.

Errors use ASP.NET Core validation problem responses. The API currently uses a fixed mock author and has no authentication.

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

Run only the Vue and TypeScript checks:

```powershell
cd support-desk-frontend
npm run type-check
```

Run backend unit and HTTP integration tests (the tests use an isolated in-memory SQLite database):

```powershell
dotnet test .\SupportDesk.Api.Tests\SupportDesk.Api.Tests.csproj
```

The backend tests cover ticket filtering and ordering, create/update/comment/delete rules, status history, request validation, and essential HTTP status codes. Frontend component and E2E automation remain out of scope; the frontend is type-checked and its main flows are manually smoke-tested.
