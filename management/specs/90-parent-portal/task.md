# Spec 90: Tasks — Parent Portal API

## Task Checklist

### T9.1: Create Parent DTOs
- [ ] Create `src/EduGestor.Api/Contracts/ParentDtos.cs`
- [ ] ParentDashboardDto, ChildSummaryDto, ChildDetailDto, GradeDto

### T9.2: Create ParentService
- [ ] Create `src/EduGestor.Infrastructure/Services/ParentService.cs`
- [ ] GetDashboardAsync: aggregate children, notifications, documents, enrollments
- [ ] GetChildrenAsync: query StudentParent join
- [ ] GetChildDetailAsync: student + documents + enrollment + grades
- [ ] UploadDocumentAsync: validate parent-child link, delegate to DocumentService
- [ ] Every method: verify parent-child link first

### T9.3: Create ParentController
- [ ] Create `src/EduGestor.Api/Controllers/ParentController.cs` with `[Authorize(Roles = "Parent")]`
- [ ] All endpoints as defined in design

### T9.4: Create ForbiddenException
- [ ] Create `src/EduGestor.Api/Middleware/ForbiddenException.cs` with 403 mapping

### T9.5: Verify
- [ ] Parent login → only sees linked children
- [ ] Parent tries to access unlinked child → 403
- [ ] Parent uploads document for child → appears in Admin document list
- [ ] Dashboard shows correct counts
