# SEVPMS / Nvent

Smart Event, Venue & Parking Management Platform implemented with Angular, ASP.NET Core Web API, Entity Framework Core, SQL Server and SignalR.

## Repository structure

- `frontend/sevpms-web` - Angular customer, organizer, venue-owner and admin application.
- `backend/src/SEVPMS.Api` - HTTP API, authentication, authorization, middleware and SignalR hosting.
- `backend/src/SEVPMS.Application` - use cases, DTOs, validation and interfaces.
- `backend/src/SEVPMS.Domain` - domain entities and enums.
- `backend/src/SEVPMS.Infrastructure` - EF Core, repositories and external providers.
- `backend/src/SEVPMS.Realtime` - realtime contracts and dispatching.
- `backend/tests` - backend unit/integration tests.

## Local validation

From the repository root run `VALIDATE_SEVPMS_FINAL.cmd`. It restores, builds and tests the backend, verifies/applies EF migrations, performs a clean frontend install, creates the Angular production build and runs frontend tests.

## Local application URLs

The development API is configured around `http://localhost:5090`. Angular development normally runs on `http://localhost:4200`. Production SSR can proxy `/api` and `/hubs` to the backend by setting `BACKEND_ORIGIN`.

## Configuration and secrets

Do not store production credentials in source control. Configure SQL Server, JWT signing, SMTP, PayHere and ticket QR signing through environment-specific configuration, environment variables or user-secrets. Production startup validates security-sensitive configuration.

## Main platform journeys

The application includes authentication/email verification, event and venue management, organizer-configured seating and seat views, booking/ticket/QR check-in, venue marketplace/rentals, parking, food/place services, payments/receipts, notifications and administrative workspaces.
