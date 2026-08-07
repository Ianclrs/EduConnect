# Spec 4: Design — Student Management

## Domain Entities

### Student (EduGestor.Core/Entities/Student.cs)
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

public enum StudentStatus
{
    Ativo = 0,
    Inativo = 1,
    Transferido = 2
}
```

### StudentParent — join table (EduGestor.Core/Entities/StudentParent.cs)
```csharp
public class StudentParent
{
    public Guid StudentId { get; set; }
    public Guid ParentId { get; set; }  // FK to User (role=Parent)
    public Student Student { get; set; } = null!;
    public User Parent { get; set; } = null!;
}
```

## DTOs

```csharp
public record StudentDto(
    Guid Id, string Nome, DateTime DataNascimento, string? Cpf,
    string Turma, int AnoLetivo, string Status, string? Observacoes,
    DateTime CreatedAt, List<ParentLinkDto> Parents);

public record CreateStudentRequest(
    string Nome, DateTime DataNascimento, string? Cpf,
    string Turma, int AnoLetivo, string? Observacoes);

public record UpdateStudentRequest(
    string Nome, DateTime DataNascimento, string? Cpf,
    string Turma, int AnoLetivo, string? Observacoes);

public record LinkParentRequest(Guid ParentId);
public record ParentLinkDto(Guid ParentId, string ParentName, string ParentEmail);

public record PagedResponse<T>(List<T> Items, int Total, int Page, int PageSize);
```

## StudentController

| Endpoint | Auth |
|---|---|
| `GET /students?search=&turma=&status=&page=1&pageSize=20` | Admin, Staff, Parent |
| `GET /students/{id}` | Admin, Staff, Parent (own children only) |
| `POST /students` | Admin, Staff |
| `PUT /students/{id}` | Admin, Staff |
| `DELETE /students/{id}` | Admin only |
| `POST /students/{id}/link-parent` | Admin, Staff |
| `DELETE /students/{id}/link-parent/{parentId}` | Admin, Staff |

## StudentService (EduGestor.Infrastructure/Services/StudentService.cs)

- `GetAllAsync(query, tenantId, userId?, userRole?)` — paginated, filtered. Parents only see linked children.
- `GetByIdAsync(id, tenantId)` — with parents included.
- `CreateAsync(request, tenantId)` — validate, create, save.
- `UpdateAsync(id, request, tenantId)` — find, update, save.
- `DeleteAsync(id, tenantId)` — set Status=Inativo.
- `LinkParentAsync(studentId, parentId, tenantId)` — validate parent exists and belongs to same tenant.
- `UnlinkParentAsync(studentId, parentId, tenantId)` — remove join.

## AppDbContext Updates

```csharp
public DbSet<Student> Students { get; set; }
public DbSet<StudentParent> StudentParents { get; set; }

// OnModelCreating:
builder.Entity<Student>(e => {
    e.HasIndex(s => s.TenantId);
    e.HasIndex(s => new { s.TenantId, s.Nome });
    e.HasIndex(s => new { s.TenantId, s.Cpf }).IsUnique().HasFilter("[Cpf] IS NOT NULL");
});
builder.Entity<StudentParent>(e => {
    e.HasKey(sp => new { sp.StudentId, sp.ParentId });
});
```

## File Locations

| File | Path |
|---|---|
| Student entity | `src/EduGestor.Core/Entities/Student.cs` |
| StudentParent entity | `src/EduGestor.Core/Entities/StudentParent.cs` |
| StudentStatus enum | `src/EduGestor.Core/Entities/StudentStatus.cs` |
| Student DTOs | `src/EduGestor.Api/Contracts/StudentDtos.cs` |
| IStudentService + impl | `src/EduGestor.Infrastructure/Services/StudentService.cs` |
| StudentController | `src/EduGestor.Api/Controllers/StudentController.cs` |
