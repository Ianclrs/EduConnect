# Spec 40: Design — Student Management

## Design Approach

CRUD de estudantes com entidade `Student` implementando `ITenantScoped` para isolamento automático por tenant. Tabela de junção `StudentParent` para vínculo muitos-para-muitos entre Students e Users (role=Parent).

**Segurança:** Parent só acessa filhos vinculados — filtro implementado no serviço, não apenas no controller. Admin/Staff veem todos os alunos do tenant.

## Architecture Decisions

- **AD-001: Soft-delete** — `DELETE` seta `Status=Inativo` em vez de remover. Dados nunca são perdidos.
- **AD-002: StudentParent como join table** — evita poluir User com lista de filhos. Separação clara de responsabilidades.
- **AD-003: CPF index filtrado** — permite múltiplos nulos sem violar unicidade.

## Data Flow

```
GET /students?search=João&turma=A&page=1
  → StudentService.GetAllAsync(query, tenantId, userId, role)
    → if role=Parent: inner join StudentParent WHERE ParentId=userId
    → if role=Admin/Staff: no join, apenas filtro TenantId (global query filter)
    → search: WHERE Nome ILIKE '%João%'
    → turma filter: WHERE Turma = 'A'
    → ORDER BY Nome ASC, OFFSET 0 LIMIT 20
  → return PagedResponse<StudentDto>
```

## Domain Entities

### Student (Ciclo.Core/Entities/Student.cs)
```csharp
public class Student : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string? Cpf { get; set; }
    public string Turma { get; set; } = string.Empty;
    public int AnoLetivo { get; set; }
    public StudentStatus Status { get; set; } = StudentStatus.Ativo;
    public string? Observacoes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public ICollection<StudentParent> StudentParents { get; set; } = [];
}

public enum StudentStatus { Ativo = 0, Inativo = 1, Transferido = 2 }
```

### StudentParent (Ciclo.Core/Entities/StudentParent.cs)
```csharp
public class StudentParent
{
    public Guid StudentId { get; set; }
    public Guid ParentId { get; set; }  // FK to User (role=Parent)
    public Student Student { get; set; } = null!;
    public User Parent { get; set; } = null!;
}
```

## DTOs (Ciclo.Api/Contracts/StudentDtos.cs)
```csharp
public record StudentDto(Guid Id, string Nome, DateTime DataNascimento, string? Cpf, string Turma, int AnoLetivo, string Status, string? Observacoes, DateTime CreatedAt, List<ParentLinkDto> Parents);
public record CreateStudentRequest(string Nome, DateTime DataNascimento, string? Cpf, string Turma, int AnoLetivo, string? Observacoes);
public record UpdateStudentRequest(string Nome, DateTime DataNascimento, string? Cpf, string Turma, int AnoLetivo, string? Observacoes);
public record LinkParentRequest(Guid ParentId);
public record ParentLinkDto(Guid ParentId, string ParentName, string ParentEmail);
public record PagedResponse<T>(List<T> Items, int Total, int Page, int PageSize);
```

## StudentController

| Endpoint | Auth |
|---|---|
| `GET /students?search=&turma=&status=&page=1&pageSize=20` | Admin, Staff, Parent |
| `GET /students/{id}` | Admin, Staff, Parent (own children) |
| `POST /students` | Admin, Staff |
| `PUT /students/{id}` | Admin, Staff |
| `DELETE /students/{id}` | Admin only |
| `POST /students/{id}/link-parent` | Admin, Staff |
| `DELETE /students/{id}/link-parent/{parentId}` | Admin, Staff |

## AppDbContext Updates
```csharp
public DbSet<Student> Students { get; set; }
public DbSet<StudentParent> StudentParents { get; set; }

builder.Entity<Student>(e => {
    e.HasIndex(s => s.TenantId);
    e.HasIndex(s => new { s.TenantId, s.Nome });
    e.HasIndex(s => new { s.TenantId, s.Cpf }).IsUnique().HasFilter("[Cpf] IS NOT NULL");
});
builder.Entity<StudentParent>(e => {
    e.HasKey(sp => new { sp.StudentId, sp.ParentId });
});
```

## Error Handling

| Condition | HTTP | Body |
|---|---|---|
| Student not found (wrong tenant) | 404 | `{"error":"student_not_found"}` |
| Parent not linked to student | 403 | `{"error":"not_linked_to_student"}` |
| CPF already exists | 409 | `{"error":"cpf_already_exists"}` |
| Parent from different tenant | 400 | `{"error":"parent_belongs_to_different_tenant"}` |
| User is not a Parent | 400 | `{"error":"user_is_not_parent"}` |
| Student has active enrollment | 409 | `{"error":"student_has_active_enrollment"}` |

## File / Module Layout

| File | Path |
|---|---|
| Student entity | `src/Ciclo.Core/Entities/Student.cs` |
| StudentParent entity | `src/Ciclo.Core/Entities/StudentParent.cs` |
| StudentStatus enum | `src/Ciclo.Core/Entities/StudentStatus.cs` |
| Student DTOs | `src/Ciclo.Api/Contracts/StudentDtos.cs` |
| IStudentService + impl | `src/Ciclo.Infrastructure/Services/StudentService.cs` |
| StudentController | `src/Ciclo.Api/Controllers/StudentController.cs` |

## Cross-Reference: Requirements → Design

| Requirement | Covered By |
|---|---|
| FR-001/002 List/Detail | StudentService.GetAllAsync/GetByIdAsync, Controller |
| FR-003/004 Create/Update | StudentService, DTOs |
| FR-005 Soft-delete | StudentService.DeleteAsync |
| FR-006/007 Link/Unlink | StudentService.LinkParentAsync/UnlinkParentAsync |
| FR-008/009 Entities | Domain Entities section, AppDbContext |
| FR-010 PagedResponse | DTOs |
| E1-E7 Edge cases | Error Handling table |
