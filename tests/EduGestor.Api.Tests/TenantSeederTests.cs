using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EduGestor.Infrastructure.Data;
using EduGestor.Infrastructure.Data.Seeders;
using EduGestor.Infrastructure.Tenancy;
using Moq;

namespace EduGestor.Api.Tests;

public class TenantSeederTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var tenantContext = new Mock<ITenantContext>().Object;
        return new AppDbContext(options, tenantContext);
    }

    private static IHostEnvironment CreateDevelopmentEnv()
    {
        var mock = new Mock<IHostEnvironment>();
        mock.Setup(e => e.EnvironmentName).Returns("Development");
        return mock.Object;
    }

    private static IHostEnvironment CreateProductionEnv()
    {
        var mock = new Mock<IHostEnvironment>();
        mock.Setup(e => e.EnvironmentName).Returns("Production");
        return mock.Object;
    }

    [Fact]
    public async Task SeedAsync_NoTenants_CreatesDefaultTenant()
    {
        using var db = CreateDbContext();
        var logger = Mock.Of<ILogger>();

        await TenantSeeder.SeedAsync(db, CreateDevelopmentEnv(), logger);

        var tenant = await db.Tenants.SingleOrDefaultAsync();
        Assert.NotNull(tenant);
        Assert.Equal("default", tenant!.Slug);
        Assert.Equal("Default School", tenant.Name);
    }

    [Fact]
    public async Task SeedAsync_TenantsExist_DoesNotDuplicate()
    {
        using var db = CreateDbContext();
        var logger = Mock.Of<ILogger>();

        // First seed
        await TenantSeeder.SeedAsync(db, CreateDevelopmentEnv(), logger);
        var countAfterFirst = await db.Tenants.CountAsync();

        // Second seed
        await TenantSeeder.SeedAsync(db, CreateDevelopmentEnv(), logger);
        var countAfterSecond = await db.Tenants.CountAsync();

        Assert.Equal(1, countAfterFirst);
        Assert.Equal(1, countAfterSecond);
    }

    [Fact]
    public async Task SeedAsync_Production_Skips()
    {
        using var db = CreateDbContext();
        var logger = Mock.Of<ILogger>();

        await TenantSeeder.SeedAsync(db, CreateProductionEnv(), logger);

        var count = await db.Tenants.CountAsync();
        Assert.Equal(0, count);
    }
}
