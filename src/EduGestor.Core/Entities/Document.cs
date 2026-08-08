using EduGestor.Core.Interfaces;

namespace EduGestor.Core.Entities;

public class Document : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid StudentId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public string CaminhoArquivo { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; } = DocumentStatus.Pendente;
    public DateTime? DataValidade { get; set; }
    public string? MotivoRejeicao { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }

    public Student Student { get; set; } = null!;
    public DocumentType DocumentType { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}

public enum DocumentStatus
{
    Pendente = 0,
    Aprovado = 1,
    Rejeitado = 2
}
