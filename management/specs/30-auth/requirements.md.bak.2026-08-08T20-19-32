---
name: Authentication & Authorization
status: planned
references: V3, ADR-003
---

# Spec 30: Authentication & Authorization

## Value Delivery

Esta spec entrega **V3: Authentication & Authorization** do `management/vision.md`. Especificamente:

- **Login com email/senha e Google OAuth:** Usuários podem se autenticar com credenciais próprias ou conta Google.
- **ASP.NET Core Identity para gestão de credenciais:** Senhas hash (BCrypt), validação de força, emails únicos.
- **JWT access tokens (15min) + refresh tokens (7 dias) com rotação:** Tokens curtos em memória (SPA), refresh tokens em cookie HttpOnly com rotação anti-roubo.
- **Roles: Admin, Staff, Parent:** Cada role tem permissões específicas por tenant. Atributo `[Authorize(Roles = "...")]` funciona nos controllers.
- **Integração com multi-tenancy:** Token JWT inclui claim `tenant_id`, resolvido pelo `TenantMiddleware` da Spec 20.

## Functional Requirements

### FR-001: Registro de Usuário
- `POST /auth/register` recebe `{ email, password, name, tenantId }`, valida email único por tenant, aplica hash BCrypt na senha, cria `User` com role=Parent, retorna 201 com `AuthResponse` (access token + user DTO) e define refresh token como cookie HttpOnly.
- Senha deve ter no mínimo 8 caracteres, 1 maiúscula, 1 número, 1 caractere especial.
- Acceptance: `curl -X POST /auth/register -d '{...}'` retorna 201. `dotnet test` — teste de registro passa.

### FR-002: Login com Email/Senha
- `POST /auth/login` recebe `{ email, password }`, valida credenciais, retorna 200 com `AuthResponse` e cookie `refresh_token` HttpOnly.
- Credenciais inválidas retornam 401 `{ "error": "invalid_credentials" }`.
- Usuário inativo retorna 403 `{ "error": "account_inactive" }`.
- Acceptance: Login válido → 200 + tokens. Login inválido → 401. `dotnet test`.

### FR-003: Refresh Token
- `POST /auth/refresh` lê cookie `refresh_token`, valida existência no banco, verifica `IsActive` (não expirado, não revogado), gera novo access token + novo refresh token (rotação), revoga token antigo, define novo cookie.
- Cookie ausente ou token inválido retorna 401.
- Acceptance: Access token expirado → refresh → novo access token. Token revogado → 401.

### FR-004: Revogação de Token
- `POST /auth/revoke` (requer auth) lê cookie `refresh_token`, marca `RevokedAt = DateTime.UtcNow`, salva.
- Idempotente: token já revogado retorna 200.
- Reuso de token revogado detecta roubo e revoga TODOS os refresh tokens do usuário.
- Acceptance: Revoke → refresh com mesmo token → 401.

### FR-005: Google OAuth — Redirecionamento
- `GET /auth/google` redireciona para tela de consentimento Google.
- Escopo: email, profile. Redirect URI: `/auth/google/callback`.
- Acceptance: Navegador recebe redirect 302 para `https://accounts.google.com/o/oauth2/...`.

### FR-006: Google OAuth — Callback
- `GET /auth/google/callback` recebe code do Google, extrai email + name + googleId.
- Busca User por GoogleId; se não encontrado, busca por Email.
- Se email existe sem GoogleId: vincula. Se não existe: cria User (role=Parent) com senha aleatória.
- Redireciona para frontend com token no URL fragment.
- Google OAuth cancelado: redirect com `error=cancelled`.
- GoogleId já vinculado a outro: 409.
- Acceptance: Novo usuário Google → conta criada. Existente → vinculado. `dotnet test`.

### FR-007: Forgot Password
- `POST /auth/forgot-password` recebe `{ email }`, gera token de reset via Identity.
- Em dev: loga link no console. Em prod: placeholder para `IEmailSender`.
- Sempre retorna 200 (não vaza existência de email).
- Acceptance: POST → 200 OK sempre. Token gerado em dev.

### FR-008: Reset Password
- `POST /auth/reset-password` recebe `{ email, token, newPassword }`, valida token.
- Token inválido/expirado → 400 `{ "error": "invalid_token" }`.
- Senha igual à anterior → 400 `{ "error": "same_password" }`.
- Acceptance: Token válido → senha alterada → login funciona. `dotnet test`.

### FR-009: Entidade User
- `User` implementa `ITenantScoped`: Id (Guid), TenantId, Email (unique por tenant), Name (max 200), PasswordHash, Role (Admin=0, Staff=1, Parent=2), GoogleId (nullable, unique filtrado), IsActive (default true), CreatedAt (UTC).
- Índices: único composto (TenantId, Email), único filtrado GoogleId WHERE NOT NULL.
- Acceptance: Migration cria tabela Users com índices corretos.

### FR-010: Entidade RefreshToken
- Id (Guid), UserId (FK), Token (64 bytes random), ExpiresAt, CreatedAt, RevokedAt (nullable).
- Calculados: IsExpired, IsRevoked, IsActive.
- Acceptance: Migration cria tabela RefreshTokens com FK.

### FR-011: JWT Access Token Claims
- Claims: sub (User.Id), email, tenant_id, role, name. Exp: 15 min. Algoritmo: HMAC-SHA256.
- Chave secreta de appsettings (`Jwt:Secret`, mínimo 32 caracteres).
- Acceptance: Decodificar JWT mostra 5 claims + exp.

### FR-012: Authorize Attribute
- `[Authorize(Roles = "Admin")]` bloqueia Staff/Parent.
- `[Authorize(Roles = "Admin,Staff")]` permite Admin/Staff.
- `[Authorize]` permite qualquer autenticado.
- Acceptance: Role correto → 200, errado → 403, sem token → 401.

## Non-Functional Requirements

### NFR-001: Segurança
- Senhas BCrypt (fator 12). Nunca texto plano.
- Access token em memória (variável de módulo), nunca localStorage.
- Refresh token cookie HttpOnly, Secure (HTTPS), SameSite=Strict.
- Rotação: cada refresh gera novo token e revoga anterior. Reuso detecta roubo.

### NFR-002: Performance
- Login < 500ms. Refresh < 200ms.

### NFR-003: Auditabilidade
- Toda alteração de senha logada. Toda revogação de token logada.

## Constraints

- Depende de Spec 10 (Bootstrap) e Spec 20 (Multi-Tenant: Tenant entity, ITenantContext, TenantMiddleware).
- ASP.NET Core Identity + JWT Bearer + Google OAuth.
- NÃO implementa 2FA/MFA. NÃO implementa outros provedores OAuth (apenas Google).
- Seeding de admin users: feito pelo seeder desta spec, que depende do tenant default da Spec 20.

## Edge Cases & Error States

### E1: Email duplicado no mesmo tenant
- Registro com email existente → 409 `{ "error": "email_already_registered" }`.

### E2: GoogleId já vinculado a outro
- GoogleId pertence a outro User → 409 `{ "error": "google_account_linked_to_another_user" }`.

### E3: Email Google conflita com conta email/senha
- Vincula GoogleId à conta existente. Não cria duplicata.

### E4: Access token expirado → 401. Axios interceptor tenta refresh.

### E5: Refresh token expirado → `POST /auth/refresh` retorna 401. Redireciona para login.

### E6: Reuso de refresh token → revoga todas as sessões do usuário. 401. Log de segurança.

### E7: Forgot password para email inexistente → 200 (mesmo response) para evitar enumeração.

### E8: Reset com token expirado (padrão 1h) → 400 `{ "error": "invalid_token" }`.

### E9: TenantId inválido no registro → 400 `{ "error": "invalid_tenant" }`.

### E10: Google OAuth cancelado → redirect com `error=cancelled`.

## Dependencies

- Spec 10: Bootstrap & Infrastructure
- Spec 20: Multi-Tenant Architecture (Tenant, ITenantContext, TenantMiddleware)
