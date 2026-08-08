using Microsoft.EntityFrameworkCore;
using EduGestor.Core.Interfaces;
using EduGestor.Infrastructure.Data;
using EduGestor.Infrastructure.Tenancy;
using Moq;

namespace EduGestor.Api.Tests;

public class AutoSetTenantIdTests
{
    private sealed class TestEntity : ITenantScoped
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; } = Guid.Empty;
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestDbContext : AppDbContext
    {
        public TestDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
            : base(options, tenantContext) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(200);
            });
        }
    }

    private static (TestDbContext db, Mock<ITenantContext> tenantContextMock) CreateDbContext(Guid? tenantId = null)
    {
        var tenantContextMock = new Mock<ITenantContext>();
        tenantContextMock.Setup(tc => tc.IsResolved).Returns(tenantId.HasValue);
        if (tenantId.HasValue)
            tenantContextMock.Setup(tc => tc.TenantId).Returns(tenantId.Value);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new TestDbContext(options, tenantContextMock.Object);
        return (db, tenantContextMock);
    }

    [Fact]
    public async Task SaveChangesAsync_UnresolvedTenant_DoesNotSetTenantId()
    {
        var (db, _) = CreateDbContext();
        var entity = new TestEntity { Name = "Test", TenantId = Guid.Empty };

        db.Set<TestEntity>().Add(entity);
        await db.SaveChangesAsync();

        Assert.Equal(Guid.Empty, entity.TenantId);
    }

    [Fact]
    public async Task SaveChangesAsync_ResolvedTenant_SetsTenantId()
    {
        var tenantId = Guid.NewGuid();
        var (db, _) = CreateDbContext(tenantId);
        var entity = new TestEntity { Name = "Test", TenantId = Guid.Empty };

        db.Set<TestEntity>().Add(entity);
        await db.SaveChangesAsync();

        Assert.Equal(tenantId, entity.TenantId);
    }

    [Fact]
    public async Task SaveChangesAsync_ExplicitTenantId_DoesNotOverride()
    {
        var tenantId = Guid.NewGuid();
        var explicitTenantId = Guid.NewGuid();
        var (db, _) = CreateDbContext(tenantId);
        var entity = new TestEntity { Name = "Test", TenantId = explicitTenantId };

        db.Set<TestEntity>().Add(entity);
        await db.SaveChangesAsync();

        Assert.Equal(explicitTenantId, entity.TenantId);
    }
}
