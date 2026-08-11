using Ciclo.Infrastructure.Tenancy;

namespace Ciclo.Api.Tests;

public class TenantContextTests
{
    [Fact]
    public void SetTenant_ValidGuid_SetsTenantId()
    {
        var context = new TenantContext();
        var tenantId = Guid.NewGuid();

        context.SetTenant(tenantId);

        Assert.True(context.IsResolved);
        Assert.Equal(tenantId, context.TenantId);
    }

    [Fact]
    public void TenantId_NotSet_ThrowsTenantNotResolvedException()
    {
        var context = new TenantContext();

        Assert.False(context.IsResolved);
        Assert.Throws<TenantNotResolvedException>(() => context.TenantId);
    }
}
