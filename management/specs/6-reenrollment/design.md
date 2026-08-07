# Spec 6: Design — Re-enrollment System

## Domain Entities

Re-enrollment reuses the `Enrollment` entity from Spec 5. The difference is semantic and handled at the service/controller level:

- A re-enrollment is an `Enrollment` where the `StudentId` already has a prior approved enrollment.
- The system identifies documents that are still valid (within `ValidadeMeses`) and carries them over.

### ReenrollmentRequest (extends enrollment)
```csharp
public record CreateReenrollmentRequest(Guid StudentId, Guid EnrollmentPeriodId);
// Same structure as CreateEnrollmentRequest — differentiated at controller level
```

The key difference from enrollment:
1. Auto-validates that student exists and has a prior enrollment.
2. Auto-carries forward documents that are still valid.
3. Flags expired documents for renewal.

## ReenrollmentController

All endpoints under `/reenrollments`, all require `Admin` or `Staff`.

| Endpoint | Description |
|---|---|
| `POST /reenrollments` | Create re-enrollment (auto-validates student eligibility) |
| `GET /reenrollments?status=&periodId=` | List re-enrollments |
| `GET /reenrollments/{id}` | Get re-enrollment detail with document status |
| `POST /reenrollments/{id}/approve` | Approve and update student AnoLetivo |

## ReenrollmentService

```csharp
public interface IReenrollmentService
{
    Task<EnrollmentDto> CreateAsync(CreateReenrollmentRequest request, Guid tenantId);
    Task<List<EnrollmentDto>> GetAllAsync(Guid tenantId, EnrollmentStatus? status, Guid? periodId);
    Task<EnrollmentDetailDto> GetByIdAsync(Guid id, Guid tenantId);
    Task<EnrollmentDto> ApproveAsync(Guid id, Guid tenantId);
}
```

### CreateAsync Logic
1. Verify student exists and belongs to tenant.
2. Verify student has at least one prior approved enrollment.
3. Verify enrollment period is active.
4. Create Enrollment entity with status=Pendente.
5. Auto-carry valid documents: copy `Document` IDs that are `Aprovado` and whose `DataValidade > DateTime.UtcNow`.
6. Identify expired documents: set status=DocumentacaoPendente if any required docs expired/missing.

## Document Carry-Forward

```csharp
// Pseudo-code in CreateAsync:
var previousDocs = await _db.Documents
    .Where(d => d.StudentId == request.StudentId && d.Status == DocumentStatus.Aprovado)
    .ToListAsync();

foreach (var doc in previousDocs)
{
    if (doc.DataValidade == null || doc.DataValidade > DateTime.UtcNow)
    {
        // Document still valid — copied to new enrollment implicitly
        // (we reference it in EnrollmentDocument join table if using one)
    }
}

// Check for expired/missing required doc types
var requiredTypes = await _db.DocumentTypes
    .Where(dt => dt.TenantId == tenantId && dt.IsRequired && dt.IsActive)
    .ToListAsync();

var missingTypes = requiredTypes
    .Where(rt => !validDocs.Any(vd => vd.DocumentTypeId == rt.Id))
    .ToList();

if (missingTypes.Any())
    enrollment.Status = EnrollmentStatus.DocumentacaoPendente;
```

## File Locations

| File | Path |
|---|---|
| Reenrollment DTOs | `src/EduGestor.Api/Contracts/ReenrollmentDtos.cs` |
| IReenrollmentService + impl | `src/EduGestor.Infrastructure/Services/ReenrollmentService.cs` |
| ReenrollmentController | `src/EduGestor.Api/Controllers/ReenrollmentController.cs` |
