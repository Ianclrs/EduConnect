namespace EduGestor.Infrastructure.Contracts;

public record ParentDashboardDto(
    int TotalChildren,
    int UnreadNotifications,
    int PendingDocuments,
    int ActiveEnrollments,
    List<ChildSummaryDto> Children);

public record ChildSummaryDto(
    Guid StudentId,
    string Nome,
    string Turma,
    int AnoLetivo,
    string? EnrollmentStatus,
    int PendingDocuments);

public record ChildDetailDto(
    StudentDto Student,
    List<DocumentDto> Documents,
    EnrollmentDto? CurrentEnrollment,
    List<GradeDto> Grades);

public record GradeDto(string Disciplina, decimal? Nota, string? Observacoes);
