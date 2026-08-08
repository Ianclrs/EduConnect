using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using EduGestor.Core.Entities;
using EduGestor.Core.Interfaces;
using EduGestor.Infrastructure.Tenancy;

namespace EduGestor.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

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

        // User entity configuration (IdentityUser<Guid> customizations)
        builder.Entity<User>(entity =>
        {
            // Rename Identity default table from AspNetUsers to Users
            entity.ToTable("Users");

            // Custom properties
            entity.Property(u => u.Name).HasMaxLength(200).IsRequired();
            entity.Property(u => u.Role).HasConversion<int>().IsRequired();
            entity.Property(u => u.GoogleId).HasMaxLength(256);
            entity.Property(u => u.IsActive).HasDefaultValue(true);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("now()");

            // Tenant relationship
            entity.HasOne(u => u.Tenant)
                .WithMany()
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique composite index (TenantId, Email)
            entity.HasIndex(u => new { u.TenantId, u.Email }).IsUnique();

            // Unique filtered index on GoogleId (where not null)
            entity.HasIndex(u => u.GoogleId)
                .IsUnique()
                .HasFilter("\"GoogleId\" IS NOT NULL");
        });

        // RefreshToken entity configuration
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.Token).HasMaxLength(128).IsRequired();
            entity.Property(rt => rt.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Rename Identity role table
        builder.Entity<IdentityRole<Guid>>(entity =>
        {
            entity.ToTable("Roles");
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
                var property = entry.Entity.GetType().GetProperty(nameof(ITenantScoped.TenantId));
                property?.SetValue(entry.Entity, _tenantContext.TenantId);
            }
        }
    }
}
