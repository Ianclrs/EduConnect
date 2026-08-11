namespace Ciclo.Core.Entities;

public class UserNotification
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    // Navigation
    public Notification Notification { get; set; } = null!;
    public User User { get; set; } = null!;
}
