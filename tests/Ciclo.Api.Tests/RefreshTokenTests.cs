using Ciclo.Core.Entities;

namespace Ciclo.Api.Tests;

public class RefreshTokenTests
{
    [Fact]
    public void IsExpired_WhenExpiryInFuture_ReturnsFalse()
    {
        var token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        Assert.False(token.IsExpired);
    }

    [Fact]
    public void IsExpired_WhenExpiryInPast_ReturnsTrue()
    {
        var token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow.AddHours(-1)
        };

        Assert.True(token.IsExpired);
    }

    [Fact]
    public void IsRevoked_WhenRevokedAtIsNull_ReturnsFalse()
    {
        var token = new RefreshToken
        {
            RevokedAt = null
        };

        Assert.False(token.IsRevoked);
    }

    [Fact]
    public void IsRevoked_WhenRevokedAtIsSet_ReturnsTrue()
    {
        var token = new RefreshToken
        {
            RevokedAt = DateTime.UtcNow
        };

        Assert.True(token.IsRevoked);
    }

    [Fact]
    public void IsActive_WhenNotExpiredAndNotRevoked_ReturnsTrue()
    {
        var token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            RevokedAt = null
        };

        Assert.True(token.IsActive);
    }

    [Fact]
    public void IsActive_WhenExpired_ReturnsFalse()
    {
        var token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            RevokedAt = null
        };

        Assert.False(token.IsActive);
    }

    [Fact]
    public void IsActive_WhenRevoked_ReturnsFalse()
    {
        var token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            RevokedAt = DateTime.UtcNow
        };

        Assert.False(token.IsActive);
    }

    [Fact]
    public void IsActive_WhenExpiredAndRevoked_ReturnsFalse()
    {
        var token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            RevokedAt = DateTime.UtcNow
        };

        Assert.False(token.IsActive);
    }
}
