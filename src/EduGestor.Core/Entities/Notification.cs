using EduGestor.Core.Interfaces;

namespace EduGestor.Core.Entities;

public class Notification : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public NotificationType Tipo { get; set; } = NotificationType.Geral;
    public Guid? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ICollection<UserNotification> UserNotifications { get; set; } = [];
}

public enum NotificationType
{
    Geral = 0,
    DocumentoPendente = 1,
    Reuniao = 2,
    Matricula = 3,
    Outro = 4
}
