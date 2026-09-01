# Database Setup

The starter connection string uses SQL Server LocalDB:

```text
Server=(localdb)\MSSQLLocalDB;
Database=SEVPMSDb;
Trusted_Connection=True;
TrustServerCertificate=True;
MultipleActiveResultSets=true
```

After entities and EF configurations are ready:

```powershell
cd backend

dotnet ef migrations add InitialCreate `
  --project src/SEVPMS.Infrastructure/SEVPMS.Infrastructure.csproj `
  --startup-project src/SEVPMS.Api/SEVPMS.Api.csproj `
  --output-dir Persistence/Migrations

dotnet ef database update `
  --project src/SEVPMS.Infrastructure/SEVPMS.Infrastructure.csproj `
  --startup-project src/SEVPMS.Api/SEVPMS.Api.csproj
```

Coordinate migrations as a team. Do not allow multiple developers to generate competing migrations for the same shared model at the same time.
