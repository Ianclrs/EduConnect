# Spec 70: Tasks — Document Management

## Task Checklist

### T7.1: Create DocumentType entity
- [ ] Create `src/EduGestor.Core/Entities/DocumentType.cs` implementing `ITenantScoped`
- [ ] Properties: Id, TenantId, Nome, Descricao, IsRequired, ValidadeMeses, IsActive

### T7.2: Create Document entity
- [ ] Create `src/EduGestor.Core/Entities/Document.cs` implementing `ITenantScoped`
- [ ] Properties: Id, TenantId, StudentId, DocumentTypeId, NomeArquivo, CaminhoArquivo, Status, DataValidade, MotivoRejeicao, CreatedAt, VerifiedAt
- [ ] Create `DocumentStatus` enum (Pendente, Aprovado, Rejeitado)

### T7.3: Create IFileStorage + LocalFileStorage
- [ ] Create `src/EduGestor.Infrastructure/Storage/IFileStorage.cs`
- [ ] Create `src/EduGestor.Infrastructure/Storage/LocalFileStorage.cs`
- [ ] SaveAsync: store to `uploads/{tenantId}/{studentId}/{guid}_{filename}`
- [ ] GetAsync: open FileStream for reading
- [ ] DeleteAsync: delete file

### T7.4: Create DTOs
- [ ] Create `src/EduGestor.Api/Contracts/DocumentDtos.cs`

### T7.5: Create DocumentService
- [ ] Upload: validate extension + size, save file, create entity
- [ ] Verify: approve/reject with reason
- [ ] GetPending: docs with status=Pendente
- [ ] GetExpiring: docs with DataValidade within N days

### T7.6: Create DocumentTypeService + Controller
- [ ] CRUD for document types per tenant

### T7.7: Create DocumentController
- [ ] All document endpoints
- [ ] File download with proper Content-Type

### T7.8: Update AppDbContext
- [ ] Add DbSets, indexes

### T7.9: Add config for file storage
- [ ] Add `FileStorage:RootPath` to appsettings

### T7.10: EF migration and verify
- [ ] `dotnet ef migrations add AddDocuments`
- [ ] Test upload, verify, download via Swagger
