using Microsoft.EntityFrameworkCore;
using EduGestor.Core.Entities;
using EduGestor.Infrastructure.Contracts;
using EduGestor.Infrastructure.Data;

namespace EduGestor.Infrastructure.Services;

public interface IReenrollmentService
{
    Task<EnrollmentDto> CreateAsync(CreateReenrollmentRequest request, Guid tenantId);
    Task<PagedResponse<EnrollmentDto>> GetAllAsync(Guid tenantId, string? status, Guid? periodId, int page, int pageSize, Guid userId, UserRole role);
    Task<ReenrollmentDetailDto> GetByIdAsync(Guid id, Guid tenantId, Guid userId, UserRole role);
    Task<EnrollmentDto> ApproveAsync(Guid id, Guid tenantId);
    Task RejectAsync(Guid id, string motivo, Guid tenantId);
}

public class ReenrollmentService : IReenrollmentService
{
    private readonly AppDbContext _db;

    public ReenrollmentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<EnrollmentDto> CreateAsync(CreateReenrollmentRequest request, Guid tenantId)
    {
        // Validate student
        var student = await _db.Students
            .FirstOrDefaultAsync(s => s.Id == request.StudentId);

        if (student is null)
            throw new ReenrollmentException("student_not_found", 404);

        if (student.Status == StudentStatus.Inativo)
            throw new ReenrollmentException("student_inactive", 400);

        if (student.Status == StudentStatus.Transferido)
            throw new ReenrollmentException("student_transferred", 400);

        // Must have at least one prior approved enrollment
        var hasPrior = await _db.Enrollments
            .AnyAsync(e => e.StudentId == request.StudentId && e.Status == EnrollmentStatus.Aprovado);

        if (!hasPrior)
            throw new ReenrollmentException("student_has_no_prior_enrollment", 400);

        // Validate period
        var period = await _db.EnrollmentPeriods
            .FirstOrDefaultAsync(p => p.Id == request.EnrollmentPeriodId);

        if (period is null)
            throw new ReenrollmentException("period_not_found", 404);

        if (!period.IsActive)
            throw new ReenrollmentException("enrollment_period_closed", 400);

        var now = DateTime.UtcNow;
        if (now < period.DataInicio || now > period.DataFim)
            throw new ReenrollmentException("enrollment_period_out_of_window", 400);

        // Check duplicate reenrollment in same period
        var exists = await _db.Enrollments
            .AnyAsync(e => e.StudentId == request.StudentId && e.EnrollmentPeriodId == request.EnrollmentPeriodId);
        if (exists)
            throw new ReenrollmentException("reenrollment_already_exists", 409);

        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            StudentId = request.StudentId,
            EnrollmentPeriodId = request.EnrollmentPeriodId,
            Status = EnrollmentStatus.Pendente,
            CreatedAt = DateTime.UtcNow
        };

        // TODO: Spec 70 — Document carry-forward.
        // When Document and DocumentType entities exist:
        //   1. Query student's approved Documents where DataValidade > UtcNow or null
        //   2. Query required DocumentTypes (IsRequired && IsActive)
        //   3. If any required type has no valid doc → Status = DocumentacaoPendente
        // For now, always set Pendente (no document system yet).

        _db.Enrollments.Add(enrollment);
        await _db.SaveChangesAsync();

        return new EnrollmentDto(
            enrollment.Id,
            student.Id,
            student.Nome,
            period.Id,
            period.Nome,
            enrollment.Status.ToString(),
            enrollment.MotivoRejeicao,
            enrollment.CreatedAt,
            enrollment.ApprovedAt);
    }

    public async Task<PagedResponse<EnrollmentDto>> GetAllAsync(
        Guid tenantId, string? status, Guid? periodId, int page, int pageSize, Guid userId, UserRole role)
    {
        if (page < 1) page = 1;
        if (pageSize > 100) pageSize = 100;
        if (pageSize < 1) pageSize = 20;

        IQueryable<Enrollment> query = _db.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Period);

        // Parent: only see linked children's reenrollments
        if (role == UserRole.Parent)
        {
            query = query.Where(e => e.Student.StudentParents.Any(sp => sp.ParentId == userId));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<EnrollmentStatus>(status, ignoreCase: true, out var parsedStatus))
            query = query.Where(e => e.Status == parsedStatus);

        if (periodId.HasValue)
            query = query.Where(e => e.EnrollmentPeriodId == periodId.Value);

        var total = await query.CountAsync();

        var enrollments = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EnrollmentDto(
                e.Id,
                e.StudentId,
                e.Student.Nome,
                e.EnrollmentPeriodId,
                e.Period.Nome,
                e.Status.ToString(),
                e.MotivoRejeicao,
                e.CreatedAt,
                e.ApprovedAt))
            .ToListAsync();

        return new PagedResponse<EnrollmentDto>(enrollments, total, page, pageSize);
    }

    public async Task<ReenrollmentDetailDto> GetByIdAsync(Guid id, Guid tenantId, Guid userId, UserRole role)
    {
        var enrollment = await _db.Enrollments
            .Include(e => e.Student)
            .ThenInclude(s => s.StudentParents)
            .Include(e => e.Period)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollment is null)
            throw new ReenrollmentException("enrollment_not_found", 404);

        // Parent: must be linked to student
        if (role == UserRole.Parent)
        {
            if (!enrollment.Student.StudentParents.Any(sp => sp.ParentId == userId))
                throw new ReenrollmentException("not_linked_to_student", 403);
        }

        // TODO: Spec 70 — Populate CarriedDocuments and MissingDocumentTypes
        // from the document carry-forward logic.

        return new ReenrollmentDetailDto(
            enrollment.Id,
            enrollment.StudentId,
            enrollment.Student.Nome,
            enrollment.EnrollmentPeriodId,
            enrollment.Period.Nome,
            enrollment.Status.ToString(),
            enrollment.MotivoRejeicao,
            enrollment.CreatedAt,
            enrollment.ApprovedAt,
            CarriedDocuments: [],
            MissingDocumentTypes: []);
    }

    public async Task<EnrollmentDto> ApproveAsync(Guid id, Guid tenantId)
    {
        var enrollment = await _db.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Period)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollment is null)
            throw new ReenrollmentException("enrollment_not_found", 404);

        if (!EnrollmentService.CanTransition(enrollment.Status, EnrollmentStatus.Aprovado))
            throw new ReenrollmentException("invalid_transition_to_approved", 400);

        // TODO: Spec 70 — Check pending required documents before approving.
        // If missing, return 400.

        enrollment.Status = EnrollmentStatus.Aprovado;
        enrollment.ApprovedAt = DateTime.UtcNow;

        // Side effect: update student
        enrollment.Student.Status = StudentStatus.Ativo;
        enrollment.Student.AnoLetivo = enrollment.Period.AnoLetivo;

        await _db.SaveChangesAsync();

        return new EnrollmentDto(
            enrollment.Id,
            enrollment.StudentId,
            enrollment.Student.Nome,
            enrollment.EnrollmentPeriodId,
            enrollment.Period.Nome,
            enrollment.Status.ToString(),
            enrollment.MotivoRejeicao,
            enrollment.CreatedAt,
            enrollment.ApprovedAt);
    }

    public async Task RejectAsync(Guid id, string motivo, Guid tenantId)
    {
        var enrollment = await _db.Enrollments
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollment is null)
            throw new ReenrollmentException("enrollment_not_found", 404);

        if (!EnrollmentService.CanTransition(enrollment.Status, EnrollmentStatus.Rejeitado))
            throw new ReenrollmentException("invalid_transition_to_rejected", 400);

        if (string.IsNullOrWhiteSpace(motivo) || motivo.Length < 10 || motivo.Length > 500)
            throw new ReenrollmentException("motivo_required_10_to_500_chars", 400);

        enrollment.Status = EnrollmentStatus.Rejeitado;
        enrollment.MotivoRejeicao = motivo;

        await _db.SaveChangesAsync();
    }
}

public class ReenrollmentException : Exception
{
    public int StatusCode { get; }

    public ReenrollmentException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}
