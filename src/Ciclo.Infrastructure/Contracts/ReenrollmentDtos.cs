namespace Ciclo.Infrastructure.Contracts;

public record CreateReenrollmentRequest(Guid StudentId, Guid EnrollmentPeriodId);

public record ReenrollmentDetailDto(
    Guid Id,
    Guid StudentId,
    string StudentName,
    Guid PeriodId,
    string PeriodName,
    string Status,
    string? MotivoRejeicao,
    DateTime CreatedAt,
    DateTime? ApprovedAt,
    List<CarriedDocumentDto> CarriedDocuments,
    List<string> MissingDocumentTypes);

public record CarriedDocumentDto(Guid DocumentId, string DocumentType, DateTime? DataValidade, bool IsExpired);
