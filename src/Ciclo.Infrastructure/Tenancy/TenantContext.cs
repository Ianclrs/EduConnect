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
