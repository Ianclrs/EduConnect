# Spec 20: Design — Multi-Tenant Architecture

## Design Approach

This spec follows the **shared database with discriminator column** pattern (ADR-002). Tenant isolation is enforced at two levels:

1. **Request level:** `TenantMiddleware` extracts `tenant_id` from the JWT claim and stores it in the scoped `TenantContext`.
2. **Query level:** EF Core global query filters append `WHERE TenantId = @currentTenantId` to every query against `ITenantScoped` entities, making cross-tenant data access structurally impossible.

The approach is **infrastructure-only** — it does not touch business logic or controllers. All other specs inherit multi-tenancy automatically by implementing `ITenantScoped` on their entities and registering them in `AppDbContext`.

## Architecture Decisions

### AD-001: GUID-based TenantId
- **Decision:** Use `Guid` for `TenantId` instead of sequential integers.
- **Rationale:** Prevents enumeration attacks. GUIDs are not guessable. Compatible with client-side generation if needed in the future.
- **Consequence:** Index fragmentation on `TenantId` columns. Mitigated by `uuid_generate_v4()` or sequential GUID generation (COMB GUID) if performance profiling shows it necessary.

### AD-002: Claim-based Tenant Resolution
- **Decision:** Tenant identity is carried as a JWT claim `"tenant_id"`, not as a header or query parameter.
- **Rationale:** JWT claims are cryptographically signed, cannot be tampered with by the client. Using a dedicated claim avoids namespace collisions.
- **Consequence:** The auth system (Spec 30) must include `tenant_id` in every JWT. TenantMiddleware has a hard dependency on `HttpContext.User` being populated by Authentication middleware.

### AD-003: Fail-Fast on Unresolved Tenant
- **Decision:** `ITenantContext.TenantId` throws `TenantNotResolvedException` if accessed before resolution.
- **Rationale:** Silent null/default values could lead to cross-tenant data leaks. Fail-fast ensures every code path that needs a tenant explicitly resolves one.
- **Consequence:** Services that can operate without a tenant (e.g., health checks) must check `ITenantContext.IsResolved` before accessing `TenantId`.

### AD-004: Auto-Set TenantId on SaveChanges
- **Decision:** `SaveChangesAsync` automatically sets `TenantId` on newly added `ITenantScoped` entities.
- **Rationale:** Reduces boilerplate. Developers don't need to remember to set `TenantId` manually. The global query filter already ensures reads are scoped; this ensures writes are scoped too.
- **Consequence:** If an entity is added with a different `TenantId` explicitly set, the auto-set will NOT override it. This is intentional — the explicit value wins, allowing admin cross-tenant operations if needed.

## Component / Module Breakdown

| Component | Responsibility | File | Depends On |
|---|---|---|---|
| `Tenant` | Domain entity representing a school/college | `src/Ciclo.Core/Entities/Tenant.cs` | Nothing |
| `ITenantScoped` | Marker interface for tenant-owned entities | `src/Ciclo.Core/Interfaces/ITenantScoped.cs` | Nothing |
| `ITenantContext` | Scoped accessor for current tenant ID | `src/Ciclo.Infrastructure/Tenancy/TenantContext.cs` | Nothing |
| `TenantContext` | Concrete scoped implementation | `src/Ciclo.Infrastructure/Tenancy/TenantContext.cs` | `ITenantContext` |
| `TenantNotResolvedException` | Exception for unresolved tenant access | `src/Ciclo.Infrastructure/Tenancy/TenantNotResolvedException.cs` | `InvalidOperationException` |
| `TenantMiddleware` | ASP.NET Core middleware extracting tenant from JWT | `src/Ciclo.Api/Middleware/TenantMiddleware.cs` | `TenantContext`, `HttpContext` |
| `AppDbContext` (updated) | DbContext with global query filters | `src/Ciclo.Infrastructure/Data/AppDbContext.cs` | `ITenantContext`, `ITenantScoped` |
| `TenantSeeder` | Seeds default tenant on first run | `src/Ciclo.Infrastructure/Data/Seeders/TenantSeeder.cs` | `AppDbContext`, `IHostEnvironment` |

## Data Flow

### Flow 1: Authenticated Request with Tenant

```
┌─────────────┐     ┌──────────────────┐     ┌─────────────────┐     ┌────────────────┐     ┌───────────────┐
│   Client    │────▶│ Authentication   │────▶│ TenantMiddleware│────▶│  Controller    │────▶│   Service     │
│  (SPA/PWA)  │     │ Middleware       │     │                 │     │  /Minimal API  │     │               │
│             │     │ (Spec 30)        │     │ 1. Extract      │     │                │     │               │
│  JWT with   │     │ Validates JWT    │     │    "tenant_id"  │     │                │     │               │
│  tenant_id  │     │ Populates        │     │    from Claims  │     │                │     │               │
│             │     │ HttpContext.User  │     │ 2. Parse as Guid│     │                │     │               │
│             │     │                  │     │ 3. Call         │     │                │     │               │
│             │     │                  │     │    SetTenant()  │     │                │     │               │
│             │     │                  │     │ 4. → next()     │     │                │     │               │
│             │     │                  │     │                 │     │                │     │               │
└─────────────┘     └──────────────────┘     └────────┬────────┘     └───────┬────────┘     ┌───────┴───────┐
│                                                     │                     │              │ ITenantContext │
│                                                     │ TenantId is now     │ injects       │ .TenantId     │
│                                                     │ in scoped context   └───────────────│ returns GUID  │
│                                                     │                                       └───────┬───────┘
│                                                     │                                               │
│                                                     │                              ┌────────────────┴────────────┐
│                                                     │                              │  Service calls             │
│                                                     │                              │  dbContext.Set<T>().ToList() │
│                                                     │                              │  → EF Core adds            │
│                                                     │                              │  WHERE TenantId = @ctx    │
│                                                     │                              └─────────────────────────────┘
└─────────────────────────────────────────────────────┘
```

**Step-by-step:**
1. Client sends request with `Authorization: Bearer <jwt>` header.
2. `UseAuthentication()` middleware validates JWT signature and expiration, populates `HttpContext.User` with claims including `"tenant_id"`.
3. `TenantMiddleware.InvokeAsync()`:
   a. Checks if `HttpContext.User.Identity.IsAuthenticated`.
   b. If not authenticated and endpoint has `[Authorize]` → return 401.
   c. If not authenticated and endpoint is public → call `_next(context)`, return (skip tenant resolution).
   d. If authenticated: reads `User.FindFirst("tenant_id")`.
   e. If claim missing → return 401 (`tenant_not_resolved`).
   f. Parses claim value as `Guid`. If parse fails → return 401 (`tenant_not_resolved`).
   g. Calls `_tenantContext.SetTenant(tenantId)`.
   h. Calls `await _next(context)`.
4. Controller/service receives request. Any `ITenantContext` injection returns the resolved `TenantId`.
5. Service queries `AppDbContext`. EF Core automatically appends `WHERE e.TenantId = @tenantId` to all queries against `ITenantScoped` entities.
6. Response flows back through the pipeline.

### Flow 2: SaveChangesAsync — Auto-Set TenantId

```
Service calls:                                    Override SaveChangesAsync:
  var student = new Student                      foreach entry in ChangeTracker
  {                                               .Entries<ITenantScoped>()
    Name = "...",                                 .Where(e => e.State == Added)
  };                                             {
  dbContext.Students.Add(student);                   entry.Entity.TenantId =
  await dbContext.SaveChangesAsync();                _tenantContext.TenantId;
                                                }
                                                → base.SaveChangesAsync()
```

### Flow 3: TenantSeeder on Startup

```
Program.cs                           TenantSeeder.SeedAsync()
  app started                      ┌──────────────────────────────
  └─► ApplyMigrations()            │ if (!IsDevelopment())
      └─► TenantSeeder.SeedAsync() │     return; // skip prod
          ├─ if Tenants.Any()      │
          │    → return            │ var defaultTenant = new Tenant {
          ├─ tenant = new Tenant   │     Name = "Default School",
          │    { Name = "Default   │     Slug = "default",
          │      School",          │     IsActive = true
          │      Slug = "default" │ };
          │    }                   │ dbContext.Tenants.Add(defaultTenant);
          └─ dbContext             │ await dbContext.SaveChangesAsync();
               .Tenants            │
               .Add(tenant)        │
               .SaveChangesAsync() │
```

## Domain Entities

### Tenant (Ciclo.Core/Entities/Tenant.cs)
```csharp
namespace Ciclo.Core.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;  // unique, URL-safe identifier
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### ITenantScoped (Ciclo.Core/Interfaces/ITenantScoped.cs)
```csharp
namespace Ciclo.Core.Interfaces;

/// <summary>
/// Marker interface for entities whose data is scoped to a specific tenant.
/// Any entity implementing this interface will have EF Core global query filters
/// applied automatically, ensuring data isolation between tenants.
/// </summary>
public interface ITenantScoped
{
    Guid TenantId { get; }
}
```

All entities that belong to a tenant (Student, Enrollment, Document, Notification, etc.) MUST implement this interface.

## Infrastructure

### ITenantContext + TenantContext (Ciclo.Infrastructure/Tenancy/TenantContext.cs)
```csharp
namespace Ciclo.Infrastructure.Tenancy;

public interface ITenantContext
{
    Guid TenantId { get; }
    bool IsResolved { get; }
}

public class TenantContext : ITenantContext
{
    private Guid? _tenantId;

    public Guid TenantId => _tenantId
        ?? throw new TenantNotResolvedException();

    public bool IsResolved => _tenantId.HasValue;

    public void SetTenant(Guid tenantId) => _tenantId = tenantId;
}
```

Registered as **Scoped** in DI.

### TenantNotResolvedException (Ciclo.Infrastructure/Tenancy/TenantNotResolvedException.cs)
```csharp
namespace Ciclo.Infrastructure.Tenancy;

public class TenantNotResolvedException : InvalidOperationException
{
    public TenantNotResolvedException()
        : base("No tenant context resolved for the current request.") { }

    public TenantNotResolvedException(string message)
        : base(message) { }
}
```

### TenantMiddleware (Ciclo.Api/Middleware/TenantMiddleware.cs)
```csharp
namespace Ciclo.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var endpoint = context.GetEndpoint();
        var requireAuth = endpoint?.Metadata
            .GetMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>() != null;

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("tenant_id");
            if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            {
                if (requireAuth)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        """{"error":"tenant_not_resolved","message":"Tenant context could not be resolved from the current request."}""");
                    return;
                }
            }
            else
            {
                tenantContext.SetTenant(tenantId);
            }
        }
        else if (requireAuth)
        {
            // No authenticated user but endpoint requires auth — let Authorization middleware handle it
        }

        await _next(context);
    }
}
```

**Error Handling Specification:**

| Error Condition | HTTP Status | Response Body | When |
|---|---|---|---|
| No `tenant_id` claim on authenticated request + `[Authorize]` | 401 | `{"error":"tenant_not_resolved",...}` | JWT valid but missing claim |
| `tenant_id` claim not a valid GUID + `[Authorize]` | 401 | `{"error":"tenant_not_resolved",...}` | Malformed claim |
| `ITenantContext.TenantId` accessed before resolution | N/A (throws) | `TenantNotResolvedException` | Programming error |
| Seeder runs in Production | N/A (skips) | N/A | `IWebHostEnvironment.IsDevelopment()` is false |

### AppDbContext Changes (Ciclo.Infrastructure/Data/AppDbContext.cs)

Constructor: Add `ITenantContext tenantContext` parameter, store as `_tenantContext`.

```csharp
public class AppDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Tenant entity configuration
        builder.Entity<Tenant>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.Slug).IsUnique();
            entity.Property(t => t.Name).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Slug).HasMaxLength(100).IsRequired();
            entity.Property(t => t.IsActive).HasDefaultValue(true);
            entity.Property(t => t.CreatedAt).HasDefaultValueSql("now()");
        });

        // Apply global query filter to all ITenantScoped entities
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var tenantIdProperty = Expression.Property(parameter, nameof(ITenantScoped.TenantId));
                var currentTenantId = Expression.Property(
                    Expression.Constant(_tenantContext), nameof(ITenantContext.TenantId));
                var filter = Expression.Lambda(
                    Expression.Equal(tenantIdProperty, currentTenantId), parameter);
                builder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AutoSetTenantId();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        AutoSetTenantId();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void AutoSetTenantId()
    {
        if (!_tenantContext.IsResolved) return;

        foreach (var entry in ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added && e.Entity is ITenantScoped))
        {
            var entity = (ITenantScoped)entry.Entity;
            if (entity.TenantId == Guid.Empty)
            {
                // Use reflection to set the property since ITenantScoped.TenantId is get-only
                var property = entry.Entity.GetType().GetProperty(nameof(ITenantScoped.TenantId));
                property?.SetValue(entry.Entity, _tenantContext.TenantId);
            }
        }
    }
}
```

## Data Seeding

### TenantSeeder (Ciclo.Infrastructure/Data/Seeders/TenantSeeder.cs)
```csharp
namespace Ciclo.Infrastructure.Data.Seeders;

public static class TenantSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext,
        IWebHostEnvironment env, ILogger logger)
    {
        if (!env.IsDevelopment())
        {
            logger.LogInformation("TenantSeeder: Skipping — not Development environment");
            return;
        }

        if (await dbContext.Tenants.AnyAsync())
        {
            logger.LogInformation("TenantSeeder: Tenants already exist, skipping");
            return;
        }

        var defaultTenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Default School",
            Slug = "default",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Tenants.Add(defaultTenant);
        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "TenantSeeder: Created default tenant '{Name}' (Id: {Id})",
            defaultTenant.Name, defaultTenant.Id);
    }
}
```

**Note:** Admin user seeding is NOT part of this spec. It belongs to Spec 30 (Authentication & Authorization), which seeds an admin user for each existing tenant including the default one.

## File / Module Layout

| File | Path | Purpose |
|---|---|---|
| Tenant entity | `src/Ciclo.Core/Entities/Tenant.cs` | Domain entity: school/college |
| ITenantScoped | `src/Ciclo.Core/Interfaces/ITenantScoped.cs` | Marker interface for tenant-scoped entities |
| ITenantContext + TenantContext | `src/Ciclo.Infrastructure/Tenancy/TenantContext.cs` | Scoped service holding current tenant ID |
| TenantNotResolvedException | `src/Ciclo.Infrastructure/Tenancy/TenantNotResolvedException.cs` | Exception for unresolved tenant access |
| TenantMiddleware | `src/Ciclo.Api/Middleware/TenantMiddleware.cs` | ASP.NET Core middleware: JWT claim → tenant context |
| TenantSeeder | `src/Ciclo.Infrastructure/Data/Seeders/TenantSeeder.cs` | Seeds default tenant on first dev run |
| AppDbContext (updated) | `src/Ciclo.Infrastructure/Data/AppDbContext.cs` | Add DbSet<Tenant>, global query filters, auto-set TenantId on SaveChanges |

### Registration in Program.cs

```csharp
// In Program.cs (or a dedicated extension method):
builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

// ... after builder.Build() ...

// Middleware pipeline order:
app.UseAuthentication();              // Spec 30 — validates JWT, populates User
app.UseMiddleware<TenantMiddleware>(); // Spec 20 — extracts tenant_id from User
app.UseAuthorization();               // Spec 30 — checks roles/policies
```

## Cross-Reference: Requirements → Design Coverage

| Requirement | Covered By |
|---|---|
| FR1: Tenant Entity | Domain Entities → Tenant, AppDbContext Changes |
| FR2: ITenantScoped Interface | Domain Entities → ITenantScoped |
| FR3: Tenant-Scoped Entities | AD-004, AppDbContext Changes (auto-set + filter) |
| FR4: Global Query Filter | AppDbContext Changes → OnModelCreating |
| FR5: TenantMiddleware | TenantMiddleware, Error Handling table |
| FR6: ITenantContext Service | ITenantContext + TenantContext, Program.cs registration |
| FR7: TenantNotResolvedException | TenantNotResolvedException |
| FR8: TenantSeeder | TenantSeeder |
| FR9: SaveChanges Auto-Set | AppDbContext Changes → AutoSetTenantId |
| FR10: Tenant Table in DbContext | AppDbContext Changes → DbSet<Tenant>, OnModelCreating config |
| NFR1: Performance | AD-001 (GUID), measured via BenchmarkDotNet in task T2.9 |
| NFR2: Security | AD-002 (claim-based), tenant_id only from JWT, GUID-based |
| NFR3: Reliability | AD-003 (fail-fast), TenantSeeder idempotency check |
| NFR4: Maintainability | File / Module Layout table |
| E1-E9 | Error Handling table, Edge Cases (E1-E10) |

---

## Implementation Notes (audit 2026-08-08)

Two intentional deviations from the original design were made during implementation:

### N1: TenantMiddleware injects `TenantContext` instead of `ITenantContext`

**Design:** `InvokeAsync(HttpContext context, ITenantContext tenantContext)`  
**Actual:** `InvokeAsync(HttpContext context, TenantContext tenantContext)`

**Reason:** `SetTenant()` is defined on the concrete `TenantContext` class, not on the `ITenantContext` interface. The interface exposes read-only access (`TenantId`, `IsResolved`) while mutation is a concern of the middleware. Both `TenantContext` and `ITenantContext` resolve to the same scoped instance via DI registration.

### N2: TenantSeeder uses `IHostEnvironment` instead of `IWebHostEnvironment`

**Design:** `SeedAsync(AppDbContext, IWebHostEnvironment, ILogger)`  
**Actual:** `SeedAsync(AppDbContext, IHostEnvironment, ILogger)`

**Reason:** `IWebHostEnvironment` requires the ASP.NET Core shared framework (only available in `Microsoft.NET.Sdk.Web` projects). The Infrastructure project uses `Microsoft.NET.Sdk`. `IHostEnvironment` (from `Microsoft.Extensions.Hosting.Abstractions`) provides the identical `IsDevelopment()` API without the ASP.NET dependency, keeping Infrastructure decoupled from the web layer.

### N3: Migration created manually

Migration `20260808190000_CreateTenantTable.cs` was hand-written because the `dotnet-ef` CLI tool is not installed and `dotnet tool install` is blocked by the SDK 10.0.302 NuGet bug (see L1 in `management/lessons.md`). The migration code is deterministic based on entity configuration.
