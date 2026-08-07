---
name: Re-enrollment System (Rematrícula)
status: planned
references: V6
---

# Spec 6: Re-enrollment System (Rematrícula)

## What This Spec Delivers

Workflow de rematrícula para alunos já existentes. Permite renovação de matrícula para um novo ano letivo, reaproveitamento de documentos já validados, e identificação de documentos vencidos que precisam ser atualizados.

## Acceptance Criteria

1. **AC1:** `POST /reenrollments` inicia rematrícula para aluno existente vinculado a novo período.
2. **AC2:** `GET /reenrollments` lista rematrículas com status e data de criação.
3. **AC3:** Sistema reaproveita automaticamente documentos válidos do ano anterior.
4. **AC4:** Documentos vencidos geram pendências no checklist de rematrícula.
5. **AC5:** `POST /reenrollments/{id}/approve` aprova rematrícula e atualiza ano letivo do aluno.
6. **AC6:** Estados: `pendente → documentacao_pendente → aprovado | rejeitado`.
7. **AC7:** `GET /students/{id}/enrollment-history` retorna histórico de matrículas/rematrículas do aluno.

## Dependencies

- Spec 5 (Enrollment) — compartilha entidades e lógica de períodos
- Spec 7 (Documents) — para validação de documentos vencidos
