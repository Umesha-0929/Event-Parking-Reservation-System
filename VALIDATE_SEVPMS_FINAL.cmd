@echo off
setlocal EnableExtensions EnableDelayedExpansion
cd /d "%~dp0"

set "SOLUTION=backend\SEVPMS.sln"
set "API_PROJECT=backend\src\SEVPMS.Api\SEVPMS.Api.csproj"
set "INFRA_PROJECT=backend\src\SEVPMS.Infrastructure\SEVPMS.Infrastructure.csproj"
set "FRONTEND=frontend\sevpms-web"
set "ASPNETCORE_ENVIRONMENT=Development"
set "Jwt__Key=SEVPMS_LOCAL_VALIDATION_ONLY_JWT_KEY_2026_CHANGE_FOR_REAL_DEPLOYMENT"
set "Klegar__TicketQrSigningKey=SEVPMS_LOCAL_VALIDATION_ONLY_TICKET_QR_KEY_2026_CHANGE_FOR_REAL_DEPLOYMENT"
set "SEVPMS_VALIDATION_ROOT=%CD%"

call :step "Removing Windows download security blocks"
powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-ChildItem -LiteralPath $env:SEVPMS_VALIDATION_ROOT -Recurse -File -ErrorAction SilentlyContinue | Unblock-File -ErrorAction SilentlyContinue" >nul 2>nul

call :step "Checking required tools"
where dotnet >nul 2>nul || (set "FAIL_MESSAGE=.NET SDK was not found. Install the SDK version requested by global.json." & goto :fail)
where node >nul 2>nul || (set "FAIL_MESSAGE=Node.js was not found. Install Node.js 22 or newer." & goto :fail)
where npm >nul 2>nul || (set "FAIL_MESSAGE=npm was not found. Reinstall Node.js with npm." & goto :fail)
dotnet --version || (set "FAIL_MESSAGE=dotnet could not start." & goto :fail)
node --version || (set "FAIL_MESSAGE=Node.js could not start." & goto :fail)
call npm --version || (set "FAIL_MESSAGE=npm could not start." & goto :fail)

call :step "Restoring local .NET tools"
dotnet tool restore || (set "FAIL_MESSAGE=dotnet tool restore failed." & goto :fail)

call :step "Restoring backend packages"
dotnet restore "%SOLUTION%" || (set "FAIL_MESSAGE=dotnet restore failed." & goto :fail)

call :step "Building backend - Release"
dotnet build "%SOLUTION%" --configuration Release --no-restore || (set "FAIL_MESSAGE=dotnet build failed." & goto :fail)

call :step "Running backend tests"
dotnet test "%SOLUTION%" --configuration Release --no-build || (set "FAIL_MESSAGE=dotnet test failed." & goto :fail)

call :step "Checking EF Core migrations"
dotnet ef migrations list --project "%INFRA_PROJECT%" --startup-project "%API_PROJECT%" --configuration Release --no-build || (set "FAIL_MESSAGE=EF migration listing failed." & goto :fail)
dotnet ef migrations has-pending-model-changes --project "%INFRA_PROJECT%" --startup-project "%API_PROJECT%" --configuration Release --no-build || (set "FAIL_MESSAGE=EF reports model changes that are not represented by a migration." & goto :fail)

call :step "Updating the configured database"
dotnet ef database update --project "%INFRA_PROJECT%" --startup-project "%API_PROJECT%" --configuration Release --no-build || (set "FAIL_MESSAGE=EF database update failed. Check SQL Server or LocalDB and the configured connection string." & goto :fail)

call :step "Installing frontend dependencies with npm ci"
pushd "%FRONTEND%" || (set "FAIL_MESSAGE=Frontend folder was not found." & goto :fail)
call npm ci || (set "FAIL_MESSAGE=npm ci failed." & popd & goto :fail)

call :step "Building Angular production bundle"
call npm run build -- --configuration production || (set "FAIL_MESSAGE=Angular production build failed." & popd & goto :fail)

call :step "Running Angular frontend tests"
call npm run test:ci || (set "FAIL_MESSAGE=Frontend unit tests failed." & popd & goto :fail)

call :step "Running frontend route smoke E2E"
call npm run e2e || (set "FAIL_MESSAGE=Frontend E2E smoke test failed." & popd & goto :fail)
popd

goto :success

:step
echo.
echo ============================================================
echo  %~1
echo ============================================================
exit /b 0

:success
echo.
echo ============================================================
echo  SEVPMS / Nvent final validation PASSED
echo ============================================================
exit /b 0

:fail
echo.
echo ============================================================
echo  VALIDATION FAILED
echo  !FAIL_MESSAGE!
echo  Fix the error above, then run VALIDATE_SEVPMS_FINAL.cmd again.
echo ============================================================
exit /b 1
