# Spec 3: Design — Authentication & Authorization

## Domain Entities

### User (EduGestor.Core/Entities/User.cs)
```csharp
public class User : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Parent;
    public string? GoogleId { get; set; }       // null if email/password only
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}

public enum UserRole
{
    Admin = 0,
    Staff = 1,
    Parent = 2
}
```

### RefreshToken (EduGestor.Core/Entities/RefreshToken.cs)
```csharp
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    public User User { get; set; } = null!;
}
```

## DTOs

### AuthDtos (EduGestor.Api/Contracts/AuthDtos.cs)
```csharp
public record RegisterRequest(string Email, string Password, string Name, Guid TenantId);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string AccessToken, UserDto User);
public record UserDto(Guid Id, string Email, string Name, string Role);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);
public record RefreshRequest(); // body is empty, token comes from cookie
```

## AuthService (EduGestor.Infrastructure/Services/AuthService.cs)

```csharp
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request, HttpResponse response);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, HttpResponse response);
    Task RevokeTokenAsync(string refreshToken);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(ResetPasswordRequest request);
}

public class AuthService : IAuthService
{
    // Uses UserManager<User> from ASP.NET Core Identity
    // Uses IJwtTokenGenerator for token creation
}
```

## JwtTokenGenerator (EduGestor.Infrastructure/Auth/JwtTokenGenerator.cs)

```csharp
public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(Guid userId);
}
```
Access token claims: `sub` (User.Id), `email`, `tenant_id`, `role`, `name`.
Access token expiry: 15 minutes.
Refresh token: 64-byte random string, 7-day expiry, stored in DB.

## AuthController (EduGestor.Api/Controllers/AuthController.cs)

| Endpoint | Method | Auth Required |
|---|---|---|
| `/auth/register` | POST | No |
| `/auth/login` | POST | No |
| `/auth/refresh` | POST | No (uses cookie) |
| `/auth/revoke` | POST | Yes |
| `/auth/google` | GET | No (redirect) |
| `/auth/google/callback` | GET | No (callback) |
| `/auth/forgot-password` | POST | No |
| `/auth/reset-password` | POST | No |

## Google OAuth Flow

1. `GET /auth/google` → Challenge with Google scheme → redirect to Google
2. `GET /auth/google/callback` → Google redirects back → handler:
   - Extract Google user info (email, name, googleId)
   - Find existing user by GoogleId or email
   - If not found: create user (role=Parent, requires tenant selection or default)
   - Generate tokens, set refresh cookie, redirect to frontend with access token in URL fragment

## Cookie Configuration

```csharp
var cookieOptions = new CookieOptions
{
    HttpOnly = true,
    Secure = true,           // HTTPS only in production
    SameSite = SameSiteMode.Strict,
    Expires = refreshToken.ExpiresAt
};
response.Cookies.Append("refresh_token", refreshToken.Token, cookieOptions);
```

## ASP.NET Core Identity Setup

- Use `User` entity directly (no separate IdentityUser).
- Configure Identity options: require unique email, minimum password length 8.
- Add JWT Bearer authentication with token validation parameters.

## NuGet Packages to Add

```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.*" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.*" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.Google" Version="10.*" />
```

## File Locations

| File | Path |
|---|---|
| User entity | `src/EduGestor.Core/Entities/User.cs` |
| RefreshToken entity | `src/EduGestor.Core/Entities/RefreshToken.cs` |
| UserRole enum | `src/EduGestor.Core/Entities/UserRole.cs` |
| Auth DTOs | `src/EduGestor.Api/Contracts/AuthDtos.cs` |
| IAuthService + AuthService | `src/EduGestor.Infrastructure/Services/AuthService.cs` |
| IJwtTokenGenerator + impl | `src/EduGestor.Infrastructure/Auth/JwtTokenGenerator.cs` |
| AuthController | `src/EduGestor.Api/Controllers/AuthController.cs` |
| Google OAuth handler | `src/EduGestor.Api/Controllers/AuthController.cs` (same file) |
