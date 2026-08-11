using Ciclo.Core.Entities;

namespace Ciclo.Infrastructure.Contracts;

public record CreateNotificationRequest(string Titulo, string Mensagem, NotificationType Tipo, Guid? ReferenceId, List<Guid>? UserIds);

public record BroadcastNotificationRequest(string Titulo, string Mensagem, NotificationType Tipo, Guid? ReferenceId);

public record NotificationDto(Guid Id, Guid UserNotificationId, string Titulo, string Mensagem, string Tipo, Guid? ReferenceId, bool IsRead, DateTime CreatedAt);
