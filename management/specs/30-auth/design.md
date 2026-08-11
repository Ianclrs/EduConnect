# Spec 30: Design — Authentication & Authorization

## Design Approach

Autenticação baseada em **ASP.NET Core Identity** com entidade `User` customizada (implementando `ITenantScoped` para multi-tenancy). Autenticação stateless via **JWT Bearer tokens**: access token (15 min) em memória no SPA, refresh token (7 dias) em cookie HttpOnly com rotação. **Google OAuth** via middleware nativo do ASP.NET Core.

Fluxo: Usuário faz login → AuthService valida credenciais via UserManager → JwtTokenGenerator produz tokens → access token no body JSON, refresh token em cookie → SPA armazena access token em variável de módulo → requisições subsequentes incluem `Authorization: Bearer <token>` → TenantMiddleware extrai `tenant_id` do JWT → controllers autorizam por role.

## Architecture Decisions

- **AD-001: Identity User customizado** — `User` herda diretamente de `IdentityUser<Guid>` para ter compatibilidade total com UserManager, Roles, Claims do Identity.
- **AD-002: Refresh token em cookie HttpOnly** — previne XSS (JavaScript não acessa o cookie). Rotação previne CSRF + token theft.
- **AD-003: GoogleId nullable com índice filtrado** — permite email/password + Google no mesmo usuário. Índice único filtrado evita duplicatas sem bloquear nulos.

## Data Flow

```
Client                  API                         DB
  |                      |                           |
  |-- POST /auth/login ->|                           |
  |                      |-- Validate credentials -->|
  |                      |<-- User found ------------|
  |                      |-- Generate JWT ---------->|
  |                      |-- Generate refresh token->|
  |                      |-- Store refresh token --->|
  |<-- 200 + access_token|                           |
  |   + cookie refresh   |                           |
  |                      |                           |
  |-- GET /students ---->|                           |
  |  Authorization:      |                           |
  |  Bearer <access>     |-- Validate JWT ---------->|
  |                      |-- TenantMiddleware ------>|
  |                      |   extract tenant_id      |
  |                      |-- EF Core query filter -->|
  |                      |   WHERE TenantId = @ctx  |
  |<-- 200 + data -------|                           |
```

## Component / Module Breakdown

| Component | File | Responsibility |
|---|---|---|
| User | `Core/Entities/User.cs` | Identity entity com tenant scope |
| RefreshToken | `Core/Entities/RefreshToken.cs` | Token de renovação com tracking |
| AuthDtos | `Api/Contracts/AuthDtos.cs` | DTOs de request/response |
| IAuthService | `Infrastructure/Services/AuthService.cs` | Lógica de negócio de autenticação |
| IJwtTokenGenerator | `Infrastructure/Auth/JwtTokenGenerator.cs` | Geração e validação de JWT |
| AuthController | `Api/Controllers/AuthController.cs` | Endpoints REST + Google OAuth |

## Domain Entities

### User (Ciclo.Core/Entities/User.cs)
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

### RefreshToken (Ciclo.Core/Entities/RefreshToken.cs)
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

### AuthDtos (Ciclo.Api/Contracts/AuthDtos.cs)
```csharp
public record RegisterRequest(string Email, string Password, string Name, Guid TenantId);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string AccessToken, UserDto User);
public record UserDto(Guid Id, string Email, string Name, string Role);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);
public record RefreshRequest(); // body is empty, token comes from cookie
```

## AuthService (Ciclo.Infrastructure/Services/AuthService.cs)

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

## JwtTokenGenerator (Ciclo.Infrastructure/Auth/JwtTokenGenerator.cs)

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

## AuthController (Ciclo.Api/Controllers/AuthController.cs)

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

## Error Handling

| Error Condition | HTTP | Body |
|---|---|---|
| Invalid credentials | 401 | `{"error":"invalid_credentials"}` |
| Account inactive | 403 | `{"error":"account_inactive"}` |
| Email already registered | 409 | `{"error":"email_already_registered"}` |
| Invalid/expired token | 400 | `{"error":"invalid_token"}` |
| Same password | 400 | `{"error":"same_password"}` |
| Google account linked to another user | 409 | `{"error":"google_account_linked_to_another_user"}` |
| Invalid tenant | 400 | `{"error":"invalid_tenant"}` |

## File / Module Layout

| File | Path | Purpose |
|---|---|---|
| User entity | `src/Ciclo.Core/Entities/User.cs` | Identity entity, implements ITenantScoped |
| RefreshToken entity | `src/Ciclo.Core/Entities/RefreshToken.cs` | Token entity with expiry tracking |
| UserRole enum | `src/Ciclo.Core/Entities/UserRole.cs` | Admin=0, Staff=1, Parent=2 |
| Auth DTOs | `src/Ciclo.Infrastructure/Contracts/AuthDtos.cs` | Request/response records (moved from Api to avoid reverse dependency) |
| IAuthService + AuthService | `src/Ciclo.Infrastructure/Services/AuthService.cs` | Business logic |
| IJwtTokenGenerator + impl | `src/Ciclo.Infrastructure/Auth/JwtTokenGenerator.cs` | JWT creation |
| AuthController | `src/Ciclo.Api/Controllers/AuthController.cs` | REST endpoints + Google OAuth |

## Cross-Reference: Requirements → Design

| Requirement | Covered By |
|---|---|
| FR-001: Register | AuthService, AuthController, DTOs |
| FR-002: Login | AuthService, AuthController, JwtTokenGenerator |
| FR-003: Refresh | AuthService.RefreshTokenAsync, Cookie Config |
| FR-004: Revoke | AuthService.RevokeTokenAsync |
| FR-005/006: Google OAuth | Google OAuth Flow, AuthController |
| FR-007/008: Password Reset | AuthService, Identity UserManager |
| FR-009/010: Entities | User, RefreshToken domain entities |
| FR-011: JWT Claims | JwtTokenGenerator |
| FR-012: Authorize | ASP.NET Core Identity Setup |
| NFR-001-003 | Cookie Config, BCrypt in AuthService |
| E1-E10 | Error Handling table |
