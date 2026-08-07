# EduGestor

> Plataforma SaaS multi-tenant de gestão escolar que conecta colégios e pais em um único lugar.

---

## O que é o EduGestor?

O EduGestor centraliza e automatiza todo o ciclo de vida acadêmico dos alunos: **matrículas**, **rematrículas**, **documentos obrigatórios**, **notificações** e **acompanhamento de desempenho**. Ele elimina papelada, reduz erros e acelera a comunicação entre escola e família.

### Para quem é?

| Perfil | O que faz na plataforma |
|--------|--------------------------|
| **Gestores escolares (Admin/Staff)** | Diretores, secretários e coordenadores que gerenciam matrículas, documentos, turmas e comunicação com os pais. |
| **Pais/Responsáveis** | Acompanham o desempenho dos filhos, recebem notificações de documentos pendentes, fazem upload de documentos e acompanham status de matrícula/rematrícula. |

---

## Funcionalidades

| # | Funcionalidade | Descrição |
|---|---------------|-----------|
| 1 | **Multi-Tenant Isolation** | Cada colégio tem seu ambiente isolado com `TenantId`. Dados nunca vazam entre tenants. |
| 2 | **Autenticação** | Login com email/senha ou conta Google. JWT access tokens (15min) + refresh tokens (7 dias) com rotação. |
| 3 | **Gestão de Alunos** | CRUD completo com dados pessoais, contatos, filiação (vínculo pais/responsáveis) e informações acadêmicas. |
| 4 | **Matrícula** | Workflow completo: abertura de período, inscrição, checklist de documentos, aprovação e confirmação. |
| 5 | **Rematrícula** | Renovação para alunos existentes com reaproveitamento de documentos validados e alertas de documentos vencidos. |
| 6 | **Documentos** | Upload, categorização (RG, CPF, comprovante, histórico escolar), verificação pela secretaria, tracking de validade e vencimento. |
| 7 | **Notificações** | Notificações in-app + email para documentos pendentes, reuniões, comunicados e lembretes de rematrícula. |
| 8 | **Portal dos Pais** | Dashboard com resumo dos filhos, desempenho/notas, inbox de notificações e upload de documentos pendentes. |
| 9 | **PWA Mobile** | Aplicativo progressivo instalável no celular (iOS/Android) com experiência nativa e suporte offline. |

---

## Stack Tecnológica

| Camada | Tecnologia |
|--------|-----------|
| **Backend** | C# .NET 10 — ASP.NET Core Web API |
| **ORM** | Entity Framework Core 10 + Npgsql |
| **Banco de Dados** | PostgreSQL 16 |
| **Autenticação** | ASP.NET Core Identity + JWT Bearer + Google OAuth 2.0 (OpenID Connect) |
| **Validação** | FluentValidation |
| **Logging** | Serilog (Console + File) |
| **Documentação da API** | Swagger / OpenAPI |
| **Erros** | Global exception middleware + ProblemDetails (RFC 9457) |
| **Background Jobs** | Hangfire / Quartz.NET |
| **Armazenamento de Arquivos** | Disco local (dev) / S3-compatible (prod) |
| **Frontend** | React 19 + Vite 6 + Tailwind CSS 4 |
| **PWA** | Workbox (service worker, cache offline, install prompt) |
| **Deploy** | Docker Compose (dev), Docker Swarm / AWS ECS (prod) |

---

## Arquitetura

```
┌─────────────────────────────────────┐
│  Presentation (React SPA + PWA)     │  ← Frontend (Spec 10)
├─────────────────────────────────────┤
│  API Layer (ASP.NET Core Controllers)│  ← REST endpoints
├─────────────────────────────────────┤
│  Application Layer (Services)       │  ← Regras de negócio, workflows
├─────────────────────────────────────┤
│  Domain Layer (Entities, Value Obj) │  ← Modelos de domínio
├─────────────────────────────────────┤
│  Infrastructure Layer               │  ← EF Core, Auth, Storage, Email
├─────────────────────────────────────┤
│  PostgreSQL 16                       │  ← Persistência de dados
└─────────────────────────────────────┘
```

### Organização dos Projetos (Solution .NET)

| Projeto | Responsabilidade |
|---------|-----------------|
| `EduGestor.Api` | Controllers, middleware, configuração da aplicação |
| `EduGestor.Core` | Entidades, interfaces, DTOs, regras de domínio |
| `EduGestor.Infrastructure` | DbContext, repositórios, serviços de email, storage |
| `EduGestor.Tests` | Testes unitários e de integração |

---

## Modelo Multi-Tenant

Cada colégio é um **tenant** isolado. O isolamento é feito via **coluna `TenantId`** em todas as tabelas tenant-scoped, combinado com:

1. **Middleware** que resolve o `TenantId` a partir do claim JWT do usuário logado em cada requisição.
2. **EF Core Global Query Filters** que automaticamente adicionam `WHERE TenantId = @currentTenantId` em toda query — impossibilitando vazamento de dados entre tenants.

> Decisão documentada em [ADR-002](management/adr.md#adr-002-shared-database-with-tenantid-column).

---

## Autenticação e Autorização

| Mecanismo | Detalhe |
|-----------|---------|
| **Access Token (JWT)** | 15 minutos — armazenado em memória no SPA (nunca em localStorage) |
| **Refresh Token** | 7 dias — HttpOnly cookie + armazenado no banco (revogável) |
| **Google OAuth** | OpenID Connect via `Microsoft.AspNetCore.Authentication.Google` |
| **Roles** | `Admin`, `Staff`, `Parent` — permissões específicas por tenant |

> Decisão documentada em [ADR-003](management/adr.md#adr-003-jwt-access--refresh-tokens).

---

## Status do Projeto

> **Fase atual:** Especificação — zero código implementado.

O desenvolvimento segue o fluxo **SDD (Spec-Driven Development)**. Cada funcionalidade passa por 5 etapas:
`planned → created → verified → implemented → audited`

| # | Spec | Status |
|---|------|--------|
| 1 | Project Bootstrap & Infrastructure | ✅ `verified` |
| 2 | Multi-Tenant Architecture | 📋 `planned` |
| 3 | Authentication & Authorization | 📋 `planned` |
| 4 | Student Management | 📋 `planned` |
| 5 | Enrollment System (Matrícula) | 📋 `planned` |
| 6 | Re-enrollment System (Rematrícula) | 📋 `planned` |
| 7 | Document Management | 📋 `planned` |
| 8 | Notification System | 📋 `planned` |
| 9 | Parent Portal API | 📋 `planned` |
| 10 | Frontend Application (React + PWA) | 📋 `planned` |

### Dependências entre Specs

```
Spec 1: Bootstrap & Infra
  └─► Spec 2: Multi-Tenant
        ├─► Spec 3: Auth
        │     ├─► Spec 4: Student Management
        │     │     ├─► Spec 5: Enrollment ──► Spec 6: Re-enrollment
        │     │     ├─► Spec 7: Documents
        │     │     └─► Spec 9: Parent Portal API ──► Spec 10: Frontend
        │     └─► Spec 8: Notifications ──► Spec 9: Parent Portal API
        └─► Spec 7: Documents ──► Spec 8: Notifications
```

---

## Pré-requisitos para Desenvolvimento

| Ferramenta | Versão | Uso |
|-----------|--------|-----|
| **.NET SDK** | 10.0 | Backend API |
| **Node.js** | 22+ | Frontend (Vite/React) |
| **Docker** | 24+ | PostgreSQL e serviços |
| **Docker Compose** | 2+ | Orquestração local |

---

## Como Rodar (quando implementado)

```bash
# 1. Clonar o repositório
git clone <repo-url> && cd EduConnect

# 2. Subir o banco de dados
docker compose up -d

# 3. Rodar as migrations
dotnet ef database update --project src/EduGestor.Api

# 4. Iniciar a API
dotnet run --project src/EduGestor.Api

# 5. Iniciar o frontend (em outro terminal)
cd frontend && npm install && npm run dev
```

A API estará em `https://localhost:5001` e o frontend em `http://localhost:5173`.

---

## Decisões de Arquitetura (ADRs)

| ADR | Decisão |
|-----|---------|
| [ADR-001](management/adr.md#adr-001-net-10--ef-core--postgresql-stack) | Stack .NET 10 + EF Core + PostgreSQL 16 |
| [ADR-002](management/adr.md#adr-002-shared-database-with-tenantid-column) | Banco compartilhado com coluna `TenantId` |
| [ADR-003](management/adr.md#adr-003-jwt-access--refresh-tokens) | JWT access + refresh tokens com rotação |

---

## Estrutura de Diretórios

```
EduConnect/
├── README.md                   ← Você está aqui
├── management/                 ← Documentação SDD (specs, arquitetura, roadmap)
│   ├── vision.md               │  Visão do produto e value blocks (V1-V10)
│   ├── arch.md                 │  Arquitetura técnica e conceitual
│   ├── roadmap.md              │  Status das specs e grafo de dependências
│   ├── adr.md                  │  Decisões de arquitetura (ADR-001 a ADR-003)
│   ├── lessons.md              │  Lições aprendidas
│   └── specs/                  │  Specs individuais
│       ├── 1-project-bootstrap/│    requirements.md · design.md · task.md
│       ├── 2-multi-tenant/     │    requirements.md · design.md · task.md
│       ├── 3-auth/             │    ...
│       ├── 4-student-management/
│       ├── 5-enrollment/
│       ├── 6-reenrollment/
│       ├── 7-documents/
│       ├── 8-notifications/
│       ├── 9-parent-portal/
│       └── 10-frontend/
├── .dscode/                    ← Estado interno do DsCode (settings, memória, logs)
└── src/                        ← Código fonte (a ser criado na Spec 1)
    ├── EduGestor.Api/
    ├── EduGestor.Core/
    ├── EduGestor.Infrastructure/
    └── EduGestor.Tests/
```
