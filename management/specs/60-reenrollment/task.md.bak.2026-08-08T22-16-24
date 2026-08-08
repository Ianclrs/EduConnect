# Spec 60: Tasks — Re-enrollment System

## Tasks

### T60.1: Create DTOs
- [ ] pending
- **Action:** Create `src/EduGestor.Api/Contracts/ReenrollmentDtos.cs` with `CreateReenrollmentRequest`, `ReenrollmentDetailDto`.
- **Verify:** `dotnet build` exits 0.

### T60.2: Create ReenrollmentService
- [ ] pending
- **Action:** Create `src/EduGestor.Infrastructure/Services/ReenrollmentService.cs`. Implement CreateAsync (eligibility + carry-forward), GetAllAsync, GetByIdAsync, ApproveAsync, RejectAsync.
- **Verify:** Unit tests for eligibility validation, document carry-forward, expired doc flagging.

### T60.3: Create ReenrollmentController
- [ ] pending
- **Action:** Create `src/EduGestor.Api/Controllers/ReenrollmentController.cs` with all 5 endpoints. Auth: Admin/Staff.
- **Verify:** Integration tests.

### T60.4: Add enrollment history endpoint
- [ ] pending
- **Action:** Add `GET /students/{id}/enrollment-history` to `StudentController` (Spec 40). Returns all Enrollments ordered by AnoLetivo DESC.
- **Verify:** History returns correct records.

### T60.5: Verify
- [ ] pending
- **Action:** `dotnet test` — all pass. `dotnet build` — zero errors. Full workflow: period → enroll → approve → new period → re-enroll → verify docs.

## Task Dependency Order

```
Spec 50 complete → T60.1 → T60.2 → T60.3 → T60.4 → T60.5
```

## Cross-Reference: Requirements → Tasks

| Requirement | Task(s) |
|---|---|
| FR-001-003: CRUD | T60.2, T60.3 |
| FR-004: Document carry-forward | T60.2 |
| FR-005/006: Approve/Reject | T60.2, T60.3 |
| FR-007: History | T60.4 |
| E1-E5: Edge cases | T60.2, T60.5 |
