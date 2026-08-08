# Spec 50: Design — Enrollment System

## Design Approach

Sistema de matrícula com **máquina de estados server-side**. A entidade `Enrollment` gerencia o ciclo de vida da matrícula: rascunho → pendente → documentacao_pendente → aprovado/rejeitado/cancelado. `EnrollmentPeriod` define janelas de matrícula por ano letivo.

**State machine**: método `CanTransition(from, to): bool` valida cada transição. Transações atômicas: status + side effects (Student.Status, ApprovedAt) na mesma operação EF Core.

## Architecture Decisions

- **AD-001: Máquina de estados explícita** — transições inválidas rejeitadas com 400 + estados permitidos listados.
- **AD-002: Período de matrícula por tenant** — cada tenant gerencia seus próprios períodos. Sem sobreposição de datas.
- **AD-003: DocumentacaoPendente como estado intermediário** — permite fluxo parcial: matrícula existe mas aguarda documentos (Spec 70).

## Data Flow: Submeter → Aprovar

```
POST /enrollments/{id}/submit
  → EnrollmentService.SubmitAsync(id, tenantId)
    → Validate: CanTransition(Rascunho, Pendente)? YES
    → Check pending required documents (Spec 70)
    → If pending: Status = DocumentacaoPendente
    → If no pending: Status = Pendente
    → SaveChangesAsync()

POST /enrollments/{id}/approve
  → EnrollmentService.ApproveAsync(id, tenantId)
    → Validate: CanTransition(current, Aprovado)? YES
    → Validate: no pending required documents
    → Status = Aprovado, ApprovedAt = UtcNow
    → Student.Status = Ativo, Student.AnoLetivo = Period.AnoLetivo
    → SaveChangesAsync() (atomic)
```

## Domain Entities

### EnrollmentPeriod (EduGestor.Core/Entities/EnrollmentPeriod.cs)
```csharp
public class EnrollmentPeriod : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public int AnoLetivo { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = [];
}
```

### Enrollment (EduGestor.Core/Entities/Enrollment.cs)
```csharp
public class Enrollment : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid StudentId { get; set; }
    public Guid EnrollmentPeriodId { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Rascunho;
    public string? MotivoRejeicao { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public Student Student { get; set; } = null!;
    public EnrollmentPeriod Period { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}

public enum EnrollmentStatus
{
    Rascunho = 0, Pendente = 1, DocumentacaoPendente = 2,
    Aprovado = 3, Rejeitado = 4, Cancelado = 5
}
```

### State Machine Rules
```
Rascunho              → Pendente               (submit)
Pendente              → DocumentacaoPendente    (docs pending)
Pendente              → Aprovado                (approve, all docs ok)
Pendente              → Rejeitado               (reject)
Pendente              → Cancelado               (cancel)
DocumentacaoPendente  → Pendente                (docs completed)
DocumentacaoPendente  → Aprovado                (approve after docs)
DocumentacaoPendente  → Rejeitado               (reject)
DocumentacaoPendente  → Cancelado               (cancel)
Aprovado              → Cancelado               (Admin only, reverts Student.Status)
```
Invalid transitions return 400 with allowed states list.

## DTOs
```csharp
public record CreateEnrollmentPeriodRequest(string Nome, DateTime DataInicio, DateTime DataFim, int AnoLetivo);
public record EnrollmentPeriodDto(Guid Id, string Nome, DateTime DataInicio, DateTime DataFim, int AnoLetivo, bool IsActive);
public record CreateEnrollmentRequest(Guid StudentId, Guid EnrollmentPeriodId);
public record EnrollmentDto(Guid Id, Guid StudentId, string StudentName, Guid PeriodId, string PeriodName, string Status, string? MotivoRejeicao, DateTime CreatedAt, DateTime? ApprovedAt);
public record RejectEnrollmentRequest(string Motivo);
```

## EnrollmentController

| Endpoint | Auth |
|---|---|
| `POST /enrollment-periods` | Admin |
| `GET /enrollment-periods` | Admin, Staff |
| `PUT /enrollment-periods/{id}/close` | Admin |
| `POST /enrollments` | Admin, Staff |
| `GET /enrollments?status=&periodId=&page=1&pageSize=20` | Admin, Staff |
| `GET /enrollments/{id}` | Admin, Staff, Parent (own children) |
| `POST /enrollments/{id}/submit` | Admin, Staff |
| `POST /enrollments/{id}/approve` | Admin, Staff |
| `POST /enrollments/{id}/reject` | Admin, Staff |
| `POST /enrollments/{id}/cancel` | Admin |

## Error Handling

| Condition | HTTP | Body |
|---|---|---|
| Invalid transition | 400 | `{"error":"invalid_transition","from":"X","to":"Y","allowed":["A","B"]}` |
| Pending required docs | 400 | `{"error":"pending_required_documents","count":N}` |
| Period closed | 400 | `{"error":"enrollment_period_closed"}` |
| Out of window | 400 | `{"error":"enrollment_period_out_of_window"}` |
| Duplicate enrollment | 409 | `{"error":"enrollment_already_exists"}` |
| Overlapping periods | 400 | `{"error":"overlapping_periods"}` |
| Student transferred | 400 | `{"error":"student_transferred"}` |
| Cannot cancel approved | 400 | `{"error":"cannot_cancel_approved_enrollment"}` |

## File / Module Layout

| File | Path |
|---|---|
| Enrollment entity | `src/EduGestor.Core/Entities/Enrollment.cs` |
| EnrollmentPeriod entity | `src/EduGestor.Core/Entities/EnrollmentPeriod.cs` |
| EnrollmentStatus enum | `src/EduGestor.Core/Entities/EnrollmentStatus.cs` |
| DTOs | `src/EduGestor.Api/Contracts/EnrollmentDtos.cs` |
| IEnrollmentService + impl | `src/EduGestor.Infrastructure/Services/EnrollmentService.cs` |
| EnrollmentController | `src/EduGestor.Api/Controllers/EnrollmentController.cs` |

## Cross-Reference: Requirements → Design

| Requirement | Covered By |
|---|---|
| FR-001-003: Period CRUD | EnrollmentPeriod, Controller |
| FR-004-006: Enrollment CRUD | Enrollment entity, Controller |
| FR-007-010: State transitions | State Machine, EnrollmentService |
| FR-011/012: Entities | Domain Entities, AppDbContext |
| FR-013: State machine | State Machine Rules, CanTransition method |
| E1-E7: Edge cases | Error Handling table |
