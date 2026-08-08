# Spec 60: Design — Re-enrollment System

## Design Approach

Rematrícula **reutiliza a entidade `Enrollment` da Spec 50**. A diferença é semântica: uma rematrícula é um `Enrollment` onde o aluno já tem matrícula anterior aprovada. O serviço valida elegibilidade e automaticamente carrega documentos válidos da matrícula anterior.

## Architecture Decisions

- **AD-001: Reuso de entidade Enrollment** — evita duplicação de lógica. A diferenciação está no controller (`/reenrollments` vs `/enrollments`) e nas validações do serviço.
- **AD-002: Document carry-forward** — documentos aprovados com `DataValidade > UtcNow` são automaticamente referenciados na nova matrícula (sem duplicar arquivos).

## Data Flow: Create Reenrollment

```
POST /reenrollments { studentId, enrollmentPeriodId }
  → ReenrollmentService.CreateAsync()
    → Validate: student exists, belongs to tenant
    → Validate: student has ≥1 prior approved Enrollment
    → Validate: period is active
    → Create Enrollment(status=Pendente)
    → Fetch student's approved Documents WHERE status=Aprovado
       → Filter: DataValidade == null OR DataValidade > UtcNow
       → Reference valid docs in EnrollmentDocument join (if table exists)
    → Fetch required DocumentTypes WHERE IsRequired AND IsActive
       → Check each required type has a valid doc
       → If missing: status = DocumentacaoPendente
    → SaveChangesAsync()
```

## ReenrollmentController

All under `/reenrollments`, require `Admin` or `Staff`.

| Endpoint | Description |
|---|---|
| `POST /reenrollments` | Create re-enrollment |
| `GET /reenrollments?status=&periodId=` | List re-enrollments |
| `GET /reenrollments/{id}` | Detail with document carry-forward status |
| `POST /reenrollments/{id}/approve` | Approve and update AnoLetivo |
| `POST /reenrollments/{id}/reject` | Reject with motivo |

## ReenrollmentService

```csharp
public interface IReenrollmentService
{
    Task<EnrollmentDto> CreateAsync(CreateReenrollmentRequest request, Guid tenantId);
    Task<PagedResponse<EnrollmentDto>> GetAllAsync(Guid tenantId, EnrollmentStatus? status, Guid? periodId, int page, int pageSize);
    Task<EnrollmentDetailDto> GetByIdAsync(Guid id, Guid tenantId);
    Task<EnrollmentDto> ApproveAsync(Guid id, Guid tenantId);
    Task<EnrollmentDto> RejectAsync(Guid id, string motivo, Guid tenantId);
}
```

### CreateAsync Logic
1. Verify student exists, belongs to tenant, not Inativo/Transferido.
2. Verify student has ≥1 prior `Enrollment` with `Status = Aprovado`.
3. Verify period active and within window.
4. Create `Enrollment` with `Status = Pendente`.
5. Query student's `Document`s where `Status = Aprovado`.
6. Filter valid: `DataValidade == null || DataValidade > UtcNow`.
7. Query `DocumentType`s where `IsRequired && IsActive` for tenant.
8. If any required type has no valid document → `Status = DocumentacaoPendente`.

### Document Carry-Forward
```csharp
var validDocs = await _db.Documents
    .Where(d => d.StudentId == request.StudentId && d.Status == DocumentStatus.Aprovado
        && (d.DataValidade == null || d.DataValidade > DateTime.UtcNow))
    .ToListAsync();

var requiredTypes = await _db.DocumentTypes
    .Where(dt => dt.TenantId == tenantId && dt.IsRequired && dt.IsActive)
    .ToListAsync();

var missingTypes = requiredTypes
    .Where(rt => !validDocs.Any(vd => vd.DocumentTypeId == rt.Id))
    .ToList();

if (missingTypes.Any())
    enrollment.Status = EnrollmentStatus.DocumentacaoPendente;
```

## Error Handling

| Condition | HTTP | Body |
|---|---|---|
| No prior enrollment | 400 | `{"error":"student_has_no_prior_enrollment"}` |
| Duplicate reenrollment | 409 | `{"error":"reenrollment_already_exists"}` |
| Period closed | 400 | `{"error":"enrollment_period_closed"}` |

## File / Module Layout

| File | Path |
|---|---|
| Reenrollment DTOs | `src/EduGestor.Api/Contracts/ReenrollmentDtos.cs` |
| IReenrollmentService + impl | `src/EduGestor.Infrastructure/Services/ReenrollmentService.cs` |
| ReenrollmentController | `src/EduGestor.Api/Controllers/ReenrollmentController.cs` |

## Cross-Reference: Requirements → Design

| Requirement | Covered By |
|---|---|
| FR-001/002/003: CRUD | ReenrollmentService, Controller |
| FR-004: Document carry-forward | Document Carry-Forward section |
| FR-005/006: Approve/Reject | ReenrollmentService |
| FR-007: History | Added to StudentController (Spec 40) |
| E1-E5: Edge cases | Error Handling table |
