---
name: Multi-Tenant Architecture
status: planned
references: V2, ADR-002
---

# Spec 20: Multi-Tenant Architecture

## Value Delivery

This spec delivers **V2: Multi-Tenant Architecture** from `management/vision.md`. Specifically:

- **Complete data isolation between tenants (schools):** Every tenant-scoped entity carries a `TenantId` discriminator column. EF Core global query filters append `WHERE TenantId = @currentTenantId` to every query against tenant-scoped tables, making it **structurally impossible** for queries to leak data between tenants.
- **Per-request tenant resolution from JWT:** A middleware extracts the `tenant_id` claim from the authenticated user's JWT on every HTTP request and sets the current tenant context.
- **Scoped DI tenant context:** `ITenantContext` is registered as scoped and provides the current `TenantId` to any service, repository, or DbContext within the request pipeline.
- **Seeded default tenant:** The system ships with a default tenant for initial setup, seeded on first run.

## Functional Requirements

### FR1: Tenant Entity
- The system MUST have a `Tenant` entity with fields: `Id` (Guid, PK), `Name` (string, required, max 200), `Slug` (string, required, max 100, unique, URL-safe: lowercase alphanumeric + hyphens only), `IsActive` (bool, default true), `CreatedAt` (DateTime, UTC).
- Acceptance: EF Core migration creates a `Tenants` table with a unique index on `Slug`. Model validation rejects slugs that are not lowercase alphanumeric + hyphens.

### FR2: ITenantScoped Interface
- The system MUST define `ITenantScoped` interface in `EduGestor.Core/Interfaces/` with a single property: `Guid TenantId { get; }`.
- Acceptance: The interface compiles and can be implemented by any entity class.

### FR3: Tenant-Scoped Entities
- All entities that contain data belonging to a specific tenant (Student, Enrollment, Document, Notification, etc.) MUST implement `ITenantScoped`.
- Acceptance: At implementation time, any entity marked as tenant-scoped that does NOT implement `ITenantScoped` fails the build via a Roslyn analyzer or compile-time check (manual enforcement until Spec 30+ entities exist).

### FR4: Global Query Filter
- `AppDbContext.OnModelCreating` MUST iterate over all entity types implementing `ITenantScoped` and apply a global query filter: `e => e.TenantId == _currentTenantId`.
- The filter MUST use `ITenantContext.TenantId` injected into `AppDbContext`.
- Acceptance: Executing `dbContext.Set<SomeTenantEntity>().ToListAsync()` returns ONLY rows where `TenantId` matches the current tenant context. A SQL Profiler trace shows `WHERE TenantId = @p0` appended to every query.

### FR5: TenantMiddleware
- A `TenantMiddleware` MUST run AFTER `UseAuthentication()` and BEFORE `UseAuthorization()` in the ASP.NET Core pipeline.
- It MUST extract the `"tenant_id"` claim (type: `ClaimTypes.NameIdentifier` or custom claim `"tenant_id"`) from `HttpContext.User`.
- It MUST parse the claim value as `Guid` and call `ITenantContext.SetTenant(tenantId)`.
- If the endpoint has `[Authorize]` attribute and the `"tenant_id"` claim is missing or not a valid Guid, the middleware MUST return HTTP 401 with body: `{ "error": "tenant_not_resolved", "message": "Tenant context could not be resolved from the current request." }`.
- If the endpoint does NOT have `[Authorize]`, the middleware MUST silently continue without setting the tenant (tenant resolution is optional for public endpoints).
- Acceptance: A request with a valid JWT containing `tenant_id: "00000000-0000-0000-0000-000000000001"` sets the tenant context. A request with no JWT on a `[Authorize]` endpoint returns 401.

### FR6: ITenantContext Service
- `ITenantContext` MUST be registered as **Scoped** in DI.
- Interface MUST expose: `Guid TenantId { get; }` (throws `TenantNotResolvedException` if unresolved) and `bool IsResolved { get; }`.
- The concrete `TenantContext` MUST have `SetTenant(Guid tenantId)` method.
- Acceptance: Within the same HTTP request scope, any service injecting `ITenantContext` receives the same `TenantId` set by the middleware.

### FR7: TenantNotResolvedException
- Calling `ITenantContext.TenantId` when no tenant is resolved MUST throw `TenantNotResolvedException`.
- The exception MUST inherit from `InvalidOperationException`.
- Message MUST be: `"No tenant context resolved for the current request."`
- Acceptance: Unit test verifies that accessing `TenantId` on a fresh `TenantContext` throws `TenantNotResolvedException`.

### FR8: TenantSeeder
- On application startup (development mode only), a `TenantSeeder` MUST check if the `Tenants` table is empty.
- If empty, it MUST create a default tenant with: `Name = "Default School"`, `Slug = "default"`, `IsActive = true`.
- The seeder MUST run after EF Core migrations are applied.
- Acceptance: First run of the application creates exactly one tenant. Subsequent runs do not duplicate.

### FR9: SaveChanges Auto-Set TenantId
- `AppDbContext.SaveChangesAsync` (both overloads) MUST be overridden to iterate over `ChangeTracker.Entries<ITenantScoped>()` that are in `Added` state and auto-set `TenantId` to `_tenantContext.TenantId`.
- This ensures that when a service creates a new tenant-scoped entity without explicitly setting `TenantId`, it is automatically populated.
- Acceptance: Creating a `new Student()` and calling `dbContext.Students.Add(student)` followed by `SaveChangesAsync()` persists the student with the current tenant's `TenantId` even if `student.TenantId` was not explicitly set.

### FR10: Tenant Table in AppDbContext
- `AppDbContext` MUST expose `DbSet<Tenant> Tenants { get; set; }`.
- `Tenant` entity configuration in `OnModelCreating` MUST include: unique index on `Slug`, max length constraint on `Name` (200) and `Slug` (100).
- Acceptance: EF Core migration generates the `Tenants` table with the specified constraints.

## Non-Functional Requirements

### NFR1: Performance
- Tenant resolution (middleware + context set) MUST add no more than **1ms** overhead per request (measured as median time across 10,000 requests).
- Global query filter application MUST add no more than **5ms** overhead to query execution (measured on a 100k-row table via BenchmarkDotNet).

### NFR2: Security
- TenantId MUST NEVER be settable from client input (query string, body, or header). It MUST be derived exclusively from the JWT claim.
- The global query filter MUST NOT be bypassable. There MUST be no `IgnoreQueryFilters()` usage in any production code path. A Roslyn analyzer or code review policy MUST enforce this.
- `TenantId` values MUST be GUIDs (not sequential integers) to prevent enumeration attacks.

### NFR3: Reliability
- If `ITenantContext.TenantId` is accessed without a resolved tenant, the system MUST throw `TenantNotResolvedException` (fail-fast). No silent data corruption or cross-tenant data access is acceptable.
- The `TenantSeeder` MUST be idempotent — running it multiple times MUST NOT create duplicate tenants.

### NFR4: Maintainability (AI-First)
- All tenant-related infrastructure code MUST be in exactly two directories: `src/EduGestor.Infrastructure/Tenancy/` and `src/EduGestor.Api/Middleware/`.
- The `ITenantScoped` interface MUST be in `src/EduGestor.Core/Interfaces/`.
- The `Tenant` entity MUST be in `src/EduGestor.Core/Entities/`.

## Constraints

- Depends on **Spec 10** (Project Bootstrap & Infrastructure) for: solution structure, `AppDbContext`, Docker Compose, DI container.
- This spec does NOT include authentication logic — it assumes a valid JWT with `tenant_id` claim is already present (provided by Spec 30).
- This spec does NOT seed admin users for tenants — admin user seeding is handled by Spec 30's seeder.
- All code MUST be in C# 13 / .NET 10.
- Database MUST be PostgreSQL 16 via Npgsql EF Core provider.

## Edge Cases & Error States

### E1: Missing tenant_id claim
- **Scenario:** Authenticated user's JWT lacks the `tenant_id` claim.
- **Expected:** TenantMiddleware returns HTTP 401 with error `tenant_not_resolved`.

### E2: Invalid tenant_id claim format
- **Scenario:** JWT contains `tenant_id` claim that is not a valid GUID.
- **Expected:** TenantMiddleware returns HTTP 401 with error `tenant_not_resolved`.

### E3: TenantId not found in database
- **Scenario:** JWT contains a valid `tenant_id` GUID, but no row exists in `Tenants` table with that Id.
- **Expected:** TenantMiddleware sets the tenant context anyway (it only resolves the claim). The first query against a tenant-scoped entity returns zero results (no data leak, just empty result set). Future specs may add a tenant existence check.

### E4: Public endpoint (no [Authorize])
- **Scenario:** Request hits a public endpoint without authentication (e.g., health check, login).
- **Expected:** `HttpContext.User` is unauthenticated or lacks claims. TenantMiddleware detects no `tenant_id` claim and silently continues. `ITenantContext.IsResolved` returns `false`. Any attempt to query tenant-scoped data throws `TenantNotResolvedException`.

### E5: Background job / non-HTTP context
- **Scenario:** A Hangfire/Quartz background job needs to operate in a tenant context (e.g., send notifications for a specific school).
- **Expected (design note):** This spec does NOT handle background job tenant resolution. Future specs that introduce background jobs MUST explicitly set `ITenantContext.SetTenant()` at the start of each job execution. The `ITenantContext` design supports this because `SetTenant()` is public.

### E6: Concurrent requests
- **Scenario:** Two simultaneous requests for different tenants arrive.
- **Expected:** `TenantContext` is scoped (one instance per request). Each request has its own `TenantId`. No cross-contamination.

### E7: Slug uniqueness violation
- **Scenario:** Attempting to create a tenant with a `Slug` that already exists.
- **Expected:** Database throws unique constraint violation. The API layer (future specs) wraps this in a user-friendly error.

### E8: Seeder on production
- **Scenario:** `TenantSeeder` runs in Production environment.
- **Expected:** Seeder MUST check `IWebHostEnvironment.IsDevelopment()` and skip entirely if not development. Production tenant creation is a manual/admin operation.

### E9: Tenant deactivation
- **Scenario:** A tenant's `IsActive` is set to `false`.
- **Expected:** The global query filter does NOT filter by `IsActive`. Deactivation logic is a business concern handled by future specs (e.g., an auth check or middleware that rejects requests for inactive tenants). This spec only ensures data isolation, not active/inactive enforcement.

### E10: Entity without ITenantScoped mistakenly added
- **Scenario:** A developer adds a new entity that should be tenant-scoped but forgets to implement `ITenantScoped`.
- **Expected:** The entity will have no `TenantId` column and no global query filter → data leaks across tenants. Mitigation: a unit test that scans all entity types in the assembly and asserts every entity whose name is in a known list of tenant-scoped entities implements `ITenantScoped`. This test is added as a task.

## Dependencies

- Spec 10: Project Bootstrap & Infrastructure (solution structure, AppDbContext, Docker Compose, DI container)
- Spec 30: Authentication & Authorization (provides JWT with `tenant_id` claim — this spec only consumes it)
