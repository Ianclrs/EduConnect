# DsCode Roadmap

## Spec Statuses

| Status | Meaning |
|---|---|---|---|
| `proposed` | Idea stage, no spec written yet |
| `planned` | Roadmap entry created via /spec-plan |
| `created` | Spec documents created via /spec-new |
| `verified` | Spec documents verified via /spec-verify |
| `implemented` | Code implemented via /spec-implement |
| `in-progress` | Under active development on a feature branch |
| `audited` | Final stage — implementation audited via /spec-audit. Feature is live. |
| `discarded` | Intentionally abandoned |

---

## Specs

| # | Name | Status | References |
|---|---|---|---|
| 1 | Project Bootstrap & Infrastructure | planned | V1, ADR-001 |
| 2 | Multi-Tenant Architecture | planned | V2, ADR-002 |
| 3 | Authentication & Authorization | planned | V3, ADR-003 |
| 4 | Student Management | planned | V4 |
| 5 | Enrollment System (Matrícula) | planned | V5 |
| 6 | Re-enrollment System (Rematrícula) | planned | V6 |
| 7 | Document Management | planned | V7 |
| 8 | Notification System | planned | V8 |
| 9 | Parent Portal API | planned | V9 |
| 10 | Frontend Application (React + PWA) | planned | V10 |

---

## Dependency Graph

```
Spec 1: Bootstrap & Infra
  └─► Spec 2: Multi-Tenant
        ├─► Spec 3: Auth
        │     ├─► Spec 4: Student Management
        │     │     ├─► Spec 5: Enrollment
        │     │     │     └─► Spec 6: Re-enrollment
        │     │     ├─► Spec 7: Documents
        │     │     └─► Spec 9: Parent Portal API
        │     │           └─► Spec 10: Frontend
        │     └─► Spec 8: Notifications
        │           └─► Spec 9: Parent Portal API
        └─► Spec 7: Documents
              └─► Spec 8: Notifications
```
