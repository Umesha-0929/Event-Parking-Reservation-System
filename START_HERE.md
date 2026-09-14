# START HERE - SEVPMS / Nvent

## Validate the complete repository

1. Extract the repository ZIP.
2. Open Command Prompt or PowerShell in the repository root.
3. Run:

```bat
VALIDATE_SEVPMS_FINAL.cmd
```

The script checks required tools, restores/builds/tests the ASP.NET solution, verifies and updates EF Core migrations, performs a clean `npm ci`, builds Angular in production mode and runs frontend tests.

## Run the API manually

```powershell
dotnet run --project backend/src/SEVPMS.Api/SEVPMS.Api.csproj
```

Development health endpoint:

```text
http://localhost:5090/api/health
```

## Run Angular manually

```powershell
cd frontend/sevpms-web
npm start
```

Then open `http://localhost:4200`.

## Local configuration

Use development user-secrets/environment variables for credentials and signing keys. Keep production secrets outside the repository.
