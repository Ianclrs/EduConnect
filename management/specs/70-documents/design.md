# Spec 70: Design — Document Management

## Domain Entities

### DocumentType (EduGestor.Core/Entities/DocumentType.cs)
```csharp
public class DocumentType : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Nome { get; set; } = string.Empty;       // "RG", "CPF", "Comprovante de Residência"
    public string? Descricao { get; set; }
    public bool IsRequired { get; set; } = true;            // obrigatório para matrícula?
    public int ValidadeMeses { get; set; } = 0;             // 0 = não expira
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
}
```

### Document (EduGestor.Core/Entities/Document.cs)
```csharp
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
```

## File Storage

### IFileStorage (EduGestor.Infrastructure/Storage/IFileStorage.cs)
```csharp
public interface IFileStorage
{
    Task<string> SaveAsync(Guid tenantId, Guid studentId, string fileName, Stream content);
    Task<Stream> GetAsync(string filePath);
    Task DeleteAsync(string filePath);
}
```

### LocalFileStorage — dev implementation
Stores files under `uploads/{tenantId}/{studentId}/{guid}_{filename}`.
Path root configurable via `appsettings.json`: `"FileStorage:RootPath": "uploads"`.

## DTOs

```csharp
public record CreateDocumentTypeRequest(string Nome, string? Descricao, bool IsRequired, int ValidadeMeses);
public record DocumentTypeDto(Guid Id, string Nome, string? Descricao, bool IsRequired, int ValidadeMeses);
public record DocumentDto(Guid Id, Guid StudentId, string StudentName, Guid DocumentTypeId, string DocumentTypeName, string NomeArquivo, string Status, DateTime? DataValidade, string? MotivoRejeicao, DateTime CreatedAt);
public record VerifyDocumentRequest(bool Approved, string? MotivoRejeicao);
```

## DocumentController

| Endpoint | Auth |
|---|---|
| `POST /documents/upload` (multipart: file + StudentId + DocumentTypeId) | Admin, Staff, Parent |
| `GET /students/{id}/documents` | Admin, Staff, Parent (own children) |
| `GET /documents/{id}/download` | Admin, Staff, Parent (own) |
| `POST /documents/{id}/verify` | Admin, Staff |
| `GET /documents/pending` | Admin, Staff |
| `GET /documents/expiring?days=30` | Admin, Staff |
| `POST /document-types` | Admin |
| `GET /document-types` | Admin, Staff |
| `PUT /document-types/{id}` | Admin |
| `DELETE /document-types/{id}` | Admin |

## DocumentService

- Upload: validate file (max 10MB, allowed extensions: pdf, jpg, jpeg, png), save via IFileStorage, create Document entity with status=Pendente.
- Verify: set Aprovado/Rejeitado, set VerifiedAt, set MotivoRejeicao if rejected.
- If Aprovado and has ValidadeMeses: calculate DataValidade.
- GetExpiring: documents where DataValidade <= DateTime.UtcNow.AddDays(days) and status=Aprovado.

## File Locations

| File | Path |
|---|---|
| DocumentType entity | `src/EduGestor.Core/Entities/DocumentType.cs` |
| Document entity | `src/EduGestor.Core/Entities/Document.cs` |
| DocumentStatus enum | `src/EduGestor.Core/Entities/DocumentStatus.cs` |
| IFileStorage | `src/EduGestor.Infrastructure/Storage/IFileStorage.cs` |
| LocalFileStorage | `src/EduGestor.Infrastructure/Storage/LocalFileStorage.cs` |
| DTOs | `src/EduGestor.Api/Contracts/DocumentDtos.cs` |
| IDocumentService + impl | `src/EduGestor.Infrastructure/Services/DocumentService.cs` |
| DocumentController | `src/EduGestor.Api/Controllers/DocumentController.cs` |
