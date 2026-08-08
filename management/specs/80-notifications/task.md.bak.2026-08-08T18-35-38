# Spec 80: Tasks — Notification System

## Task Checklist

### T8.1: Create Notification entity
- [ ] Create `src/EduGestor.Core/Entities/Notification.cs` implementing `ITenantScoped`
- [ ] Properties: Id, TenantId, Titulo, Mensagem, Tipo, ReferenceId, CreatedAt
- [ ] Create `NotificationType` enum

### T8.2: Create UserNotification entity
- [ ] Create `src/EduGestor.Core/Entities/UserNotification.cs`
- [ ] Properties: Id, NotificationId, UserId, IsRead, ReadAt

### T8.3: Create DTOs
- [ ] Create `src/EduGestor.Api/Contracts/NotificationDtos.cs`

### T8.4: Create NotificationService
- [ ] CreateAsync: create Notification + UserNotification per userId
- [ ] BroadcastAsync: to all parents in tenant
- [ ] SendByStudentAsync: to parents of a specific student
- [ ] GetForUserAsync: paginated, filtered by read status
- [ ] MarkReadAsync, MarkAllReadAsync
- [ ] GetUnreadCountAsync

### T8.5: Create NotificationController
- [ ] All endpoints with proper auth

### T8.6: Integrate with Document verification
- [ ] On document reject: auto-create notification for student's parents
- [ ] On document approve: no notification needed (positive signal)

### T8.7: Update AppDbContext
- [ ] Add DbSets, indexes on UserId+IsRead

### T8.8: EF migration and verify
- [ ] `dotnet ef migrations add AddNotifications`
- [ ] Test create, broadcast, read tracking via Swagger
