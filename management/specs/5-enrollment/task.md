# Spec 5: Tasks — Enrollment System

## Task Checklist

### T5.1: Create EnrollmentPeriod entity
- [ ] Create `src/EduGestor.Core/Entities/EnrollmentPeriod.cs` implementing `ITenantScoped`
- [ ] Properties: Id, TenantId, Nome, DataInicio, DataFim, AnoLetivo, IsActive, CreatedAt
- [ ] Navigation to Enrollments

### T5.2: Create Enrollment entity
- [ ] Create `src/EduGestor.Core/Entities/Enrollment.cs` implementing `ITenantScoped`
- [ ] Properties: Id, TenantId, StudentId, EnrollmentPeriodId, Status, MotivoRejeicao, CreatedAt, ApprovedAt
- [ ] Create `EnrollmentStatus` enum with all 6 states
- [ ] Navigations: Student, Period, Tenant

### T5.3: Create DTOs
- [ ] Create `src/EduGestor.Api/Contracts/EnrollmentDtos.cs`
- [ ] All request/response records

### T5.4: Create EnrollmentService
- [ ] Create `src/EduGestor.Infrastructure/Services/EnrollmentService.cs`
- [ ] State machine validation: helper method `CanTransition(from, to)`
- [ ] CreateAsync, SubmitAsync, ApproveAsync, RejectAsync, CancelAsync
- [ ] On approve: update Student.Status = Ativo, Student.AnoLetivo = Period.AnoLetivo

### T5.5: Create EnrollmentController
- [ ] Full CRUD for EnrollmentPeriod
- [ ] Full CRUD for Enrollment with status transitions

### T5.6: Update AppDbContext
- [ ] Add DbSets and indexes

### T5.7: Add EF migration and verify
- [ ] `dotnet ef migrations add AddEnrollments`
- [ ] Test full workflow via Swagger
