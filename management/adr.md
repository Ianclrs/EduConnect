# Architecture Decision Records (ADR)

This file records architecture decisions that affect the project. Each ADR describes a decision, its context, the rationale behind it, and its consequences. ADRs are numbered sequentially (ADR-001, ADR-002, ...) and never renumbered after creation.

---

## ADR-001: .NET 10 + EF Core + PostgreSQL Stack

**Date:** 2026-08-07
**Status:** Accepted

**Context:** We need to choose the backend stack for a multi-tenant school management SaaS. Requirements: REST API, relational data model, JWT auth, file storage, background jobs.

**Decision:** Use **C# .NET 10 (ASP.NET Core Web API)** + **Entity Framework Core 10** + **PostgreSQL 16**.

**Rationale:**
- .NET 10 is the latest LTS-targeted release with the best performance profile for APIs.
- EF Core 10 provides a mature ORM with global query filters (essential for multi-tenancy).
- PostgreSQL 16 is robust, free, and has first-class EF Core provider via Npgsql.
- C# strong typing ensures compile-time safety — ideal for AI-driven development.

**Consequences:**
- EF Core global query filters enable clean tenant isolation without schema-per-tenant complexity.
- PostgreSQL allows JSONB columns for flexible data (performance records, custom fields).
- Requires Npgsql.EntityFrameworkCore.PostgreSQL package.

---

## ADR-002: Shared Database with TenantId Column

**Date:** 2026-08-07
**Status:** Accepted

**Context:** We need to isolate data between multiple schools (tenants). Options: database-per-tenant, schema-per-tenant, or shared database with discriminator column.

**Decision:** Use a **shared database with a `TenantId` discriminator column** on all tenant-scoped tables, enforced by EF Core global query filters and a tenant-aware middleware.

**Rationale:**
- Single database to manage and backup — operational simplicity.
- EF Core global query filters automatically append `WHERE TenantId = @currentTenantId` to every query.
- Tenant middleware resolves `TenantId` from the JWT claim on every request.
- Avoids the complexity of per-tenant schema migrations.

**Consequences:**
- Every tenant-scoped entity must include `TenantId`.
- Tenant context must flow from HTTP request through the entire pipeline.
- Migration discipline required: never forget to include TenantId in new entities.

---

## ADR-003: JWT Access + Refresh Tokens

**Date:** 2026-08-07
**Status:** Accepted

**Context:** The system needs stateless authentication for the SPA/PWA with support for Google OAuth.

**Decision:** Use **JWT Bearer tokens** (access: 15 min, refresh: 7 days) + **ASP.NET Core Identity** for credential management. Google OAuth via OpenID Connect middleware.

**Rationale:**
- Access tokens are short-lived (15 min) and stored in memory (SPA) — never in localStorage.
- Refresh tokens are stored in an HttpOnly cookie and in the database (revocable).
- Google OAuth uses the standard ASP.NET Core `Microsoft.AspNetCore.Authentication.Google` package.
- Stateless API — no server-side session store needed.

**Consequences:**
- Refresh token rotation prevents long-term token theft.
- Google OAuth onboarding is frictionless for parents.
- SPA must handle token refresh transparently via Axios interceptors.
