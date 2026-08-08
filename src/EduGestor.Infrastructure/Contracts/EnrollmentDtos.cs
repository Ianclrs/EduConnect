namespace EduGestor.Infrastructure.Contracts;

public record CreateEnrollmentPeriodRequest(string Nome, DateTime DataInicio, DateTime DataFim, int AnoLetivo);

public record EnrollmentPeriodDto(Guid Id, string Nome, DateTime DataInicio, DateTime DataFim, int AnoLetivo, bool IsActive);

public record CreateEnrollmentRequest(Guid StudentId, Guid EnrollmentPeriodId);

public record EnrollmentDto(
    Guid Id,
    Guid StudentId,
    string StudentName,
    Guid PeriodId,
    string PeriodName,
    string Status,
    string? MotivoRejeicao,
    DateTime CreatedAt,
    DateTime? ApprovedAt);

public record RejectEnrollmentRequest(string Motivo);

public record InvalidTransitionResponse(string Error, string From, string To, List<string> Allowed);
