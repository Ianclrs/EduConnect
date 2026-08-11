# Spec 50: Tasks — Enrollment System

## Tasks

### T50.1: Create EnrollmentPeriod entity
- [x] done
- **Action:** Create `src/Ciclo.Core/Entities/EnrollmentPeriod.cs` implementing `ITenantScoped`.
- **Verify:** `dotnet build src/Ciclo.Core` exits 0.

### T50.2: Create Enrollment entity + EnrollmentStatus enum
- [x] done
- **Action:** Create `src/Ciclo.Core/Entities/Enrollment.cs` implementing `ITenantScoped`. Create `EnrollmentStatus` enum with all 6 states.
- **Verify:** `dotnet build` exits 0.

### T50.3: Create DTOs
- [x] done
- **Action:** Create `src/Ciclo.Api/Contracts/EnrollmentDtos.cs` with all records.
- **Verify:** `dotnet build` exits 0.

### T50.4: Create EnrollmentService with state machine
- [x] done
- **Action:** Create `src/Ciclo.Infrastructure/Services/EnrollmentService.cs`. Implement `CanTransition(from, to): bool` with transition table. Methods: CreateAsync, SubmitAsync, ApproveAsync, RejectAsync, CancelAsync. Period management: CreatePeriodAsync, ClosePeriodAsync.
- **Verify:** Unit tests for all valid and invalid transitions.

### T50.5: Create EnrollmentController
- [x] done
- **Action:** Create `src/Ciclo.Api/Controllers/EnrollmentController.cs` with all endpoints and auth attributes.
- **Verify:** Integration tests via `WebApplicationFactory`.

### T50.6: Update AppDbContext
- [x] done
- **Action:** Add `DbSet<EnrollmentPeriod>`, `DbSet<Enrollment>`. Configure indexes.
- **Verify:** `dotnet build` exits 0.

### T50.7: EF Migration and verify
- [x] done
- **Action:** `dotnet ef migrations add AddEnrollments`. Run full test suite.
- **Verify:** `dotnet test` — all pass. `dotnet build` — zero errors.

## Task Dependency Order

```
T50.1/T50.2 → T50.3 → T50.4 → T50.5 → T50.6 → T50.7
```

## Cross-Reference: Requirements → Tasks

| Requirement | Task(s) |
|---|---|
| FR-001-003: Periods | T50.1, T50.4, T50.5 |
| FR-004-010: Enrollments + states | T50.2, T50.4, T50.5 |
| FR-011/012: Entities | T50.1, T50.2 |
| FR-013: State machine | T50.4 |
| E1-E7: Edge cases | T50.4, T50.7 |
