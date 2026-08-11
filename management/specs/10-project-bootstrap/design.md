# Spec 10: Design — Project Bootstrap & Infrastructure

## Design Approach

**Principle:** This spec follows the "Clean Architecture Lite" pattern — the simplest possible project separation that still enforces dependency direction. Core has zero dependencies (pure domain). Infrastructure depends on Core. API depends on both. No MediatR, no CQRS, no repository pattern — direct `AppDbContext` injection into controllers for this bootstrap phase. These patterns will be introduced in later specs ONLY when justified by complexity.

**Rationale:** The bootstrap spec must be the simplest runnable skeleton. Over-engineering at this stage creates coupling debt that cascades through all dependent specs. The design explicitly avoids:
- Repository pattern (not needed with EF Core as the repository)
- MediatR/CQRS (no business logic yet)
- AutoMapper (no DTOs yet)
- Custom middleware (no cross-cutting concerns yet)

These can be added in Specs 2-10 as genuine needs arise.

## Data Flow

```
HTTP Request                                                 HTTP Response
    │                                                              ▲
    ▼                                                              │
┌──────────────┐    ┌──────────────────┐    ┌──────────────┐    ┌─┴──────────┐
│ ASP.NET Core │───▶│  HealthController│───▶│ AppDbContext │───▶│ JSON Body  │
│ Middleware   │    │  (Endpoint)      │    │ (EF Core)    │    │ 200/503    │
│ Pipeline     │    └──────────────────┘    └──────┬───────┘    └────────────┘
└──────────────┘                                    │
      │                                             ▼
      │                                    ┌──────────────────┐
      └───────────────────────────────────▶│ PostgreSQL 16    │
           (Serilog logs every request)    │ (Docker)         │
                                           └──────────────────┘
```

**Step-by-step for `/health`:**
1. HTTP GET arrives at ASP.NET Core pipeline.
2. Serilog logs the request (method, path, status).
3. Routing middleware matches `/health` to `HealthController.Get()`.
4. Controller returns `Ok(new { status = "Healthy" })` — no DB call.
5. ASP.NET Core serializes to JSON and returns 200.

**Step-by-step for `/health/db`:**
1. HTTP GET arrives. Serilog logs the request.
2. Routing middleware matches `/health/db` to `HealthController.GetDb(AppDbContext db)`.
3. DI injects scoped `AppDbContext` (resolved from request scope).
4. `db.Database.CanConnectAsync()` opens a connection to PostgreSQL via Npgsql.
5. If connected → 200 OK with `{"status":"Healthy","database":"Connected"}`.
6. If connection fails (timeout, NpgsqlException) → 503 with `{"status":"Unhealthy","database":"Disconnected"}`. Exception is caught and logged by Serilog.

**Startup data flow:**
1. `Program.cs` creates WebApplication builder.
2. Serilog is configured from `appsettings.json` + code defaults.
3. `builder.Services.AddInfrastructure(builder.Configuration)` registers:
   - `AppDbContext` with Npgsql provider.
   - Connection string from `ConnectionStrings:Default`.
4. `builder.Services.AddControllers()` + `AddEndpointsApiExplorer()` + `AddSwaggerGen()`.
5. App is built: `builder.Build()`.
6. Middleware pipeline: `UseSwagger()` → `UseSwaggerUI()` → `UseSerilogRequestLogging()` → `UseRouting()` → `MapControllers()`.
7. If `ASPNETCORE_ENVIRONMENT=Development`: `db.Database.Migrate()` auto-applies pending EF Core migrations.
8. `app.Run()` — blocking call, API starts listening.

## Architecture Decisions

### ADR within this spec (design-level, not project-level)

| Decision | Rationale | Alternatives Rejected |
|---|---|---|
| Use `AppDbContext` directly in controller (no service layer) | Bootstrap has zero business logic. Service layer would be a pass-through with no value | Service layer (added in Spec 40+) |
| Use `AddInfrastructure(IConfiguration)` extension method | Keeps Program.cs clean and gives Infrastructure control over its own DI registration | Inline registration in Program.cs |
| Use `dotnet new` CLI for project scaffolding | Deterministic, reproducible, no manual XML editing of csproj files | Manual project creation |
| Health endpoints in `HealthController` | Separation from future business endpoints. Clear responsibility boundary | Adding health to a generic controller |
| Floating NuGet versions (`10.*`) | Allows patch updates without spec changes. Major version locked to .NET 10 | Exact versions (require spec updates for every patch) |
| Multi-stage Dockerfile | Separates build dependencies from runtime image. Smaller final image | Single-stage (larger image, SDK shipped to production) |
| `ConnectionStrings:Default` naming | ASP.NET Core convention. EF Core tooling (`dotnet ef`) auto-discovers this key | Custom key name (breaks tooling) |
| PostgreSQL healthcheck via `pg_isready` in Docker Compose | Native PostgreSQL tool, zero dependencies. Prevents API from starting before DB is ready | TCP port check (DB might not be accepting connections yet) |
| `TreatWarningsAsErrors=true` | Force zero-warning builds from day one. Warnings become debt if allowed to accumulate | Allow warnings (accumulates technical debt) |

## Component / Module Breakdown

### Ciclo.Api
- **Responsibility:** HTTP endpoint hosting, middleware pipeline, configuration loading.
- **Public surface:** REST endpoints at configured ports. Swagger UI in Development.
- **Dependencies:** Ciclo.Core, Ciclo.Infrastructure.
- **Reasons to change:** New controller added, middleware pipeline reordered, new NuGet package for API layer.

### Ciclo.Core
- **Responsibility:** Domain entities, value objects, interfaces. Pure C# — no external packages.
- **Public surface:** All public classes and interfaces in the `Ciclo.Core` namespace.
- **Dependencies:** None (zero package references).
- **Reasons to change:** New domain entity added, new interface defined, new enum created.

### Ciclo.Infrastructure
- **Responsibility:** Data access (EF Core), external service integrations, DI registration.
- **Public surface:** `AddInfrastructure(IConfiguration)` extension method. `AppDbContext` class. Services registered in DI.
- **Dependencies:** Ciclo.Core, Npgsql.EntityFrameworkCore.PostgreSQL.
- **Reasons to change:** New DbSet added, new external service integrated, connection string format changed.

### Ciclo.Api.Tests
- **Responsibility:** Integration tests for API endpoints. Uses `WebApplicationFactory<Program>`.
- **Public surface:** Test classes and methods (xUnit).
- **Dependencies:** Ciclo.Api, Microsoft.AspNetCore.Mvc.Testing.
- **Reasons to change:** New controller needs tests, existing endpoint behavior changes.

## File / Module Layout

```
Ciclo/
├── Ciclo.sln
├── docker-compose.yml
├── .dockerignore
├── .gitignore
├── .env.example
├── Directory.Build.props
├── db/
│   └── init-db.sql                  # SQL script to create database + user (no Docker required)
│
├── src/
│   ├── Ciclo.Api/                  # ASP.NET Core Web API
│   │   ├── Ciclo.Api.csproj
│   │   ├── Program.cs                  # App entry point + DI + middleware pipeline
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Controllers/
│   │   │   └── HealthController.cs     # /health + /health/db endpoints
│   │   └── Dockerfile
│   │
│   ├── Ciclo.Core/                 # Domain layer — zero dependencies
│   │   ├── Ciclo.Core.csproj
│   │   └── (empty — populated by later specs)
│   │
│   └── Ciclo.Infrastructure/       # EF Core, external services
│       ├── Ciclo.Infrastructure.csproj
│       ├── Data/
│       │   └── AppDbContext.cs         # EF Core DbContext (empty, no entities yet)
│       └── DependencyInjection.cs      # Extension method: AddInfrastructure()
│
└── tests/
    └── Ciclo.Api.Tests/
        ├── Ciclo.Api.Tests.csproj
        └── HealthControllerTests.cs
```

## Project Dependencies

```
Ciclo.Api ──► Ciclo.Infrastructure ──► Ciclo.Core
Ciclo.Api ──► Ciclo.Core
Ciclo.Api.Tests ──► Ciclo.Api
```

## Technology Versions

| Component | Version |
|---|---|
| .NET SDK | 10.0.x |
| Target Framework | net10.0 |
| ASP.NET Core | 10.0.x |
| EF Core | 10.0.x |
| Npgsql EF Core | 10.0.x |
| PostgreSQL | 16-alpine (Docker) |
| Swashbuckle | 10.x |
| Serilog.AspNetCore | 9.x |

## NuGet Packages (Ciclo.Api)

```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.*" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.*" />
<PackageReference Include="Serilog.AspNetCore" Version="9.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.*" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.*" />
<InternalsVisibleTo Include="Ciclo.Api.Tests" />
```

## NuGet Packages (Ciclo.Infrastructure)

```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.*" />
```

## NuGet Packages (Ciclo.Core)

```xml
<!-- No packages — pure domain layer -->
```

## AppDbContext (src/Ciclo.Infrastructure/Data/AppDbContext.cs)

```csharp
using Microsoft.EntityFrameworkCore;

namespace Ciclo.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Placeholder — entity configurations added by Specs 2-10
    }
}
```

## DependencyInjection (src/Ciclo.Infrastructure/DependencyInjection.cs)

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Ciclo.Infrastructure.Data;

namespace Ciclo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));
        return services;
    }
}
```

## Database Setup (Alternative: No Docker)

For developers who already have PostgreSQL installed locally and prefer not to use Docker, a SQL initialization script is provided at `db/init-db.sql`.

**Usage:**

```bash
# Run as postgres superuser
psql -U postgres -f db/init-db.sql
```

**What the script does:**

1. Creates the `edugestor` role with password `1234` (if it doesn't already exist)
2. Creates the `edugestor` database owned by `edugestor` (if it doesn't already exist)
3. Grants `ALL PRIVILEGES` on database and schema `public` to `edugestor`

**After running the script**, configure the connection string in `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "Default": "Host=localhost;Database=edugestor;Username=edugestor;Password=1234;Timeout=5;Command Timeout=30"
}
```

Then run the API normally:

```bash
dotnet run --project src/Ciclo.Api
```

The EF Core auto-migration (in Development mode) creates all tables automatically.

## docker-compose.yml

```yaml
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: edugestor
      POSTGRES_USER: edugestor
      POSTGRES_PASSWORD: ${DB_PASSWORD:-1234}
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U edugestor"]
      interval: 5s
      timeout: 5s
      retries: 5

  api:
    build:
      context: .
      dockerfile: src/Ciclo.Api/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Default=Host=postgres;Database=edugestor;Username=edugestor;Password=${DB_PASSWORD:-1234}
    ports:
      - "5000:8080"
    depends_on:
      postgres:
        condition: service_healthy

volumes:
  pgdata:
```

## Dockerfile (src/Ciclo.Api/Dockerfile)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies (layer caching)
COPY src/Ciclo.Core/Ciclo.Core.csproj src/Ciclo.Core/
COPY src/Ciclo.Infrastructure/Ciclo.Infrastructure.csproj src/Ciclo.Infrastructure/
COPY src/Ciclo.Api/Ciclo.Api.csproj src/Ciclo.Api/
RUN dotnet restore src/Ciclo.Api/Ciclo.Api.csproj

# Copy all source and build
COPY . .
RUN dotnet publish src/Ciclo.Api/Ciclo.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Ciclo.Api.dll"]
```

## appsettings.json (src/Ciclo.Api/appsettings.json)

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    }
  },
  "AllowedHosts": "*"
}
```

## appsettings.Development.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  },
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=edugestor;Username=edugestor;Password=1234;Timeout=5;Command Timeout=30"
  }
}
```

## HealthController

```csharp
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "Healthy" });

    [HttpGet("db")]
    public async Task<IActionResult> GetDb(AppDbContext db)
    {
        var canConnect = await db.Database.CanConnectAsync();
        return canConnect
            ? Ok(new { status = "Healthy", database = "Connected" })
            : StatusCode(503, new { status = "Unhealthy", database = "Disconnected" });
    }
}
```

## Program.cs (src/Ciclo.Api/Program.cs)

```csharp
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Ciclo.Infrastructure;
using Ciclo.Infrastructure.Data;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration)
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));

    // Services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // Middleware pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();
    app.MapControllers();

    // Auto-migrate database in Development
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            await db.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Database migration failed — database may not be available");
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
```

## Directory.Build.props (root)

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsNotAsErrors>$(WarningsNotAsErrors);NU1903</WarningsNotAsErrors>
    <AnalysisLevel>latest-recommended</AnalysisLevel>
    <NoWarn>$(NoWarn);NU1903;CA1707</NoWarn>
  </PropertyGroup>
</Project>
```

## .gitignore

Generated by executing `dotnet new gitignore` at the solution root. Then append the following entries to the end of the file:

```gitignore
# Environment files
.env

# IDE
.vs/
.vscode/
.idea/
*.swp
*.swo

# OS files
.DS_Store
Thumbs.db
```

The base template from `dotnet new gitignore` already excludes `bin/`, `obj/`, `*.user`, `packages/`, and all standard .NET build artifacts. The above additions cover secrets, editor files, and OS metadata not covered by the base template.

## .dockerignore

```dockerignore
**/.git/
**/bin/
**/obj/
**/node_modules/
**/.vs/
**/.vscode/
**/.env
**/Dockerfile*
**/docker-compose*

# Exclude test projects from Docker build context
tests/
```
