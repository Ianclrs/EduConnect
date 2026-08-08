# Spec 10: Tasks — Project Bootstrap & Infrastructure

## Tasks

### T1.1: Create solution and projects
- [ ] Run `dotnet new sln -n EduGestor`
- [ ] Run `dotnet new webapi -n EduGestor.Api -o src/EduGestor.Api`
- [ ] Run `dotnet new classlib -n EduGestor.Core -o src/EduGestor.Core`
- [ ] Run `dotnet new classlib -n EduGestor.Infrastructure -o src/EduGestor.Infrastructure`
- [ ] Run `dotnet new xunit -n EduGestor.Api.Tests -o tests/EduGestor.Api.Tests`
- [ ] Add all projects to solution: `dotnet sln add src/*/`
- [ ] Add project references:
  - `EduGestor.Api` → `EduGestor.Infrastructure`, `EduGestor.Core`
  - `EduGestor.Infrastructure` → `EduGestor.Core`
  - `EduGestor.Api.Tests` → `EduGestor.Api`

### T1.2: Configure Directory.Build.props
- [ ] Create `Directory.Build.props` at solution root
- [ ] Set TargetFramework=net10.0, Nullable=enable, ImplicitUsings=enable, TreatWarningsAsErrors=true

### T1.3: Install NuGet packages
- [ ] EduGestor.Api: `dotnet add package Swashbuckle.AspNetCore --version 7.*`, `dotnet add package Serilog.AspNetCore --version 9.*`, `dotnet add package Microsoft.EntityFrameworkCore.Design --version 10.*`
- [ ] EduGestor.Infrastructure: `dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.*`, `dotnet add package Microsoft.EntityFrameworkCore --version 10.*`
- [ ] EduGestor.Api.Tests: `dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 10.*`

### T1.4: Create AppDbContext
- [ ] Create `src/EduGestor.Infrastructure/Data/AppDbContext.cs`
- [ ] Inherit from DbContext, empty DbSets for now
- [ ] Override OnModelCreating (empty, placeholder for future configs)
- [ ] Create DI registration extension: `AddInfrastructure(IConfiguration)` in DependencyInjection.cs

### T1.5: Create HealthController
- [ ] Create `src/EduGestor.Api/Controllers/HealthController.cs`
- [ ] Implement GET `/health` returning 200 with status JSON
- [ ] Implement GET `/health/db` checking `db.Database.CanConnectAsync()`

### T1.6: Configure Program.cs
- [ ] Configure Serilog with console sink
- [ ] Add services: Controllers, EndpointsApiExplorer, SwaggerGen
- [ ] Call `builder.Services.AddInfrastructure(builder.Configuration)`
- [ ] Use middleware: Swagger, Routing, MapControllers
- [ ] Add `app.UseSerilogRequestLogging()`
- [ ] Add db migration auto-apply in Development mode

### T1.7: Configure appsettings
- [ ] Create `src/EduGestor.Api/appsettings.json` with Serilog configuration (MinimumLevel: Information, override AspNetCore/EFCore to Warning) and AllowedHosts: *
- [ ] Create `src/EduGestor.Api/appsettings.Development.json` with Serilog Debug level and ConnectionStrings:Default = `Host=localhost;Database=edugestor;Username=edugestor;Password=edugestor_dev;Timeout=5;Command Timeout=30`

### T1.8: Create Dockerfile
- [ ] Create multi-stage Dockerfile in `src/EduGestor.Api/Dockerfile`
- [ ] Build stage: `mcr.microsoft.com/dotnet/sdk:10.0`
- [ ] Runtime stage: `mcr.microsoft.com/dotnet/aspnet:10.0`
- [ ] Expose port 8080

### T1.9: Create docker-compose.yml
- [ ] PostgreSQL 16-alpine service with healthcheck
- [ ] API service with build context, dependency on postgres healthy
- [ ] Named volume for pgdata

### T1.10: Create .gitignore and .dockerignore
- [ ] Run `dotnet new gitignore` at solution root (generates base .NET gitignore)
- [ ] Append to .gitignore: `.env`, `.vs/`, `.vscode/`, `.idea/`, `*.swp`, `*.swo`, `.DS_Store`, `Thumbs.db`
- [ ] Create `.dockerignore` at solution root with content from design.md (exclude .git, bin, obj, node_modules, .vs, .vscode, .env, Dockerfile*, docker-compose*, tests/)

### T1.11: Create .env.example
- [ ] `DB_PASSWORD=edugestor_dev` — placeholder value

### T1.12: Create HealthControllerTests
- [ ] Create `tests/EduGestor.Api.Tests/HealthControllerTests.cs`
- [ ] Use `WebApplicationFactory<Program>` for integration testing
- [ ] Test `GET /health` returns 200 OK and response body is `{"status":"Healthy"}`
- [ ] Test `GET /health/db` returns 200 OK when database is reachable (requires running PostgreSQL)
- [ ] Test `GET /health/db` returns 503 when database is unreachable (use `WebApplicationFactory` with override connection string pointing to non-existent host)
- [ ] Mark DB-dependent tests with `[Trait("Category", "Integration")]`

### T1.13: Verify
- [ ] `dotnet build` — zero errors/warnings
- [ ] `dotnet test` — all tests pass
- [ ] `docker compose up` — API responds on localhost:5000/health
