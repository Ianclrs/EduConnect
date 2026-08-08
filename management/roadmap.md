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
| 10 | Project Bootstrap & Infrastructure | audited | V1, ADR-001 |
| 20 | Multi-Tenant Architecture | audited | V2, ADR-002 |
| 30 | Authentication & Authorization | verified | V3, ADR-003 |
| 40 | Student Management | verified | V4 |
| 50 | Enrollment System (Matrícula) | verified | V5 |
| 60 | Re-enrollment System (Rematrícula) | verified | V6 |
| 70 | Document Management | verified | V7 |
| 80 | Notification System | verified | V8 |
| 90 | Parent Portal API | verified | V9 |
| 100 | Frontend Application (React + PWA) | verified | V10 |

---

## Dependency Graph

```
Spec 10: Bootstrap & Infra
  └─► Spec 20: Multi-Tenant
        ├─► Spec 30: Auth
        │     ├─► Spec 40: Student Management
        │     │     ├─► Spec 50: Enrollment
        │     │     │     └─► Spec 60: Re-enrollment
        │     │     ├─► Spec 70: Documents
        │     │     └─► Spec 90: Parent Portal API
        │     │           └─► Spec 100: Frontend
        │     └─► Spec 80: Notifications
        │           └─► Spec 90: Parent Portal API
        └─► Spec 70: Documents
              └─► Spec 80: Notifications
```
