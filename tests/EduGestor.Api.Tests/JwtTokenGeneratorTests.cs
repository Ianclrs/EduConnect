using System.IdentityModel.Tokens.Jwt;
using EduGestor.Core.Entities;
using EduGestor.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EduGestor.Api.Tests;

public class JwtTokenGeneratorTests
{
    private static IConfiguration CreateConfiguration(string secret = "test-secret-key-that-is-at-least-32-characters-long!!")
    {
        var config = new Mock<IConfiguration>();
        config.Setup(c => c["Jwt:Secret"]).Returns(secret);
        config.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        config.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        return config.Object;
    }

    private static User CreateTestUser(
        Guid? id = null, string email = "test@example.com", string name = "Test User",
        Guid? tenantId = null, UserRole role = UserRole.Parent)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Email = email,
            Name = name,
            TenantId = tenantId ?? Guid.NewGuid(),
            Role = role
        };
    }

    [Fact]
    public void GenerateAccessToken_ReturnsNonEmptyString()
    {
        var generator = new JwtTokenGenerator(CreateConfiguration());
        var user = CreateTestUser();

        var token = generator.GenerateAccessToken(user);

        Assert.False(string.IsNullOrEmpty(token));
    }

    [Fact]
    public void GenerateAccessToken_ProducesValidJwt()
    {
        var generator = new JwtTokenGenerator(CreateConfiguration());
        var user = CreateTestUser();

        var token = generator.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.NotNull(jwt);
        Assert.Equal("TestIssuer", jwt.Issuer);
        Assert.Contains(jwt.Audiences, a => a == "TestAudience");
    }

    [Fact]
    public void GenerateAccessToken_ContainsAllRequiredClaims()
    {
        var generator = new JwtTokenGenerator(CreateConfiguration());
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, email: "user@school.com", name: "John Doe",
            tenantId: tenantId, role: UserRole.Admin);

        var token = generator.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal(userId.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("user@school.com", jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(tenantId.ToString(), jwt.Claims.First(c => c.Type == "tenant_id").Value);
        Assert.Equal("Admin", jwt.Claims.First(c => c.Type == "role").Value);
        Assert.Equal("John Doe", jwt.Claims.First(c => c.Type == "name").Value);
    }

    [Fact]
    public void GenerateAccessToken_HasCorrectExpiry()
    {
        var generator = new JwtTokenGenerator(CreateConfiguration());
        var user = CreateTestUser();

        var token = generator.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var expectedExpiry = DateTime.UtcNow.AddMinutes(15);
        Assert.True(jwt.ValidTo > DateTime.UtcNow.AddMinutes(14));
        Assert.True(jwt.ValidTo <= expectedExpiry.AddSeconds(5));
    }

    [Fact]
    public void GenerateAccessToken_DifferentRoles_ProducesCorrectClaim()
    {
        var generator = new JwtTokenGenerator(CreateConfiguration());

        var admin = CreateTestUser(role: UserRole.Admin);
        var staff = CreateTestUser(role: UserRole.Staff);
        var parent = CreateTestUser(role: UserRole.Parent);

        var handler = new JwtSecurityTokenHandler();

        var adminJwt = handler.ReadJwtToken(generator.GenerateAccessToken(admin));
        var staffJwt = handler.ReadJwtToken(generator.GenerateAccessToken(staff));
        var parentJwt = handler.ReadJwtToken(generator.GenerateAccessToken(parent));

        Assert.Equal("Admin", adminJwt.Claims.First(c => c.Type == "role").Value);
        Assert.Equal("Staff", staffJwt.Claims.First(c => c.Type == "role").Value);
        Assert.Equal("Parent", parentJwt.Claims.First(c => c.Type == "role").Value);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsValidToken()
    {
        var generator = new JwtTokenGenerator(CreateConfiguration());
        var userId = Guid.NewGuid();

        var refreshToken = generator.GenerateRefreshToken(userId);

        Assert.Equal(userId, refreshToken.UserId);
        Assert.False(string.IsNullOrEmpty(refreshToken.Token));
        Assert.True(refreshToken.ExpiresAt > DateTime.UtcNow.AddDays(6));
        Assert.True(refreshToken.ExpiresAt <= DateTime.UtcNow.AddDays(7).AddSeconds(5));
        Assert.True(refreshToken.IsActive);
        Assert.False(refreshToken.IsExpired);
        Assert.False(refreshToken.IsRevoked);
    }

    [Fact]
    public void GenerateRefreshToken_GeneratesUniqueTokens()
    {
        var generator = new JwtTokenGenerator(CreateConfiguration());
        var userId = Guid.NewGuid();

        var token1 = generator.GenerateRefreshToken(userId);
        var token2 = generator.GenerateRefreshToken(userId);

        Assert.NotEqual(token1.Token, token2.Token);
        Assert.NotEqual(token1.Id, token2.Id);
    }

    [Fact]
    public void GenerateRefreshToken_TokenLengthIsAtLeast64Bytes()
    {
        var generator = new JwtTokenGenerator(CreateConfiguration());

        var refreshToken = generator.GenerateRefreshToken(Guid.NewGuid());

        // 64 bytes Base64-encoded = 88 characters (without padding)
        Assert.True(refreshToken.Token.Length >= 86);
    }
}
