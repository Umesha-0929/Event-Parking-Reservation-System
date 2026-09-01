# SEVPMS - Smart Event, Venue & Parking Management Platform

Student-friendly **N-tier / layered architecture starter** for the SEVPMS team project.

## Technology baseline

- Frontend: Angular + TypeScript + Angular HttpClient
- Backend: ASP.NET Core Web API
- Data access: Entity Framework Core
- Database: SQL Server
- Realtime: ASP.NET Core SignalR

## Backend layers

```text
SEVPMS.Domain
      ↑
SEVPMS.Application
   ↑            ↑
Infrastructure  Realtime
      \        /
       SEVPMS.Api
```

### Responsibilities

- **Domain**: core entities, enums, value objects and domain rules.
- **Application**: use cases, DTOs, interfaces, validation and application-level logic.
- **Infrastructure**: EF Core, SQL Server, repositories and external-provider implementations.
- **Realtime**: SignalR hubs, dispatchers, group names and event contracts.
- **Api**: HTTP entry point, controllers, middleware, authorization and configuration.

## Team ownership guide

- **Abimanju**: shared/core backend - auth, users, events, venues/rentals, booking core, payments, receipts, notifications, SignalR infrastructure, admin/shared APIs.
- **Klegar**: Angular frontend + seat/ticket backend domain.
- **Nidhushiya**: vehicles, parking, parking recommendation/navigation, food and place finder backend.
- **Yumesha**: Team Lead + major Angular frontend/integration work.

## First setup

Open a terminal at the repository root:

```powershell
cd backend
dotnet restore
dotnet build SEVPMS.sln
dotnet run --project src/SEVPMS.Api/SEVPMS.Api.csproj
```

Then open the API URL shown by the terminal. A starter health endpoint is available at:

```text
GET /api/health
```

## Important

This ZIP is a **basement/starter**, not the completed application. It intentionally avoids putting fake business logic into every feature. Feature folders are reserved so each team member can implement their owned module without reorganizing the solution later.

The starter targets **.NET 8** as an implementation assumption for the generated project files. If your mentor/team freezes a different .NET version, change `TargetFramework` and package versions together before starting feature work.
