# Ciclo

> Plataforma SaaS multi-tenant de gestão escolar que conecta colégios e pais em um único lugar.

---

## Sobre

O **Ciclo** centraliza e automatiza o ciclo de vida acadêmico dos alunos: matrículas, rematrículas, documentos obrigatórios, notificações e acompanhamento de desempenho — eliminando papelada, reduzindo erros e acelerando a comunicação entre escola e família.

- **Gestores escolares (Admin/Staff):** gerenciam matrículas, documentos, turmas e comunicação com os pais.
- **Pais/Responsáveis:** acompanham o desempenho dos filhos, recebem notificações de documentos pendentes e acompanham status de matrícula/rematrícula.

### Funcionalidades

| # | Funcionalidade | Descrição |
|---|---------------|-----------|
| 1 | **Multi-Tenant** | Cada colégio tem seu ambiente isolado via `TenantId`. Dados nunca vazam entre tenants. |
| 2 | **Autenticação** | Login com email/senha ou Google. JWT access tokens (15min) + refresh tokens (7 dias). |
| 3 | **Gestão de Alunos** | CRUD completo com dados pessoais, contatos, filiação e informações acadêmicas. |
| 4 | **Matrícula** | Workflow completo: abertura de período, inscrição, checklist de documentos, aprovação. |
| 5 | **Rematrícula** | Renovação com reaproveitamento de documentos validados e alertas de vencimento. |
| 6 | **Documentos** | Upload, categorização, verificação pela secretaria, tracking de validade. |
| 7 | **Notificações** | In-app + email para documentos pendentes, comunicados e lembretes. |
| 8 | **Portal dos Pais** | Dashboard dos filhos, desempenho, inbox de notificações e upload de documentos. |
| 9 | **PWA Mobile** | Aplicativo progressivo instalável no celular (iOS/Android) com suporte offline. |

---

## Tecnologias

| Camada | Tecnologia |
|--------|-----------|
| **Backend** | C# .NET 10 — ASP.NET Core Web API |
| **ORM** | Entity Framework Core 10 + Npgsql |
| **Banco de Dados** | PostgreSQL 16 |
| **Autenticação** | ASP.NET Core Identity + JWT Bearer + Google OAuth 2.0 |
| **Validação** | FluentValidation |
| **Logging** | Serilog (Console + File) |
| **Documentação da API** | Swagger / OpenAPI |
| **Erros** | Global exception middleware + ProblemDetails (RFC 9457) |
| **Armazenamento** | Disco local (dev) / S3-compatible (prod) |
| **Frontend** | React 19 + Vite 8 + Tailwind CSS 4 + TypeScript 6 |
| **PWA** | Workbox (service worker, cache offline, install prompt) |
| **Deploy** | Docker Compose |

### Projetos da Solution

| Projeto | Responsabilidade |
|---------|-----------------|
| `Ciclo.Api` | Controllers, middleware, configuração da aplicação |
| `Ciclo.Core` | Entidades, interfaces, regras de domínio |
| `Ciclo.Infrastructure` | DbContext, serviços, storage, email |
| `Ciclo.Api.Tests` | Testes unitários e de integração |

---

## Como Rodar

### Pré-requisitos

| Ferramenta | Versão |
|-----------|--------|
| .NET SDK | 10.0 |
| Node.js | 22+ |
| Docker + Docker Compose | 24+ / 2+ |

### Backend + Banco

```bash
# Subir banco e API
docker compose up -d
```

A API estará em `http://localhost:5000` e o Swagger em `http://localhost:5000/swagger`.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

O frontend estará em `http://localhost:5173`. O Vite faz proxy das chamadas da API para o backend automaticamente.

### Sem Docker (PostgreSQL local)

```bash
# 1. Criar banco e usuário
psql -U postgres -f db/init-db.sql

# 2. Iniciar a API
dotnet run --project src/Ciclo.Api
```

> As migrations do EF Core são aplicadas automaticamente na inicialização em ambiente Development.

### Testes

```bash
dotnet test
```

---

## Estrutura de Diretórios

```
├── src/
│   ├── Ciclo.Api/            # API REST (controllers, middleware)
│   ├── Ciclo.Core/            # Domínio (entidades, interfaces)
│   └── Ciclo.Infrastructure/  # Dados, serviços, autenticação
├── tests/
│   └── Ciclo.Api.Tests/       # Testes unitários e de integração
├── frontend/                  # SPA React + PWA
├── db/                        # Scripts SQL de inicialização
├── management/                # Documentação SDD (specs, ADRs, roadmap)
└── docker-compose.yml
```
