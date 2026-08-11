# Spec 10: Tasks — Project Bootstrap & Infrastructure

## Tasks

### T1.1: Create solution and projects
- [x] Run `dotnet new sln -n Ciclo`
- [x] Run `dotnet new webapi -n Ciclo.Api -o src/Ciclo.Api`
- [x] Run `dotnet new classlib -n Ciclo.Core -o src/Ciclo.Core`
- [x] Run `dotnet new classlib -n Ciclo.Infrastructure -o src/Ciclo.Infrastructure`
- [x] Run `dotnet new xunit -n Ciclo.Api.Tests -o tests/Ciclo.Api.Tests`
- [x] Add all projects to solution: `dotnet sln add src/*/`
- [x] Add project references:
  - `Ciclo.Api` → `Ciclo.Infrastructure`, `Ciclo.Core`
  - `Ciclo.Infrastructure` → `Ciclo.Core`
  - `Ciclo.Api.Tests` → `Ciclo.Api`

### T1.2: Configure Directory.Build.props
- [x] Create `Directory.Build.props` at solution root
- [x] Set TargetFramework=net10.0, Nullable=enable, ImplicitUsings=enable, TreatWarningsAsErrors=true

### T1.3: Install NuGet packages
- [x] Ciclo.Api: Swashbuckle.AspNetCore 10.*, Serilog.AspNetCore 9.*, Microsoft.EntityFrameworkCore.Design 10.*, Npgsql.EntityFrameworkCore.PostgreSQL 10.*
- [x] Ciclo.Infrastructure: Npgsql.EntityFrameworkCore.PostgreSQL 10.*, Microsoft.EntityFrameworkCore 10.*
- [x] Ciclo.Api.Tests: Microsoft.AspNetCore.Mvc.Testing 10.*

### T1.4: Create AppDbContext
- [x] Create `src/Ciclo.Infrastructure/Data/AppDbContext.cs`
- [x] Inherit from DbContext, empty DbSets for now
- [x] Override OnModelCreating (empty, placeholder for future configs)
- [x] Create DI registration extension: `AddInfrastructure(IConfiguration)` in DependencyInjection.cs

### T1.5: Create HealthController
- [x] Create `src/Ciclo.Api/Controllers/HealthController.cs`
- [x] Implement GET `/health` returning 200 with status JSON
- [x] Implement GET `/health/db` checking `db.Database.CanConnectAsync()`

### T1.6: Configure Program.cs
- [x] Configure Serilog with console sink
- [x] Add services: Controllers, EndpointsApiExplorer, SwaggerGen
- [x] Call `builder.Services.AddInfrastructure(builder.Configuration)`
- [x] Use middleware: Swagger, Routing, MapControllers
- [x] Add `app.UseSerilogRequestLogging()`
- [x] Add db migration auto-apply in Development mode

### T1.7: Configure appsettings
- [x] Create `src/Ciclo.Api/appsettings.json` with Serilog configuration (MinimumLevel: Information, override AspNetCore/EFCore to Warning) and AllowedHosts: *
- [x] Create `src/Ciclo.Api/appsettings.Development.json` with Serilog Debug level and ConnectionStrings:Default = `Host=localhost;Database=edugestor;Username=edugestor;Password=1234;Timeout=5;Command Timeout=30`

### T1.8: Create Dockerfile
- [x] Create multi-stage Dockerfile in `src/Ciclo.Api/Dockerfile`
- [x] Build stage: `mcr.microsoft.com/dotnet/sdk:10.0`
- [x] Runtime stage: `mcr.microsoft.com/dotnet/aspnet:10.0`
- [x] Expose port 8080

### T1.9: Create docker-compose.yml
- [x] PostgreSQL 16-alpine service with healthcheck
- [x] API service with build context, dependency on postgres healthy
- [x] Named volume for pgdata

### T1.10: Create .gitignore and .dockerignore
- [x] Run `dotnet new gitignore` at solution root (generates base .NET gitignore)
- [x] Append to .gitignore: `.env`, `.vs/`, `.vscode/`, `.idea/`, `*.swp`, `*.swo`, `.DS_Store`, `Thumbs.db`
- [x] Create `.dockerignore` at solution root with content from design.md (exclude .git, bin, obj, node_modules, .vs, .vscode, .env, Dockerfile*, docker-compose*, tests/)

### T1.11: Create .env.example
- [x] `DB_PASSWORD=1234` — placeholder value

### T1.12: Create HealthControllerTests
- [x] Create `tests/Ciclo.Api.Tests/HealthControllerTests.cs`
- [x] Use `WebApplicationFactory<Program>` for integration testing
- [x] Test `GET /health` returns 200 OK and response body is `{"status":"Healthy"}`
- [x] Test `GET /health/db` returns 200 OK when database is reachable (requires running PostgreSQL)
- [x] Test `GET /health/db` returns 503 when database is unreachable
- [x] Mark DB-dependent tests with `[Trait("Category", "Integration")]`

### T1.13: Create database init script
- [x] Create `db/init-db.sql` at solution root
- [x] Script creates `edugestor` role (if not exists) with password `1234`
- [x] Script creates `edugestor` database (if not exists) owned by `edugestor`
- [x] Script grants ALL PRIVILEGES on database and schema public to `edugestor`
- [x] Script uses `\set ON_ERROR_STOP on` to fail on first error
- [x] Script is idempotent — safe to run multiple times

### T1.14: Verify
- [x] `dotnet build` — zero errors/warnings
- [x] `dotnet test` — all tests pass (2/2)
- [ ] `docker compose up` — API responds on localhost:5000/health (Docker not available in this environment)
