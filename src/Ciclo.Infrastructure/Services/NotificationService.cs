using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ciclo.Core.Entities;
using Ciclo.Infrastructure.Contracts;
using Ciclo.Infrastructure.Data;
using Ciclo.Infrastructure.Email;

namespace Ciclo.Infrastructure.Services;

public interface INotificationService
{
    Task<NotificationDto> CreateAsync(CreateNotificationRequest request, Guid tenantId);
    Task<(int RecipientCount, NotificationDto Notification)> BroadcastAsync(BroadcastNotificationRequest request, Guid tenantId);
    Task<int> SendByStudentAsync(Guid studentId, string titulo, string mensagem, NotificationType tipo, Guid? referenceId, Guid tenantId);
    Task<PagedResponse<NotificationDto>> GetForUserAsync(Guid userId, bool unreadOnly, int page, int pageSize);
    Task MarkReadAsync(Guid userNotificationId, Guid userId);
    Task<int> MarkAllReadAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
}

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(AppDbContext db, IEmailSender emailSender, ILogger<NotificationService> logger)
    {
        _db = db;
        _emailSender = emailSender;
        _logger = logger;
    }

    // ── FR-001: Create Notification ──────────────────────────────────

    public async Task<NotificationDto> CreateAsync(CreateNotificationRequest request, Guid tenantId)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo) || request.Titulo.Length > 200)
            throw new NotificationException("titulo_required_max_200", 400);

        if (string.IsNullOrWhiteSpace(request.Mensagem) || request.Mensagem.Length > 2000)
            throw new NotificationException("mensagem_required_max_2000", 400);

        var userIds = request.UserIds;
        if (userIds is null || userIds.Count == 0)
            throw new NotificationException("user_ids_required", 400);

        // Validate all userIds belong to the same tenant
        var validUserIds = await _db.Users
            .IgnoreQueryFilters()
            .Where(u => userIds.Contains(u.Id) && u.TenantId == tenantId)
            .Select(u => u.Id)
            .ToListAsync();

        if (validUserIds.Count != userIds.Count)
            throw new NotificationException("users_must_belong_to_same_tenant", 400);

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Titulo = request.Titulo,
            Mensagem = request.Mensagem,
            Tipo = request.Tipo,
            ReferenceId = request.ReferenceId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);

        var userNotifications = validUserIds.Select(uid => new UserNotification
        {
            Id = Guid.NewGuid(),
            NotificationId = notification.Id,
            UserId = uid
        }).ToList();

        _db.UserNotifications.AddRange(userNotifications);
        await _db.SaveChangesAsync();

        // Send emails (fire-and-forget, don't revert on failure)
        _ = SendEmailsAsync(validUserIds, request.Titulo, request.Mensagem);

        return MapToDto(notification, false);
    }

    // ── FR-002: Broadcast to Parents ─────────────────────────────────

    public async Task<(int RecipientCount, NotificationDto Notification)> BroadcastAsync(
        BroadcastNotificationRequest request, Guid tenantId)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo) || request.Titulo.Length > 200)
            throw new NotificationException("titulo_required_max_200", 400);

        if (string.IsNullOrWhiteSpace(request.Mensagem) || request.Mensagem.Length > 2000)
            throw new NotificationException("mensagem_required_max_2000", 400);

        var parentIds = await _db.Users
            .Where(u => u.TenantId == tenantId && u.Role == UserRole.Parent && u.IsActive)
            .Select(u => u.Id)
            .ToListAsync();

        if (parentIds.Count == 0)
            return (0, new NotificationDto(Guid.Empty, Guid.Empty, string.Empty, string.Empty, string.Empty, null, false, DateTime.MinValue));

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Titulo = request.Titulo,
            Mensagem = request.Mensagem,
            Tipo = request.Tipo,
            ReferenceId = request.ReferenceId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);

        var userNotifications = parentIds.Select(uid => new UserNotification
        {
            Id = Guid.NewGuid(),
            NotificationId = notification.Id,
            UserId = uid
        }).ToList();

        _db.UserNotifications.AddRange(userNotifications);

        await _db.SaveChangesAsync();

        _ = SendEmailsAsync(parentIds, request.Titulo, request.Mensagem);

        return (parentIds.Count, MapToDto(notification, false));
    }

    // ── FR-003: Notify by Student ────────────────────────────────────

    public async Task<int> SendByStudentAsync(Guid studentId, string titulo, string mensagem,
        NotificationType tipo, Guid? referenceId, Guid tenantId)
    {
        if (string.IsNullOrWhiteSpace(titulo) || titulo.Length > 200)
            throw new NotificationException("titulo_required_max_200", 400);

        if (string.IsNullOrWhiteSpace(mensagem) || mensagem.Length > 2000)
            throw new NotificationException("mensagem_required_max_2000", 400);

        // Verify student belongs to tenant (prevents cross-tenant data leak)
        var studentExists = await _db.Students
            .IgnoreQueryFilters()
            .AnyAsync(s => s.Id == studentId && s.TenantId == tenantId);

        if (!studentExists)
            throw new NotificationException("student_not_found", 404);

        var parentIds = await _db.StudentParents
            .Where(sp => sp.StudentId == studentId)
            .Select(sp => sp.ParentId)
            .ToListAsync();

        // Aluno sem pais vinculados → count=0 (not an error)
        if (parentIds.Count == 0)
            return 0;

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Titulo = titulo,
            Mensagem = mensagem,
            Tipo = tipo,
            ReferenceId = referenceId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);

        var userNotifications = parentIds.Select(uid => new UserNotification
        {
            Id = Guid.NewGuid(),
            NotificationId = notification.Id,
            UserId = uid
        }).ToList();

        _db.UserNotifications.AddRange(userNotifications);
        await _db.SaveChangesAsync();

        _ = SendEmailsAsync(parentIds, titulo, mensagem);

        return parentIds.Count;
    }

    // ── FR-004: List User Notifications ──────────────────────────────

    public async Task<PagedResponse<NotificationDto>> GetForUserAsync(
        Guid userId, bool unreadOnly, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize > 100) pageSize = 100;

        var query = _db.UserNotifications
            .Include(un => un.Notification)
            .Where(un => un.UserId == userId);

        if (unreadOnly)
            query = query.Where(un => !un.IsRead);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(un => un.Notification.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(un => new NotificationDto(
                un.Notification.Id,
                un.Id,
                un.Notification.Titulo,
                un.Notification.Mensagem,
                un.Notification.Tipo.ToString(),
                un.Notification.ReferenceId,
                un.IsRead,
                un.Notification.CreatedAt))
            .ToListAsync();

        return new PagedResponse<NotificationDto>(items, total, page, pageSize);
    }

    // ── FR-005: Mark Single as Read ──────────────────────────────────

    public async Task MarkReadAsync(Guid userNotificationId, Guid userId)
    {
        var un = await _db.UserNotifications
            .FirstOrDefaultAsync(u => u.Id == userNotificationId && u.UserId == userId);

        if (un is null)
            throw new NotificationException("notification_not_found", 404);

        // Already read → idempotent
        if (!un.IsRead)
        {
            un.IsRead = true;
            un.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    // ── FR-006: Mark All as Read ─────────────────────────────────────

    public async Task<int> MarkAllReadAsync(Guid userId)
    {
        var unread = await _db.UserNotifications
            .Where(un => un.UserId == userId && !un.IsRead)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var un in unread)
        {
            un.IsRead = true;
            un.ReadAt = now;
        }

        if (unread.Count > 0)
            await _db.SaveChangesAsync();

        return unread.Count;
    }

    // ── FR-007: Unread Count ─────────────────────────────────────────

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _db.UserNotifications
            .CountAsync(un => un.UserId == userId && !un.IsRead);
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private static NotificationDto MapToDto(Notification n, bool isRead)
    {
        return new NotificationDto(n.Id, Guid.Empty, n.Titulo, n.Mensagem, n.Tipo.ToString(), n.ReferenceId, isRead, n.CreatedAt);
    }

    private async Task SendEmailsAsync(List<Guid> userIds, string titulo, string mensagem)
    {
        try
        {
            var users = await _db.Users
                .IgnoreQueryFilters()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Email, u.Name })
                .ToListAsync();

            foreach (var user in users)
            {
                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    await _emailSender.SendAsync(user.Email, user.Name, titulo, mensagem);
                }
            }
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848
            _logger.LogWarning(ex, "Failed to send email notifications");
#pragma warning restore CA1848
        }
    }
}

public class NotificationException : Exception
{
    public int StatusCode { get; }
    public NotificationException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}
