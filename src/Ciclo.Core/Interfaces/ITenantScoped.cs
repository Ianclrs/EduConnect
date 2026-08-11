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
