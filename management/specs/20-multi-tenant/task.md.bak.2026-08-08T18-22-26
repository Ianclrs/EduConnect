# Spec 20: Tasks — Multi-Tenant Architecture

## Task Checklist

### T2.1: Create Tenant entity
- [ ] Create `src/EduGestor.Core/Entities/Tenant.cs`
- [ ] Properties: Id (Guid), Name (string), Slug (string), IsActive (bool), CreatedAt (DateTime)

### T2.2: Create ITenantScoped interface
- [ ] Create `src/EduGestor.Core/Interfaces/ITenantScoped.cs`
- [ ] Single property: `Guid TenantId { get; }`

### T2.3: Create ITenantContext and TenantContext
- [ ] Create `src/EduGestor.Infrastructure/Tenancy/TenantContext.cs`
- [ ] Interface `ITenantContext` with `TenantId` and `IsResolved`
- [ ] Implementation `TenantContext` with `SetTenant(Guid)` method

### T2.4: Create TenantNotResolvedException
- [ ] Create `src/EduGestor.Infrastructure/Tenancy/TenantNotResolvedException.cs`
- [ ] Inherits `InvalidOperationException`

### T2.5: Create TenantMiddleware
- [ ] Create `src/EduGestor.Api/Middleware/TenantMiddleware.cs`
- [ ] Extract `tenant_id` claim from authenticated user
- [ ] Call `ITenantContext.SetTenant(tenantId)`
- [ ] Return 401 if no tenant claim on authenticated endpoint

### T2.6: Update AppDbContext
- [ ] Add `TenantContext _tenantContext` dependency
- [ ] Add `DbSet<Tenant> Tenants`
- [ ] Configure `Tenant` entity: unique index on `Slug`
- [ ] Apply global query filter for all `ITenantScoped` entities
- [ ] Override `SaveChangesAsync` to auto-set `TenantId` on new entities

### T2.7: Register services in DI
- [ ] Register `TenantContext` as scoped
- [ ] Register `ITenantContext` using factory from `TenantContext`
- [ ] Add `UseMiddleware<TenantMiddleware>()` after `UseAuthentication()`

### T2.8: Create TenantSeeder
- [ ] Create `src/EduGestor.Infrastructure/Data/Seeders/TenantSeeder.cs`
- [ ] If no tenants exist, create default: "Default School" / "default"
- [ ] Seed admin user: admin@default.local (only if Spec 30 is done)

### T2.9: Verify
- [ ] `dotnet build` — zero errors
- [ ] EF migration generates `Tenants` table with unique slug index
- [ ] Manual test: endpoint with auth rejects request without tenant claim
