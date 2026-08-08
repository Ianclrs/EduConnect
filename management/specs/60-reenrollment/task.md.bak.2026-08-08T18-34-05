# Spec 60: Tasks — Re-enrollment System

## Task Checklist

### T6.1: Create DTOs
- [ ] Create `src/EduGestor.Api/Contracts/ReenrollmentDtos.cs`
- [ ] CreateReenrollmentRequest, ReenrollmentDetailDto

### T6.2: Create ReenrollmentService
- [ ] Create `src/EduGestor.Infrastructure/Services/ReenrollmentService.cs`
- [ ] CreateAsync: validate eligibility, carry forward valid docs, flag expired
- [ ] GetAllAsync: query enrollments with prior-enrollment filter
- [ ] GetByIdAsync: detailed with document status summary
- [ ] ApproveAsync: update student AnoLetivo, set ApprovedAt

### T6.3: Create ReenrollmentController
- [ ] Create `src/EduGestor.Api/Controllers/ReenrollmentController.cs`
- [ ] POST /reenrollments, GET /reenrollments, GET /reenrollments/{id}, POST /reenrollments/{id}/approve

### T6.4: Add enrollment history endpoint
- [ ] Add `GET /students/{id}/enrollment-history` to StudentController
- [ ] Returns all Enrollment records for that student, ordered by AnoLetivo desc

### T6.5: Verify
- [ ] Create enrollment period → Create student → Enroll → Approve
- [ ] Create new period → Re-enroll same student
- [ ] Verify valid documents carried forward
- [ ] Verify expired documents flagged
