# Spec 3: Tasks — Authentication & Authorization

## Task Checklist

### T3.1: Add Identity packages
- [ ] Add `Microsoft.AspNetCore.Identity.EntityFrameworkCore` to Infrastructure
- [ ] Add `Microsoft.AspNetCore.Authentication.JwtBearer` to Api
- [ ] Add `Microsoft.AspNetCore.Authentication.Google` to Api

### T3.2: Create User entity
- [ ] Create `src/EduGestor.Core/Entities/User.cs` implementing `ITenantScoped`
- [ ] Create `src/EduGestor.Core/Entities/UserRole.cs` enum (Admin, Staff, Parent)
- [ ] Properties: Id, TenantId, Email, Name, PasswordHash, Role, GoogleId?, IsActive, CreatedAt
- [ ] Navigation: Tenant, RefreshTokens collection

### T3.3: Create RefreshToken entity
- [ ] Create `src/EduGestor.Core/Entities/RefreshToken.cs`
- [ ] Properties: Id, UserId, Token, ExpiresAt, CreatedAt, RevokedAt?
- [ ] Helper: IsExpired, IsRevoked, IsActive
- [ ] Navigation: User

### T3.4: Update AppDbContext for Identity
- [ ] Add `DbSet<User> Users`
- [ ] Add `DbSet<RefreshToken> RefreshTokens`
- [ ] Configure User: unique index on Email+TenantId, unique index on GoogleId
- [ ] Configure RefreshToken: FK to User

### T3.5: Create Auth DTOs
- [ ] Create `src/EduGestor.Api/Contracts/AuthDtos.cs`
- [ ] RegisterRequest, LoginRequest, AuthResponse, UserDto, ForgotPasswordRequest, ResetPasswordRequest

### T3.6: Create JwtTokenGenerator
- [ ] Create `src/EduGestor.Infrastructure/Auth/JwtTokenGenerator.cs`
- [ ] GenerateAccessToken: claims sub, email, tenant_id, role, name; 15min expiry
- [ ] GenerateRefreshToken: 64-byte random, 7-day expiry

### T3.7: Create AuthService
- [ ] Create `src/EduGestor.Infrastructure/Services/AuthService.cs`
- [ ] Register: hash password with BCrypt, create user, return tokens
- [ ] Login: verify password, return tokens, set refresh cookie
- [ ] RefreshToken: validate, rotate, return new tokens
- [ ] RevokeToken: mark as revoked
- [ ] ForgotPassword: generate reset token, log to console (dev)
- [ ] ResetPassword: validate token, update password

### T3.8: Create AuthController
- [ ] Create `src/EduGestor.Api/Controllers/AuthController.cs`
- [ ] POST /auth/register, POST /auth/login, POST /auth/refresh, POST /auth/revoke
- [ ] POST /auth/forgot-password, POST /auth/reset-password

### T3.9: Add Google OAuth
- [ ] Configure Google authentication in Program.cs
- [ ] GET /auth/google → Challenge()
- [ ] GET /auth/google/callback → handle response, create/find user, return tokens

### T3.10: Configure JWT and Identity in Program.cs
- [ ] Add Identity with User entity
- [ ] Add JWT Bearer auth with token validation params (issuer, audience, lifetime, signing key)
- [ ] Configure cookie policy for refresh token

### T3.11: Wire TenantMiddleware to Auth
- [ ] Update `TenantMiddleware` to extract `tenant_id` from JWT claims
- [ ] Return 401 for authenticated requests without tenant claim

### T3.12: Add Authorize attributes
- [ ] Verify `[Authorize(Roles = "Admin")]` works on a test endpoint
- [ ] Verify `[Authorize(Roles = "Admin,Staff")]` works

### T3.13: Verify
- [ ] `dotnet build` — zero errors
- [ ] Register → Login → access protected endpoint → 200 OK
- [ ] Google OAuth redirect flow works (test with Google dev console)
- [ ] Refresh token flow works: expired access token → refresh → new access token
- [ ] Revoke → refresh with revoked token → 401
