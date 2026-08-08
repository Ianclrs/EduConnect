---
name: Notification System
status: planned
references: V8
---

# Spec 80: Notification System

## What This Spec Delivers

Sistema de notificações para comunicação entre colégio e pais. Suporte a notificações in-app e envio de email. Tipos: documentos pendentes, reuniões, comunicados gerais, lembretes de matrícula/rematrícula. Tracking de leitura.

## Acceptance Criteria

1. **AC1:** `POST /notifications` cria notificação (Admin/Staff): Título, Mensagem, Tipo, Destinatários.
2. **AC2:** `POST /notifications/broadcast` envia para todos os pais do tenant (Admin only).
3. **AC3:** `POST /notifications/by-student/{studentId}` envia para pais de aluno específico.
4. **AC4:** `GET /notifications` lista notificações do usuário logado com filtro de lidas/não-lidas.
5. **AC5:** `PUT /notifications/{id}/read` marca como lida.
6. **AC6:** `PUT /notifications/read-all` marca todas como lidas.
7. **AC7:** Notificações automáticas ao detectar documentos pendentes/vencidos (integrado com Spec 70).
8. **AC8:** Suporte a envio de email (via SMTP configurável, console log em dev).
9. **AC9:** `GET /notifications/unread-count` retorna contagem de não-lidas.

## Dependencies

- Spec 10, 2, 3, 4 (precisa de auth, alunos, pais)
- Spec 70 (documentos — para notificações automáticas)
