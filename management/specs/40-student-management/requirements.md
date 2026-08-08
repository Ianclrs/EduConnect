---
name: Student Management
status: planned
references: V4
---

# Spec 40: Student Management

## What This Spec Delivers

CRUD completo para gestão de alunos. Cada aluno pertence a um tenant. Inclui dados pessoais, contatos, filiação (vínculo com pais/responsáveis que são usuários do sistema), e informações acadêmicas (turma, ano letivo). Busca com filtros e paginação.

## Acceptance Criteria

1. **AC1:** `GET /students` retorna lista paginada de alunos do tenant atual, com filtros opcionais: nome, turma, status.
2. **AC2:** `GET /students/{id}` retorna detalhes do aluno incluindo pais vinculados.
3. **AC3:** `POST /students` cria novo aluno (Admin/Staff only).
4. **AC4:** `PUT /students/{id}` atualiza dados do aluno.
5. **AC5:** `DELETE /students/{id}` soft-delete (desativa) o aluno.
6. **AC6:** `POST /students/{id}/link-parent` vincula um usuário pai ao aluno.
7. **AC7:** `DELETE /students/{id}/link-parent/{parentId}` desvincula um pai.
8. **AC8:** Aluno tem campos: Nome, DataNascimento, CPF (opcional), Turma, AnoLetivo, Status (Ativo/Inativo/Transferido), Observacoes.
9. **AC9:** Aluno implementa `ITenantScoped`.
10. **AC10:** Pais só podem ver alunos vinculados a eles.

## Dependencies

- Spec 10, 2, 3 (precisa de tenant, auth, roles)
