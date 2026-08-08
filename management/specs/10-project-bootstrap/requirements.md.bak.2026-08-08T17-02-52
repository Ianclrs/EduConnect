---
name: Project Bootstrap & Infrastructure
status: verified
references: V1, ADR-001
---

# Spec 10: Project Bootstrap & Infrastructure

## O Que Esta Spec Entrega

Inicializa a solução backend completa em .NET 10 com separação adequada de projetos, conectividade com banco de dados, suporte a Docker e todas as configurações fundamentais. Após a implementação desta spec, um desenvolvedor pode executar `docker compose up` e ter uma instância PostgreSQL funcional com a API rodando e respondendo a health checks.

## Critérios de Aceite

1. **AC1:** `dotnet build` executa com sucesso na solução, com zero erros e zero warnings.
2. **AC2:** `docker compose up` inicia o PostgreSQL 16 e a API nas portas 5432 e 5000, respectivamente.
3. **AC3:** `GET /health` retorna `200 OK` com `{ "status": "Healthy" }`.
4. **AC4:** `GET /health/db` retorna `200 OK` confirmando conectividade com o PostgreSQL.
5. **AC5:** Migrations do EF Core executam com sucesso contra o container PostgreSQL.
6. **AC6:** Swagger UI está disponível em `/swagger` no modo Development.
7. **AC7:** A estrutura de projetos corresponde exatamente ao documento de design.
8. **AC8:** `.gitignore` exclui todos os artefatos de build, `bin/`, `obj/`, `.env` e arquivos de IDE.

## Entrega de Valor

Implementa o V1 (Project Bootstrap & Infrastructure) do vision.md. Esta spec entrega toda a fundação técnica da plataforma EduGestor: um projeto API .NET 10 executável, conectividade com PostgreSQL via Docker, integração com EF Core, endpoints de monitoramento de saúde e prontidão para build/CI. Sem esta spec, nenhuma outra spec pode iniciar sua implementação.

## Requisitos Funcionais

| ID | Requisito | Cobertura AC |
|---|---|---|
| FR1 | A solução DEVE conter 4 projetos: EduGestor.Api, EduGestor.Core, EduGestor.Infrastructure, EduGestor.Api.Tests | AC1, AC7 |
| FR2 | EduGestor.Api DEVE referenciar EduGestor.Infrastructure e EduGestor.Core | AC1, AC7 |
| FR3 | EduGestor.Infrastructure DEVE referenciar EduGestor.Core | AC1, AC7 |
| FR4 | Todos os projetos DEVEM target net10.0 com Nullable habilitado, ImplicitUsings habilitado, TreatWarningsAsErrors=true | AC1 |
| FR5 | A API DEVE expor `GET /health` retornando 200 OK com `{"status":"Healthy"}` | AC3 |
| FR6 | A API DEVE expor `GET /health/db` retornando 200 OK quando PostgreSQL estiver acessível, 503 quando não estiver | AC4 |
| FR7 | A API DEVE usar Serilog para logging estruturado, com console sink configurado na inicialização | AC1 |
| FR8 | Swagger UI DEVE estar disponível em `/swagger` quando `ASPNETCORE_ENVIRONMENT=Development` | AC6 |
| FR9 | O DbContext do EF Core (AppDbContext) DEVE conectar ao PostgreSQL via Npgsql usando a connection string de `ConnectionStrings:Default` | AC4, AC5 |
| FR10 | Migrations do EF Core DEVEM ser aplicadas automaticamente na inicialização em modo Development via `db.Database.Migrate()` | AC5 |
| FR11 | Docker Compose DEVE definir serviço PostgreSQL 16-alpine com healthcheck (`pg_isready`) | AC2 |
| FR12 | Docker Compose DEVE definir serviço API que depende do healthcheck do PostgreSQL, expondo porta 5000 mapeada para porta 8080 do container | AC2 |
| FR13 | Dockerfile DEVE ser multi-stage (build SDK + runtime ASP.NET) usando `mcr.microsoft.com/dotnet/sdk:10.0` e `mcr.microsoft.com/dotnet/aspnet:10.0` | AC2 |
| FR14 | `.gitignore` DEVE excluir `bin/`, `obj/`, `.env`, diretórios de IDE e artefatos de build padrão do .NET | AC8 |
| FR15 | `.dockerignore` DEVE excluir `bin/`, `obj/`, `.git/`, `node_modules/` | AC2 |
| FR16 | `Directory.Build.props` DEVE definir TargetFramework=net10.0, Nullable=enable, ImplicitUsings=enable, TreatWarningsAsErrors=true, AnalysisLevel=latest-recommended | AC1 |

## Requisitos Não-Funcionais

| ID | Requisito | Meta |
|---|---|---|
| NFR1 | Tempo de build a partir do estado limpo DEVE ser inferior a 60 segundos | `dotnet build` em máquina de desenvolvedor |
| NFR2 | Endpoint de health DEVE responder em até 200ms (p95) | Medido via qualquer cliente HTTP |
| NFR3 | Endpoint de health do DB DEVE timeout após 5 segundos e retornar 503 se PostgreSQL estiver inacessível | Connection timeout na connection string |
| NFR4 | Todos os pacotes NuGet DEVEM usar notação de versão flutuante (sufixo `*`) para atualizações de patch | ex.: `10.*` |
| NFR5 | Imagens Docker DEVEM ser baseadas em imagens oficiais Microsoft .NET (sem imagens base de terceiros) | `mcr.microsoft.com/dotnet/*` |
| NFR6 | Imagem Docker do PostgreSQL DEVE usar variante `16-alpine` para tamanho mínimo de imagem | `postgres:16-alpine` |
| NFR7 | Código DEVE compilar com zero warnings (TreatWarningsAsErrors=true) | AC1 |

## Restrições

| ID | Restrição |
|---|---|
| C1 | Versão do .NET SDK deve ser exatamente 10.0.x — nem anterior, nem posterior |
| C2 | Target framework deve ser `net10.0` |
| C3 | Todos os nomes de projeto DEVEM começar com o prefixo `EduGestor.` |
| C4 | Pastas de projeto DEVEM estar sob `src/` (código-fonte) e `tests/` (projetos de teste) |
| C5 | Arquivo de solução DEVE ser nomeado `EduGestor.sln` na raiz do repositório |
| C6 | Docker Compose DEVE usar `docker compose` (sintaxe do plugin v2), não `docker-compose` (v1) |
| C7 | Porta 5432 para PostgreSQL, porta 5000 para API — estas NÃO são negociáveis para evitar conflitos com as Specs 2-10 |
| C8 | `.env.example` DEVE existir com `DB_PASSWORD=edugestor_dev` — `.env` DEVE ser ignorado pelo git |

## Casos de Borda e Estados de Erro

| ID | Cenário | Comportamento Esperado |
|---|---|---|
| E1 | Container PostgreSQL não está rodando quando a API inicia | `AddInfrastructure()` loga warning mas NÃO crasha. Health check `/health` retorna 200 (API está viva). `/health/db` retorna 503 com `{"status":"Unhealthy","database":"Disconnected"}` |
| E2 | PostgreSQL fica indisponível após a API estar rodando | EF Core lança `NpgsqlException` na próxima query. Health check `/health/db` retorna 503. Sem crash — degradação graciosa |
| E3 | Build Docker falha por falta da imagem .NET SDK | `docker compose build` retorna código de saída diferente de zero. Mensagem de erro do daemon do Docker |
| E4 | Porta 5432 ou 5000 já em uso no host | Docker Compose falha ao iniciar com erro de conflito de porta. Usuário deve liberar as portas ou alterar sobrescritas no `.env` |
| E5 | Migration do EF Core falha (ex.: schema incompatível em BD existente) | Auto-migration na inicialização lança `MigrationException`. API falha ao iniciar — loga erro via Serilog. Isso é intencional: não executar com schema inválido |
| E6 | `dotnet test` executa sem PostgreSQL disponível | HealthControllerTests para `/health/db` falham. Outros testes (apenas `/health`) passam. Testes que exigem BD devem ser marcados como integração |
| E7 | Solução aberta em máquina sem .NET 10 SDK | `dotnet build` falha com erro de SDK não encontrado. Directory.Build.props especifica net10.0 que exige SDK 10 |
| E8 | Arquivo `.env` contém caracteres especiais em DB_PASSWORD | PostgreSQL os aceita se devidamente codificados em URL. Connection string no docker-compose.yml passa o valor literalmente — caracteres especiais como `$`, `&` devem ser escapados ou evitados em dev |
| E9 | Volume Docker `pgdata` contém dados de uma versão major diferente do PostgreSQL | PostgreSQL 16-alpine recusa iniciar. Usuário deve executar `docker compose down -v` para redefinir o volume |
| E10 | `ASPNETCORE_ENVIRONMENT` está definido como `Production` | Swagger UI NÃO está disponível em `/swagger`. Auto-migration NÃO executa. Este é o comportamento correto — segurança em produção |

## Dependências

- Nenhuma (esta é a spec raiz — nada depende dela, tudo depende dela).
