---
name: Frontend Application (React + PWA)
status: planned
references: V10
---

# Spec 100: Frontend Application (React + PWA)

## Value Delivery

Esta spec entrega **V10: Frontend Application** do `management/vision.md`. Especificamente:

- **SPA React 19 + Vite 6 + Tailwind CSS 4** com TypeScript strict mode.
- **Duas áreas distintas:** Admin/Staff (gestão escolar) e Pais (portal da família).
- **PWA completo:** Service worker (Workbox), cache offline, install prompt nativo.
- **Design responsivo mobile-first** com Tailwind.
- **Segurança:** Token JWT em memória (não localStorage), refresh automático via Axios interceptor.

## Functional Requirements

### FR-001: Scaffold do Projeto
- Projeto Vite 6 + React 19 + TypeScript 5.7 em `frontend/`.
- Tailwind CSS 4 com `@tailwindcss/vite`. TypeScript strict mode.
- Dependencies: react-router-dom ^7, axios ^1.7, react-hot-toast ^2, lucide-react ^0.400+, vite-plugin-pwa ^0.21+.
- Acceptance: `npm run build` exits 0. `npm run dev` inicia na porta 5173.

### FR-002: Login
- Tela `/login` com formulário email/senha + botão "Entrar com Google".
- Google OAuth: redirect para `/auth/google` no backend. Callback em `/auth/google/callback`.
- Token JWT armazenado em variável de módulo (memória). Role determina redirect: Admin/Staff → `/admin`, Parent → `/parent`.
- Acceptance: Login email/senha funciona. Login Google funciona. Token em memória.

### FR-003: Axios Client
- `src/api/client.ts`: Axios instance com `withCredentials: true`.
- Request interceptor: anexa `Authorization: Bearer {token}`.
- Response interceptor: 401 → tenta refresh → retry. Falha no refresh → redirect `/login`.
- Arquivos de API: `auth.ts`, `students.ts`, `enrollments.ts`, `documents.ts`, `notifications.ts`, `parent.ts`.
- Acceptance: Chamadas API funcionam com token. Refresh automático funciona.

### FR-004: AuthContext
- `AuthContext.tsx` com estado: user, accessToken (em memória), isAuthenticated.
- Métodos: login, logout, refreshToken, getCurrentUser.
- Persistência: ao recarregar página, tenta refresh silencioso via cookie.
- Acceptance: Estado de auth persiste entre reloads.

### FR-005: UI Components
- Componentes reutilizáveis: Button, Card, Input, Table, Modal, Badge, Pagination, Sidebar, Layout.
- Todos estilizados com Tailwind. Responsivos.
- Acceptance: Componentes renderizam corretamente em mobile e desktop.

### FR-006: AdminLayout
- Sidebar: Dashboard, Alunos, Matrículas, Documentos, Notificações.
- Header: nome do usuário + role + botão logout.
- `ProtectedRoute` wrapper: redireciona para `/login` se não autenticado.
- Acceptance: Admin vê sidebar. Staff vê sidebar (sem opções Admin-only).

### FR-007: Admin Dashboard
- Cards: Total de Alunos, Matrículas Ativas, Documentos Pendentes, Notificações Não-lidas.
- Dados via API calls agregadas.
- Acceptance: Dashboard mostra números corretos.

### FR-008: Student Management (Admin)
- Lista com busca por nome, filtro por turma e status. Paginação.
- Formulário de criação/edição com todos os campos.
- Detalhes: dados do aluno + pais vinculados (com botão adicionar/remover).
- Soft-delete com confirmação.
- Acceptance: CRUD completo funciona.

### FR-009: Enrollment Management (Admin)
- Lista com filtro por período e status. Paginação.
- Detalhes: dados do aluno, período, timeline de status, checklist de documentos.
- Botões: Aprovar, Rejeitar (com modal de motivo).
- Acceptance: Workflow de aprovação/rejeição funciona.

### FR-010: Document Verification (Admin)
- Tabs: Pendentes / Todos.
- Preview de imagens, link de download para PDFs.
- Botões Aprovar/Rejeitar com modal de motivo.
- Acceptance: Verificação de documentos funciona.

### FR-011: Notifications (Admin)
- Lista de notificações enviadas.
- Formulário de criação: título, mensagem, tipo, destino (broadcast ou usuários específicos).
- Acceptance: Admin cria e visualiza notificações.

### FR-012: Parent Dashboard
- Cards dos filhos: nome, turma, status matrícula, documentos pendentes.
- Badge de notificações não-lidas no header.
- Acceptance: Pai vê cards dos filhos vinculados.

### FR-013: Child Detail (Parent)
- Tabs: Informações, Documentos, Notas, Histórico de Matrícula.
- Upload de documentos pendentes.
- Acceptance: Pai visualiza e faz upload de documentos.

### FR-014: Notification Inbox (Parent)
- Lista com indicador de lida/não-lida.
- Clique expande/marca como lida.
- Botão "Marcar todas como lidas".
- Acceptance: Inbox funciona com tracking de leitura.

### FR-015: PWA
- Service worker registrado via `vite-plugin-pwa` (autoUpdate).
- Manifest.json: nome "EduGestor", tema #4f46e5 (indigo-600), display standalone.
- Ícones 192x192 e 512x512.
- Cache offline: página offline quando sem conexão.
- Install prompt nativo no mobile.
- Acceptance: `npm run build` gera SW. Instalável no Chrome Android.

## Non-Functional Requirements

### NFR-001: Performance
- Lighthouse PWA score ≥ 90.
- First Contentful Paint < 2s em 3G.

### NFR-002: Responsividade
- Layout funciona em 320px (iPhone SE) até 1920px (desktop).

## Constraints

- Depende de todas as APIs (Spec 10-90) estarem funcionando.
- Node.js ≥ 20. npm ≥ 10.
- NÃO implementa SSR/SSG. SPA pura.
- NÃO implementa testes E2E (Playwright/Cypress) — apenas build verification.

## Edge Cases & Error States

### E1: Token expirado durante navegação → interceptor faz refresh silencioso. Se falhar → redirect /login.
### E2: API offline → toast de erro. Tela offline via service worker.
### E3: Formulário com validação: campos obrigatórios destacados, mensagens de erro inline.
### E4: Paginação: página vazia mostra mensagem "Nenhum resultado encontrado".
### E5: Upload de arquivo grande → barra de progresso + toast de erro se > 10MB.

## Dependencies

- Spec 10-90 (todas as APIs)
