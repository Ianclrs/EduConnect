---
name: Multi-Tenant Architecture
status: planned
references: V2, ADR-002
---

# Spec 20: Multi-Tenant Architecture

## O Que Esta Spec Entrega

Infraestrutura completa de isolamento de tenants. Toda entidade com escopo de tenant carrega um `TenantId` e os filtros globais de query do EF Core garantem que não haja vazamento de dados entre colégios. Um middleware resolve o tenant atual a partir das claims JWT do usuário autenticado em cada requisição.

## Critérios de Aceite

1. **AC1:** Entidade `Tenant` existe com `Id` (Guid), `Name`, `Slug` (único), `CreatedAt`.
2. **AC2:** Interface `ITenantScoped` existe com a propriedade `Guid TenantId { get; }`.
3. **AC3:** Todas as entidades com escopo de tenant implementam `ITenantScoped`.
4. **AC4:** `AppDbContext` aplica filtro global de query `e => e.TenantId == _currentTenantId` para todas as entidades que implementam `ITenantScoped`.
5. **AC5:** `TenantMiddleware` resolve `TenantId` a partir de `HttpContext.Items["TenantId"]` definido pela autenticação.
6. **AC6:** Serviço `ITenantContext` fornece `TenantId` com escopo vinculado à requisição atual.
7. **AC7:** Tentar consultar dados sem um contexto de tenant lança `TenantNotResolvedException`.
8. **AC8:** Um `TenantSeeder` cria um tenant padrão na primeira execução (para inicialização).
9. **AC9:** Usuário admin do tenant pode ser semeado por tenant.

## Dependências

- Spec 10: Bootstrap & Infrastructure (requer solution, DbContext, Docker)
