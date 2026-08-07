---
name: Document Management
status: planned
references: V7
---

# Spec 7: Document Management

## What This Spec Delivers

Sistema de gestão de documentos dos alunos. Upload de arquivos, categorização por tipo (RG, CPF, comprovante, histórico, etc.), workflow de verificação pela secretaria, e tracking de validade/vencimento.

## Acceptance Criteria

1. **AC1:** `POST /documents/upload` faz upload de documento vinculado a aluno (multipart/form-data).
2. **AC2:** `GET /students/{id}/documents` lista documentos do aluno com status e validade.
3. **AC3:** `GET /documents/{id}/download` faz download do arquivo.
4. **AC4:** `POST /documents/{id}/verify` aprova documento (Admin/Staff).
5. **AC5:** `POST /documents/{id}/reject` rejeita documento com motivo.
6. **AC6:** `GET /documents/pending` lista documentos pendentes de verificação (Admin/Staff).
7. **AC7:** `GET /documents/expiring` lista documentos próximos do vencimento (30 dias).
8. **AC8:** Tipos de documento são configuráveis por tenant: `POST /document-types`.
9. **AC9:** Arquivos armazenados em disco local (dev) — caminho configurável.
10. **AC10:** Documento tem campos: Tipo, AlunoId, NomeArquivo, CaminhoArquivo, Status (Pendente/Aprovado/Rejeitado), DataValidade, MotivoRejeicao.

## Dependencies

- Spec 1, 2, 3, 4 (precisa de alunos)
