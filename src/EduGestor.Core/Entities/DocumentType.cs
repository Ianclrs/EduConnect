using EduGestor.Core.Interfaces;

namespace EduGestor.Core.Entities;

public class DocumentType : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool IsRequired { get; set; } = true;
    public int ValidadeMeses { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
}
