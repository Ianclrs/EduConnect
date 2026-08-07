# EduGestor Vision

## Product Vision

EduGestor é um sistema de gestão escolar SaaS multi-tenant que conecta colégios e pais em uma única plataforma. Ele centraliza e automatiza todo o ciclo de vida acadêmico dos alunos: matrículas, rematrículas, documentos obrigatórios, notificações e acompanhamento de desempenho — eliminando papelada, reduzindo erros e acelerando a comunicação entre escola e família.

## Target Audience

- **Gestores escolares (Admin/Staff):** Diretores, secretários e coordenadores que gerenciam matrículas, documentos e comunicação com os pais.
- **Pais/Responsáveis:** Usuários finais que acompanham o desempenho dos filhos, recebem notificações de documentos pendentes e comunicados do colégio.

## Value Proposition

- **Centralização:** Todos os dados de alunos, matrículas, documentos e notificações em um só lugar.
- **Automação:** Alertas automáticos de documentos vencidos/pendentes e lembretes de rematrícula.
- **Multi-tenant SaaS:** Cada colégio tem seu ambiente isolado, com seus próprios gestores, alunos e pais.
- **PWA Mobile-Ready:** Acesso completo via navegador no celular com experiência nativa (PWA).
- **Login Simplificado:** Email/senha ou conta Google para os pais, reduzindo fricção no onboarding.

---

## Value Blocks

<!-- Value blocks are added incrementally by /spec-plan and /spec-new. Do not edit manually. -->

### V1: Project Bootstrap & Infrastructure
Inicialização da solução .NET 10 com estrutura de projetos (API, Core, Infrastructure), configuração do PostgreSQL via Docker Compose, EF Core com Npgsql, e pipeline base de CI/CD. Define a fundação sobre a qual todas as outras specs são construídas.

**Delivered by:** Spec 1 (project-bootstrap)

### V2: Multi-Tenant Architecture
Isolamento completo de dados entre colégios usando `TenantId` como coluna discriminadora em todas as tabelas tenant-scoped. Middleware que resolve o tenant a partir do JWT do usuário logado. EF Core global query filters garantem que queries nunca vazem dados entre tenants.

**Delivered by:** Spec 2 (multi-tenant)

### V3: Authentication & Authorization
Login com email/senha e Google OAuth. ASP.NET Core Identity para gestão de credenciais. JWT access tokens (15min) + refresh tokens (7 dias) com rotação. Roles: Admin, Staff, Parent. Cada role tem permissões específicas por tenant.

**Delivered by:** Spec 3 (auth)

### V4: Student Management
CRUD completo de alunos com dados pessoais, contatos, filiação (vínculo pais/responsáveis), e informações acadêmicas. Cada aluno pertence a um tenant. Busca e filtros por nome, turma, status de matrícula.

**Delivered by:** Spec 4 (student-management)

### V5: Enrollment System (Matrícula)
Workflow completo de matrícula: abertura de período de matrícula, inscrição de novos alunos, checklist de documentos obrigatórios, aprovação pela secretaria, e confirmação de matrícula. Estados: pendente, documentacao_pendente, aprovado, recusado, cancelado.

**Delivered by:** Spec 5 (enrollment)

### V6: Re-enrollment System (Rematrícula)
Workflow de rematrícula para alunos existentes: renovação automática ou manual, reaproveitamento de documentos já validados, notificação de documentos vencidos que precisam ser reenviados, e confirmação para o próximo ano letivo.

**Delivered by:** Spec 6 (reenrollment)

### V7: Document Management
Upload de documentos, categorização por tipo (RG, CPF, comprovante de residência, histórico escolar, etc.), verificação pela secretaria (aprovado/rejeitado), tracking de validade/vencimento, e notificação automática de documentos pendentes ou vencidos.

**Delivered by:** Spec 7 (documents)

### V8: Notification System
Sistema de notificações para comunicar gestores e pais. Canais: notificação in-app + email. Tipos: documentos pendentes, reuniões agendadas, comunicados gerais, lembretes de rematrícula. Tracking de leitura e histórico por usuário.

**Delivered by:** Spec 8 (notifications)

### V9: Parent Portal API
API do portal dos pais: dashboard com resumo dos filhos, visualização de desempenho/notas, inbox de notificações, upload de documentos pendentes, e acompanhamento de status de matrícula/rematrícula. Cada pai vê apenas seus filhos vinculados.

**Delivered by:** Spec 9 (parent-portal)

### V10: Frontend Application (React + PWA)
SPA React 19 + Vite 6 + Tailwind CSS 4 com suporte PWA (Workbox). Telas separadas para Admin/Staff (dashboard, gestão de alunos, matrículas, documentos, notificações) e Pais (dashboard dos filhos, documentos, notificações). Install prompt para mobile. Offline-first com cache strategy.

**Delivered by:** Spec 10 (frontend)
