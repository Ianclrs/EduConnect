using Microsoft.AspNetCore.Identity;
using Ciclo.Core.Interfaces;

namespace Ciclo.Core.Entities;

public class User : IdentityUser<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Parent;

    public string? GoogleId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
