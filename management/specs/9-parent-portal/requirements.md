---
name: Parent Portal API
status: planned
references: V9
---

# Spec 9: Parent Portal API

## What This Spec Delivers

API específica para o portal dos pais. Dashboard com resumo dos filhos, visualização de desempenho/notas, inbox de notificações, upload de documentos pendentes, e acompanhamento de matrícula. Cada pai vê apenas dados dos filhos vinculados.

## Acceptance Criteria

1. **AC1:** `GET /parent/dashboard` retorna resumo: filhos vinculados, notificações não-lidas, documentos pendentes, status de matrícula.
2. **AC2:** `GET /parent/children` lista filhos vinculados ao pai logado com dados básicos.
3. **AC3:** `GET /parent/children/{id}` retorna detalhes completos do filho: dados, documentos, matrícula, notas.
4. **AC4:** `GET /parent/children/{id}/documents` lista documentos do filho com status (pendente/aprovado/rejeitado).
5. **AC5:** `POST /parent/children/{id}/documents/upload` upload de documento pendente.
6. **AC6:** `GET /parent/children/{id}/grades` retorna notas/desempenho (placeholder estrutura).
7. **AC7:** Todas as queries são automaticamente filtradas por TenantId + vínculo pai-filho.
8. **AC8:** Pai não pode acessar dados de alunos que não são seus filhos.

## Dependencies

- Spec 1, 2, 3, 4, 7, 8
