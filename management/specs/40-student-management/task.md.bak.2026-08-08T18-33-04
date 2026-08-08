# Spec 40: Tasks — Student Management

## Task Checklist

### T4.1: Create Student entity
- [ ] Create `src/EduGestor.Core/Entities/Student.cs` implementing `ITenantScoped`
- [ ] Properties: Id, TenantId, Nome, DataNascimento, Cpf, Turma, AnoLetivo, Status, Observacoes, CreatedAt, UpdatedAt
- [ ] Create `StudentStatus` enum (Ativo, Inativo, Transferido)

### T4.2: Create StudentParent join entity
- [ ] Create `src/EduGestor.Core/Entities/StudentParent.cs`
- [ ] Composite key: StudentId + ParentId
- [ ] Navigation properties

### T4.3: Update AppDbContext
- [ ] Add `DbSet<Student>`, `DbSet<StudentParent>`
- [ ] Configure indexes: TenantId, TenantId+Nome, unique TenantId+Cpf (filtered)
- [ ] Configure StudentParent composite key

### T4.4: Create DTOs
- [ ] Create `src/EduGestor.Api/Contracts/StudentDtos.cs`
- [ ] StudentDto, CreateStudentRequest, UpdateStudentRequest, LinkParentRequest, ParentLinkDto, PagedResponse<T>

### T4.5: Create StudentService
- [ ] Create `src/EduGestor.Infrastructure/Services/StudentService.cs`
- [ ] GetAllAsync with search, turma, status filters, pagination
- [ ] Parent filter: only return linked students if user role=Parent
- [ ] GetByIdAsync with parents eager-loaded
- [ ] CreateAsync, UpdateAsync, DeleteAsync (soft delete)
- [ ] LinkParentAsync, UnlinkParentAsync with validation

### T4.6: Create StudentController
- [ ] Create `src/EduGestor.Api/Controllers/StudentController.cs`
- [ ] All endpoints with proper authorization attributes
- [ ] Return 403 for Parent accessing non-linked student

### T4.7: Add EF migration and verify
- [ ] `dotnet ef migrations add AddStudents`
- [ ] `dotnet build` — zero errors
- [ ] Test CRUD via Swagger
