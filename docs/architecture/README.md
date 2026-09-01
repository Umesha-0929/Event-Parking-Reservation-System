# SEVPMS N-Tier Architecture

## Dependency rule

```text
Domain
  ↑
Application
  ↑              ↑
Infrastructure   Realtime
       \         /
           Api
```

### Domain
Must not depend on API, EF Core, SQL Server, SignalR, payment providers, SMS or email providers.

### Application
Contains use-case contracts and business orchestration. Depends on Domain.

### Infrastructure
Implements persistence and external-provider interfaces. Depends on Application + Domain.

### Realtime
Contains SignalR hubs and realtime dispatching. Depends on Application.

### Api
Composition root and HTTP boundary. Depends on Application, Infrastructure and Realtime.

## Cross-team rule

Do not let feature developers move shared entities, rename API contracts or create overlapping EF migrations without coordination.
