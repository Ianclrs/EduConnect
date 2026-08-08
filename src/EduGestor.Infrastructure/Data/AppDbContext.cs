using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using EduGestor.Core.Entities;
using EduGestor.Core.Interfaces;
using EduGestor.Infrastructure.Tenancy;

namespace EduGestor.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tenant entity configuration
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.Slug).IsUnique();
            entity.Property(t => t.Name).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Slug).HasMaxLength(100).IsRequired();
            entity.Property(t => t.IsActive).HasDefaultValue(true);
            entity.Property(t => t.CreatedAt).HasDefaultValueSql("now()");
        });

        // Apply global query filter to all ITenantScoped entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var tenantIdProperty = Expression.Property(parameter, nameof(ITenantScoped.TenantId));
                var currentTenantId = Expression.Property(
                    Expression.Constant(_tenantContext), nameof(ITenantContext.TenantId));
                var filter = Expression.Lambda(
                    Expression.Equal(tenantIdProperty, currentTenantId), parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
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
                var property = entry.Entity.GetType().GetProperty(nameof(ITenantScoped.TenantId));
                property?.SetValue(entry.Entity, _tenantContext.TenantId);
            }
        }
    }
}
