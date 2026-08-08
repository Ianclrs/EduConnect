# Spec 90: Design — Parent Portal API

## Design Philosophy

The Parent Portal API is NOT a separate set of entities. It reuses existing entities (Student, Document, Enrollment, Notification) but provides a simplified parent-specific view with enforced access control: parents only see children linked via `StudentParent`.

## ParentController

All endpoints are under `/parent` and require `[Authorize(Roles = "Parent")]`.

| Endpoint | Description |
|---|---|
| `GET /parent/dashboard` | Summary: children count, unread notifications, pending documents, active enrollments |
| `GET /parent/children` | List of linked children (StudentDto) |
| `GET /parent/children/{id}` | Child detail with documents, enrollment status, grades |
| `GET /parent/children/{id}/documents` | Child's documents with status |
| `POST /parent/children/{id}/documents/upload` | Upload document for child |
| `GET /parent/children/{id}/grades` | Child's grades/performance (placeholder) |

## ParentDashboardDto

```csharp
public record ParentDashboardDto(
    int TotalChildren,
    int UnreadNotifications,
    int PendingDocuments,
    int ActiveEnrollments,
    List<ChildSummaryDto> Children
);

public record ChildSummaryDto(
    Guid StudentId,
    string Nome,
    string Turma,
    int AnoLetivo,
    string EnrollmentStatus,
    int PendingDocuments
);
```

## ParentService (EduGestor.Infrastructure/Services/ParentService.cs)

```csharp
public interface IParentService
{
    Task<ParentDashboardDto> GetDashboardAsync(Guid parentId, Guid tenantId);
    Task<List<StudentDto>> GetChildrenAsync(Guid parentId, Guid tenantId);
    Task<ChildDetailDto> GetChildDetailAsync(Guid parentId, Guid studentId, Guid tenantId);
    Task<List<DocumentDto>> GetChildDocumentsAsync(Guid parentId, Guid studentId, Guid tenantId);
    Task<DocumentDto> UploadDocumentAsync(Guid parentId, Guid studentId, IFormFile file, Guid documentTypeId, Guid tenantId);
    Task<List<GradeDto>> GetChildGradesAsync(Guid parentId, Guid studentId, Guid tenantId);
}
```

## Access Control Pattern

Every method follows this pattern:
```csharp
// 1. Verify parent-child link exists
var link = await _db.StudentParents
    .FirstOrDefaultAsync(sp => sp.StudentId == studentId && sp.ParentId == parentId);
if (link == null)
    throw new ForbiddenException("You are not linked to this student.");

// 2. Proceed with query filtered by studentId + tenantId
```

## File Locations

| File | Path |
|---|---|
| Parent DTOs | `src/EduGestor.Api/Contracts/ParentDtos.cs` |
| IParentService + impl | `src/EduGestor.Infrastructure/Services/ParentService.cs` |
| ParentController | `src/EduGestor.Api/Controllers/ParentController.cs` |
| ForbiddenException | `src/EduGestor.Api/Middleware/ForbiddenException.cs` |
