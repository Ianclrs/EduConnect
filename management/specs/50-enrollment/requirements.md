---
name: Enrollment System (Matrícula)
status: planned
references: V5
---

# Spec 50: Enrollment System (Matrícula)

## What This Spec Delivers

Workflow completo de matrícula para novos alunos. Inclui criação de período de matrícula, checklist de documentos obrigatórios, inscrição de novos alunos, revisão e aprovação pela secretaria. Estados bem definidos com transições controladas.

## Acceptance Criteria

1. **AC1:** `POST /enrollment-periods` cria um período de matrícula (Admin only): DataInicio, DataFim, AnoLetivo.
2. **AC2:** `GET /enrollment-periods` lista períodos de matrícula do tenant.
3. **AC3:** `POST /enrollments` inicia matrícula: vincula aluno novo, período, e checklist de documentos.
4. **AC4:** `GET /enrollments` lista matrículas com filtros: status, período, turma.
5. **AC5:** `GET /enrollments/{id}` retorna matrícula com status dos documentos.
6. **AC6:** `POST /enrollments/{id}/approve` aprova matrícula (Admin/Staff).
7. **AC7:** `POST /enrollments/{id}/reject` rejeita com motivo.
8. **AC8:** Estados: `rascunho → pendente → documentacao_pendente → aprovado | rejeitado | cancelado`.
9. **AC9:** Transições de status são validadas (ex: não pode aprovar matrícula cancelada).
10. **AC10:** Matrícula aprovada atualiza status do aluno para Ativo.

## Dependencies

- Spec 10, 2, 3, 4 (precisa de alunos, documentos)
