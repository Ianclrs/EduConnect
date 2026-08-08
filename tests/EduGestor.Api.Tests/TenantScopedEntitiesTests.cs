using System.Reflection;
using EduGestor.Core.Interfaces;

namespace EduGestor.Api.Tests;

public class TenantScopedEntitiesTests
{
    /// <summary>
    /// Scans EduGestor.Core.dll for all types that are non-abstract classes
    /// with a public TenantId property of type Guid. For each such type, asserts
    /// that it implements ITenantScoped. This prevents future developers/agents
    /// from adding tenant-scoped entities without the interface, which would
    /// skip the global query filter and cause cross-tenant data leaks.
    /// </summary>
    [Fact]
    public void AllTenantScopedEntities_ImplementITenantScoped()
    {
        // Resolve assembly by locating a known type from EduGestor.Core
        var coreAssembly = typeof(EduGestor.Core.Entities.Tenant).Assembly;

        var tenantScopedTypes = coreAssembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsInterface: false })
            .Where(t => t.GetProperty(nameof(ITenantScoped.TenantId),
                BindingFlags.Public | BindingFlags.Instance) != null
                && t.GetProperty(nameof(ITenantScoped.TenantId))!.PropertyType == typeof(Guid))
            .ToList();

        foreach (var type in tenantScopedTypes)
        {
            Assert.True(typeof(ITenantScoped).IsAssignableFrom(type),
                $"Type '{type.FullName}' has a public TenantId property of type Guid " +
                "but does not implement ITenantScoped. All tenant-scoped entities " +
                "MUST implement ITenantScoped to ensure EF Core global query filters " +
                "are applied and cross-tenant data leaks are prevented.");
        }
    }
}
