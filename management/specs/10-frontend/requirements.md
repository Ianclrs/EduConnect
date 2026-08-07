---
name: Frontend Application (React + PWA)
status: planned
references: V10
---

# Spec 10: Frontend Application (React + PWA)

## What This Spec Delivers

SPA completa em React 19 + Vite 6 + Tailwind CSS 4 com suporte PWA (Workbox). Duas áreas distintas: Admin/Staff (gestão escolar) e Pais (portal da família). Design responsivo mobile-first. Service worker para cache offline e install prompt.

## Acceptance Criteria

1. **AC1:** Projeto React inicializado com Vite 6, TypeScript, Tailwind CSS 4.
2. **AC2:** Tela de login com email/senha e botão "Entrar com Google".
3. **AC3:** Área Admin: Dashboard com cards (alunos, matrículas, documentos pendentes, notificações).
4. **AC4:** Área Admin: CRUD de alunos com tabela, busca e paginação.
5. **AC5:** Área Admin: Gestão de matrículas com workflow de aprovação/rejeição.
6. **AC6:** Área Admin: Verificação de documentos (aprovar/rejeitar).
7. **AC7:** Área Admin: Criação e envio de notificações.
8. **AC8:** Área Pais: Dashboard com cards dos filhos e resumo geral.
9. **AC9:** Área Pais: Visualização de documentos pendentes e upload.
10. **AC10:** Área Pais: Inbox de notificações com marcação de lida.
11. **AC11:** PWA: Service worker registrado, cache de assets, tela offline.
12. **AC12:** PWA: Manifest.json com ícones, nome, tema. Install prompt nativo.
13. **AC13:** Token JWT armazenado em memória (não localStorage), refresh automático via Axios interceptor.
14. **AC14:** Layout responsivo (mobile-first) usando Tailwind.

## Dependencies

- Spec 1-9 (todas as APIs precisam estar funcionando)
