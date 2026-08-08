# Spec 20: Design — Multi-Tenant Architecture

## Domain Entities

### Tenant (EduGestor.Core/Entities/Tenant.cs)
```csharp
public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;  // unique, URL-safe identifier
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### ITenantScoped (EduGestor.Core/Interfaces/ITenantScoped.cs)
```csharp
public interface ITenantScoped
{
    Guid TenantId { get; }
}
```

All entities that belong to a tenant (Student, Enrollment, Document, Notification, etc.) MUST implement this interface.

## Infrastructure

### TenantContext (EduGestor.Infrastructure/Tenancy/TenantContext.cs)
```csharp
public interface ITenantContext
{
    Guid TenantId { get; }
    bool IsResolved { get; }
}

public class TenantContext : ITenantContext
{
    private Guid? _tenantId;
    public Guid TenantId => _tenantId ?? throw new TenantNotResolvedException();
    public bool IsResolved => _tenantId.HasValue;
    public void SetTenant(Guid tenantId) => _tenantId = tenantId;
}
```

Registered as **Scoped** in DI.

### TenantNotResolvedException
```csharp
public class TenantNotResolvedException : InvalidOperationException
{
    public TenantNotResolvedException()
        : base("No tenant context resolved for the current request.") { }
}
```

### TenantMiddleware
```csharp
// Order: runs AFTER Authentication middleware
// Extracts TenantId from HttpContext.User.FindFirst("tenant_id")
// Calls ITenantContext.SetTenant(tenantId)
// Returns 401 if no tenant claim and endpoint requires auth
```

### AppDbContext Changes
```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
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
```

## Data Seeding

### TenantSeeder
- Runs on app startup in Development mode.
- Checks if any Tenant exists; if not, creates a default tenant (e.g., "Default School", slug: "default").
- Seeds an Admin user for that tenant (admin@default.local / Admin123!).

## File Locations

| File | Path |
|---|---|
| Tenant entity | `src/EduGestor.Core/Entities/Tenant.cs` |
| ITenantScoped | `src/EduGestor.Core/Interfaces/ITenantScoped.cs` |
| ITenantContext + TenantContext | `src/EduGestor.Infrastructure/Tenancy/TenantContext.cs` |
| TenantNotResolvedException | `src/EduGestor.Infrastructure/Tenancy/TenantNotResolvedException.cs` |
| TenantMiddleware | `src/EduGestor.Api/Middleware/TenantMiddleware.cs` |
| TenantSeeder | `src/EduGestor.Infrastructure/Data/Seeders/TenantSeeder.cs` |
| AppDbContext (updated) | `src/EduGestor.Infrastructure/Data/AppDbContext.cs` |

## Registration in Program.cs

```csharp
builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
// ...
app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();  // after auth, before authorization
app.UseAuthorization();
```
