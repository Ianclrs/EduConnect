---
name: Authentication & Authorization
status: planned
references: V3, ADR-003
---

# Spec 30: Authentication & Authorization

## O Que Esta Spec Entrega

Sistema completo de autenticação com login por email/senha e Google OAuth. Tokens JWT de acesso + refresh. Acesso baseado em papéis (Admin, Staff, Parent) com escopo vinculado ao tenant do usuário. Registro de usuário, login, renovação de token, redefinição de senha e fluxo de login com Google.

## Critérios de Aceite

1. **AC1:** `POST /auth/register` cria um novo usuário com senha hash, retorna 201.
2. **AC2:** `POST /auth/login` com email+senha válidos retorna token JWT de acesso + define refresh token em cookie HttpOnly.
3. **AC3:** `POST /auth/refresh` retorna novo token de acesso quando um cookie de refresh token válido está presente.
4. **AC4:** `POST /auth/revoke` revoga o refresh token atual.
5. **AC5:** `GET /auth/google` redireciona para a tela de consentimento do Google OAuth.
6. **AC6:** `GET /auth/google/callback` completa o fluxo OAuth, cria ou vincula usuário, retorna tokens.
7. **AC7:** `POST /auth/forgot-password` envia email de redefinição de senha (loga no console em dev).
8. **AC8:** `POST /auth/reset-password` com token válido redefine a senha.
9. **AC9:** Entidade User inclui `TenantId`, `Role` (Admin|Staff|Parent) e `GoogleId` opcional.
10. **AC10:** Atributo `[Authorize(Roles = "Admin")]` funciona corretamente nos controllers.
11. **AC11:** Token de acesso contém as claims: `sub`, `email`, `tenant_id`, `role`, `name`.
12. **AC12:** `TenantMiddleware` extrai a claim `tenant_id` e define `ITenantContext`.

## Dependências

- Spec 10: Bootstrap & Infrastructure
- Spec 20: Multi-Tenant (precisa da entidade Tenant e ITenantContext)
