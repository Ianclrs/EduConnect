# Spec 5: Design — Enrollment System

## Domain Entities

### EnrollmentPeriod (EduGestor.Core/Entities/EnrollmentPeriod.cs)
```csharp
public class EnrollmentPeriod : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Nome { get; set; } = string.Empty;       // "Matrículas 2027"
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
    Rascunho = 0,
    Pendente = 1,
    DocumentacaoPendente = 2,
    Aprovado = 3,
    Rejeitado = 4,
    Cancelado = 5
}
```

### State Machine Rules
```
Rascunho        → Pendente               (submit)
Pendente        → DocumentacaoPendente    (docs pending)
Pendente        → Aprovado                (approve, all docs ok)
Pendente        → Rejeitado               (reject)
Pendente        → Cancelado                (cancel)
DocumentacaoPendente → Pendente           (docs completed)
DocumentacaoPendente → Aprovado           (approve after docs)
DocumentacaoPendente → Rejeitado          (reject)
DocumentacaoPendente → Cancelado          (cancel)
```

Invalid transitions return `400 Bad Request` with error message.

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

## EnrollmentService

- Validate state transitions before any status change.
- On approve: set `ApprovedAt = DateTime.UtcNow`, set `Student.Status = Ativo`, update `Student.AnoLetivo`.
- On reject: require `MotivoRejeicao` non-empty.
- `SubmitAsync`: Rascunho → Pendente.

## AppDbContext Updates

```csharp
public DbSet<EnrollmentPeriod> EnrollmentPeriods { get; set; }
public DbSet<Enrollment> Enrollments { get; set; }

builder.Entity<Enrollment>(e => {
    e.HasIndex(en => en.TenantId);
    e.HasIndex(en => en.StudentId);
    e.HasIndex(en => new { en.TenantId, en.Status });
});
builder.Entity<EnrollmentPeriod>(e => {
    e.HasIndex(ep => ep.TenantId);
});
```

## File Locations

| File | Path |
|---|---|
| Enrollment entity | `src/EduGestor.Core/Entities/Enrollment.cs` |
| EnrollmentPeriod entity | `src/EduGestor.Core/Entities/EnrollmentPeriod.cs` |
| EnrollmentStatus enum | `src/EduGestor.Core/Entities/EnrollmentStatus.cs` |
| DTOs | `src/EduGestor.Api/Contracts/EnrollmentDtos.cs` |
| IEnrollmentService + impl | `src/EduGestor.Infrastructure/Services/EnrollmentService.cs` |
| EnrollmentController | `src/EduGestor.Api/Controllers/EnrollmentController.cs` |
