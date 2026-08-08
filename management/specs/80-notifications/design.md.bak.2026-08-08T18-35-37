# Spec 80: Design — Notification System

## Domain Entities

### Notification (EduGestor.Core/Entities/Notification.cs)
```csharp
public class Notification : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public NotificationType Tipo { get; set; } = NotificationType.Geral;
    public Guid? ReferenceId { get; set; }         // e.g., StudentId, MeetingId
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public ICollection<UserNotification> UserNotifications { get; set; } = [];
}

public enum NotificationType
{
    Geral = 0,            // comunicado geral do colégio
    DocumentoPendente = 1, // documentos pendentes/vencidos
    Reuniao = 2,          // convocação para reunião
    Matricula = 3,        // lembrete de matrícula/rematrícula
    Outro = 4
}
```

### UserNotification — per-user read tracking
```csharp
public class UserNotification
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    public Notification Notification { get; set; } = null!;
    public User User { get; set; } = null!;
}
```

## DTOs

```csharp
public record CreateNotificationRequest(string Titulo, string Mensagem, NotificationType Tipo, Guid? ReferenceId, List<Guid>? UserIds);
public record BroadcastNotificationRequest(string Titulo, string Mensagem, NotificationType Tipo, Guid? ReferenceId);
public record NotificationDto(Guid Id, string Titulo, string Mensagem, string Tipo, Guid? ReferenceId, bool IsRead, DateTime CreatedAt);
public record UnreadCountDto(int Count);
```

## NotificationController

| Endpoint | Auth |
|---|---|
| `POST /notifications` | Admin, Staff |
| `POST /notifications/broadcast` | Admin |
| `POST /notifications/by-student/{studentId}` | Admin, Staff |
| `GET /notifications?unreadOnly=false&page=1&pageSize=20` | All authenticated |
| `PUT /notifications/{id}/read` | All authenticated |
| `PUT /notifications/read-all` | All authenticated |
| `GET /notifications/unread-count` | All authenticated |

## NotificationService

- `CreateAsync`: create Notification entity + UserNotification for each userId.
- `BroadcastAsync`: create Notification + UserNotification for ALL users with role=Parent in the tenant.
- `SendByStudentAsync`: create Notification + UserNotification for parents linked to the student.
- `GetForUserAsync`: query UserNotifications for current user, join with Notification, ordered by CreatedAt desc.
- `MarkReadAsync`: set IsRead=true, ReadAt=DateTime.UtcNow for current user's UserNotification.
- `MarkAllReadAsync`: update all unread for current user.

## Automatic Notifications (integration with Spec 70)

When a document is rejected via `DocumentService.VerifyAsync`, the service raises an event. A background handler (or direct call) creates a notification:
```csharp
// In DocumentService.VerifyAsync after rejection:
await _notificationService.SendByStudentAsync(new CreateNotificationByStudentRequest(
    document.TenantId, document.StudentId,
    $"Documento rejeitado: {documentType.Nome}",
    $"O documento '{document.NomeArquivo}' foi rejeitado. Motivo: {motivo}",
    NotificationType.DocumentoPendente,
    document.Id
));
```

## File Locations

| File | Path |
|---|---|
| Notification entity | `src/EduGestor.Core/Entities/Notification.cs` |
| UserNotification entity | `src/EduGestor.Core/Entities/UserNotification.cs` |
| NotificationType enum | `src/EduGestor.Core/Entities/NotificationType.cs` |
| DTOs | `src/EduGestor.Api/Contracts/NotificationDtos.cs` |
| INotificationService + impl | `src/EduGestor.Infrastructure/Services/NotificationService.cs` |
| NotificationController | `src/EduGestor.Api/Controllers/NotificationController.cs` |
