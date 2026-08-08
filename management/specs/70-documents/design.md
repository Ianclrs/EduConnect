# Spec 70: Design — Document Management

## Design Approach

Sistema de gestão de documentos com **armazenamento em disco local** via interface `IFileStorage` (permite trocar para S3 no futuro). Entidades `DocumentType` (configurável por tenant) e `Document` (vinculado a aluno) permitem categorização, upload, verificação e tracking de validade.

## Architecture Decisions

- **AD-001: IFileStorage abstraction** — desacopla storage da lógica de negócio. Dev: `LocalFileStorage`. Prod futuro: `S3FileStorage`.
- **AD-002: Validação de arquivo** — extensão + MIME type + tamanho. Path traversal prevention via sanitização.
- **AD-003: Validade calculada** — ao aprovar, se DocumentType.ValidadeMeses > 0, DataValidade = VerifiedAt + ValidadeMeses.

## Data Flow: Upload → Verify
```
POST /documents/upload (multipart)
  → DocumentService.UploadAsync(file, studentId, docTypeId, tenantId)
    → Validate: file size ≤ 10MB, extension ∈ {pdf,jpg,jpeg,png}
    → Validate: student exists, document type exists and IsActive
    → Save: IFileStorage.SaveAsync(tenantId, studentId, fileName, stream)
    → Create: Document { Status=Pendente, CaminhoArquivo=path }
    → SaveChangesAsync()
  → Return 201 DocumentDto

POST /documents/{id}/verify { approved: true }
  → DocumentService.VerifyAsync(id, approved, motivo, tenantId)
    → Validate: document exists and belongs to tenant
    → Status = Aprovado, VerifiedAt = UtcNow
    → If docType.ValidadeMeses > 0: DataValidade = UtcNow.AddMonths(ValidadeMeses)
    → SaveChangesAsync()
  → Return 200

POST /documents/{id}/verify { approved: false, motivoRejeicao: "ilegível" }
  → Similar flow, Status = Rejeitado, MotivoRejeicao = motivo
  → Auto-create notification via INotificationService (Spec 80)
```

## Domain Entities

### DocumentType (EduGestor.Core/Entities/DocumentType.cs)
```csharp
public class DocumentType : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool IsRequired { get; set; } = true;
    public int ValidadeMeses { get; set; } = 0;  // 0 = never expires
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

public enum DocumentStatus { Pendente = 0, Aprovado = 1, Rejeitado = 2 }
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

### LocalFileStorage
Stores at `{RootPath}/{tenantId}/{studentId}/{guid}_{sanitizedFilename}`.
RootPath from `appsettings.json`: `"FileStorage:RootPath": "uploads"`.

## DTOs
```csharp
public record CreateDocumentTypeRequest(string Nome, string? Descricao, bool IsRequired, int ValidadeMeses);
public record DocumentTypeDto(Guid Id, string Nome, string? Descricao, bool IsRequired, int ValidadeMeses);
public record DocumentDto(Guid Id, Guid StudentId, string StudentName, Guid DocumentTypeId, string DocumentTypeName, string NomeArquivo, string Status, DateTime? DataValidade, string? MotivoRejeicao, DateTime CreatedAt);
public record VerifyDocumentRequest(bool Approved, string? MotivoRejeicao);
```

## Controllers

| Endpoint | Auth |
|---|---|
| `POST /documents/upload` | Admin, Staff, Parent |
| `GET /students/{id}/documents` | Admin, Staff, Parent (own) |
| `GET /documents/{id}/download` | Admin, Staff, Parent (own) |
| `POST /documents/{id}/verify` | Admin, Staff |
| `GET /documents/pending` | Admin, Staff |
| `GET /documents/expiring?days=30` | Admin, Staff |
| `POST /document-types` | Admin |
| `GET /document-types` | Admin, Staff |
| `PUT /document-types/{id}` | Admin |
| `DELETE /document-types/{id}` | Admin |

## Error Handling

| Condition | HTTP | Body |
|---|---|---|
| File too large | 400 | `{"error":"file_too_large","max_mb":10}` |
| Invalid extension | 400 | `{"error":"invalid_extension","allowed":["pdf","jpg","jpeg","png"]}` |
| Document type inactive | 400 | `{"error":"document_type_inactive"}` |
| Parent not linked | 403 | `{"error":"not_linked_to_student"}` |
| File not found | 404 | `{"error":"file_not_found"}` |

## File / Module Layout

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

## Cross-Reference: Requirements → Design

| Requirement | Covered By |
|---|---|
| FR-001-003: Upload/List/Download | DocumentService, Controller, IFileStorage |
| FR-004/005: Verify/Reject | DocumentService.VerifyAsync |
| FR-006/007: Pending/Expiring | DocumentService, Controller |
| FR-008: DocumentType CRUD | DocumentTypeService, Controller |
| FR-009/010/011: Entities + Storage | Domain Entities, IFileStorage |
| E1-E7: Edge cases | Error Handling table |
