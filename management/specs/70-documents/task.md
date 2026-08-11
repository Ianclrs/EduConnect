# Spec 70: Tasks — Document Management

## Tasks

### T70.1: Create DocumentType entity
- [x] done
- **Action:** Create `src/Ciclo.Core/Entities/DocumentType.cs` implementing `ITenantScoped`.
- **Verify:** `dotnet build src/Ciclo.Core` exits 0.

### T70.2: Create Document entity + DocumentStatus enum
- [x] done
- **Action:** Create `src/Ciclo.Core/Entities/Document.cs` implementing `ITenantScoped`. Create `DocumentStatus` enum.
- **Verify:** `dotnet build` exits 0.

### T70.3: Create IFileStorage + LocalFileStorage
- [x] done
- **Action:** Create `src/Ciclo.Infrastructure/Storage/IFileStorage.cs` and `LocalFileStorage.cs`. Sanitize filenames. Configure DI.
- **Verify:** Unit test: save file, verify exists on disk, get stream, delete.

### T70.4: Create DTOs
- [x] done
- **Action:** Create `src/Ciclo.Api/Contracts/DocumentDtos.cs`.
- **Verify:** `dotnet build` exits 0.

### T70.5: Create DocumentService
- [x] done
- **Action:** Create `src/Ciclo.Infrastructure/Services/DocumentService.cs`. Upload (validate + save), Verify (approve/reject + calculate validity), GetPending, GetExpiring.
- **Verify:** Unit tests for upload validation, verification with/without validity.

### T70.6: Create DocumentTypeService + Controller endpoints
- [x] done
- **Action:** CRUD for document types in DocumentService and DocumentController.
- **Verify:** Admin can create/list/update/soft-delete document types.

### T70.7: Create DocumentController
- [x] done
- **Action:** All document endpoints with proper auth. File download with Content-Type.
- **Verify:** Integration tests.

### T70.8: Update AppDbContext
- [x] done
- **Action:** Add `DbSet<DocumentType>`, `DbSet<Document>`. Configure indexes.
- **Verify:** `dotnet build` exits 0.

### T70.9: Configure file storage
- [x] done
- **Action:** Add `FileStorage:RootPath` to `appsettings.json`. Register `LocalFileStorage` in DI.
- **Verify:** Upload works end-to-end.

### T70.10: EF Migration and verify
- [x] done
- **Action:** `dotnet ef migrations add AddDocuments`. Run full test suite.
- **Verify:** `dotnet test` — all pass. `dotnet build` — zero errors.

## Task Dependency Order

```
T70.1/T70.2/T70.3 → T70.4 → T70.5 → T70.6 → T70.7 → T70.8 → T70.9 → T70.10
```

## Cross-Reference: Requirements → Tasks

| Requirement | Task(s) |
|---|---|
| FR-001-003: Upload/Download | T70.3, T70.5, T70.7 |
| FR-004/005: Verify/Reject | T70.5 |
| FR-006/007: Pending/Expiring | T70.5, T70.7 |
| FR-008: DocumentType CRUD | T70.6 |
| FR-009/010/011: Entities/Storage | T70.1, T70.2, T70.3 |
| E1-E7: Edge cases | T70.5, T70.10 |
