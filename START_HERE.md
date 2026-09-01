# START HERE - SEVPMS Basement

## 1. Extract the ZIP

Place the extracted project where you want to work.

## 2. Open the solution

Open:

```text
backend/SEVPMS.sln
```

in Visual Studio, or open the repository root in VS Code.

## 3. Verify .NET

```powershell
dotnet --version
```

This starter targets `.NET 8`.

## 4. Restore and build

```powershell
cd backend
dotnet restore
dotnet build SEVPMS.sln
```

## 5. Run API

```powershell
dotnet run --project src/SEVPMS.Api/SEVPMS.Api.csproj
```

Open:

```text
http://localhost:5090/api/health
```

Expected shape:

```json
{
  "service": "SEVPMS.Api",
  "status": "ok",
  "utc": "..."
}
```

## 6. First Git commit suggestion

```powershell
git add .
git commit -m "chore: add SEVPMS N-tier project basement"
git push
```

## What NOT to do yet

- Do not create all database entities at once.
- Do not generate migrations before the shared schema is agreed.
- Do not put business logic inside controllers.
- Do not store secrets in `appsettings.json`.
- Do not let multiple members edit `Program.cs`, `DbContext`, migrations or shared contracts without coordination.

## Recommended first backend implementation order

1. Shared configuration and database conventions
2. Users / roles
3. Authentication / JWT
4. Events
5. Venues / rentals
6. Booking core
7. Payments
8. Receipts
9. Notifications + SignalR
10. Admin/shared APIs
11. Cross-module integration with seat/ticket and parking/food domains
