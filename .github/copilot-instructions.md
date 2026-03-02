# GitHub Copilot Agent Instructions — eAppraisal System

> **Project:** e-Appraisal Employee Recruitment Case Study  
> **Company:** Nano Technologies (fictitious training scenario)  
> **Date:** 2026-03-02  
> **Stack:** .NET / C# · Azure Kubernetes Service (AKS) · Azure SQL Database · Azure Front Door

---

## 1. Project Overview

The **eAppraisal system** digitalises the employee appraisal process at Nano Technologies — a 1,000-person firm with offices across four regions of India (Head Office: Mumbai). The manual file-based process is replaced by a role-aware, audit-compliant, web-based system.

### Key Stakeholders & Roles
| Role | Responsibilities |
|---|---|
| **HR** | Initiate appraisal cycle, manage employee records, unlock accounts, export reports |
| **Manager (Appraiser)** | Enter managerial comments, perform final assessment & approval |
| **Employee (Appraisee)** | Fill in self-assessment inputs |
| **IT Admin** | Unlock accounts, manage system configuration, access SIEM/audit |

### Non-Negotiable Requirements
- ≥ 1,000 concurrent users with **p95 < 2 s**
- **PAN masking** everywhere — UI, API payloads, logs, exports
- **15-minute idle session timeout** + **3-strike account lockout** (manual IT Admin unlock)
- **7-year append-only audit trail** with SIEM alerting
- **ISO 27001 / SOC 2 / GDPR / DPDP** alignment
- **RTO/RPO** commitments backed by quarterly restore drills

---

## 2. Architectural Overview

### Style: Service-Based Architecture (SBA)
Five coarse-grained services map 1:1 to the C4 L2 container view. They run as independent Kubernetes Deployments and share a single Azure SQL Database (zone-redundant + Failover Group).

```
Azure Front Door (Anycast edge, WAF, TLS termination)
        │
        ▼
  AKS Ingress Controller
        │
  ┌─────┴──────┐
  │   Web       │  Next.js / Blazor frontend — RBAC-aware UI
  │   API       │  Workflow, comments, CTC, masking/RBAC
  │   Export    │  Policy-aware exports, signed URLs, masking
  │   Notify    │  Outbox → SMTP, retry / DLQ
  │   Audit     │  Structured audit → SIEM, 7-year retention
  └─────────────┘
        │
  Azure SQL Database
  (Zone-redundant primary + Failover Group → secondary region)
```

### Deployment Platform
- **Azure Kubernetes Service (AKS)** — multi-AZ node pools, HPA, cluster autoscaler, PDBs, topology spread
- **Azure Front Door** — primary global ingress; Azure Traffic Manager + regional App Gateway as secondary ingress path
- **Azure SQL Database** — zone-redundant, Failover Group for regional DR
- **Azure Key Vault** — secrets, TLS certs, encryption keys
- **Azure Monitor / Log Analytics** — telemetry, SIEM forwarding

---

## 3. Component Plan

### 3.1 Web Service (`src/Web/`)
| Aspect | Detail |
|---|---|
| **Purpose** | Serve the browser UI to HR, Manager, and Employee users |
| **Technology** | .NET Blazor Server or React/Next.js (SSR) |
| **Responsibilities** | Render role-specific pages, enforce session timeout (15 min idle), display masked PAN, forward all mutations to API |
| **Key Interfaces** | HTTP → API Service (authenticated via token) |
| **Guardrails** | No business logic; no raw PAN; HSTS, CSP, X-Frame-Options headers enforced |

### 3.2 API Service (`src/Services/Api/`)
| Aspect | Detail |
|---|---|
| **Purpose** | Central application-logic service — appraisal workflow, CTC, RBAC, masking |
| **Technology** | ASP.NET Core Web API (.NET 8) |
| **Responsibilities** | Orchestrate appraisal state machine, enforce RBAC + field masking, validate CTC auto-calc, emit audit events via outbox, apply lockout/unlock logic |
| **Key Interfaces** | Inbound: HTTPS from Web; Outbound: Azure SQL, Notification outbox, Audit outbox |
| **Guardrails** | All endpoints require authentication; deny-all baseline NetworkPolicy; no raw PAN in logs |

### 3.3 Reporting & Export Service (`src/Services/Export/`)
| Aspect | Detail |
|---|---|
| **Purpose** | Generate policy-aware exports (PDF/Excel) with masked data and signed download URLs |
| **Technology** | ASP.NET Core background worker + reporting library (e.g., QuestPDF) |
| **Responsibilities** | Apply role-based column policies, mask PAN in output files, produce time-limited signed URLs via Azure Blob Storage, queue-driven to avoid API latency impact |
| **Key Interfaces** | Inbound: Azure Service Bus queue from API; Outbound: Azure Blob Storage |
| **Guardrails** | Runs in separate node pool; URL TTL ≤ 15 min; encrypted at-rest Blob storage |

### 3.4 Notification Service (`src/Services/Notification/`)
| Aspect | Detail |
|---|---|
| **Purpose** | Send transactional emails for appraisal workflow events |
| **Technology** | ASP.NET Core worker + SendGrid / Azure Communication Services |
| **Responsibilities** | Consume outbox messages from API, deliver email with retry, move to DLQ on repeated failure, never include raw PAN in email body |
| **Key Interfaces** | Inbound: Outbox table / Service Bus from API; Outbound: SMTP/Email API |
| **Guardrails** | No sensitive data in subject line; retry with exponential back-off; DLQ alert after 3 failures |

### 3.5 Audit Forwarder Service (`src/Services/Audit/`)
| Aspect | Detail |
|---|---|
| **Purpose** | Collect structured audit events from all services and forward to SIEM; ensure 7-year retention |
| **Technology** | .NET worker + Azure Event Hub / Log Analytics sink |
| **Responsibilities** | Consume append-only audit stream, enrich with correlation IDs, forward to Azure Sentinel / Log Analytics, alert on critical events (lockout, PAN unmask, privilege change) |
| **Key Interfaces** | Inbound: Audit outbox from API/Export; Outbound: Azure Log Analytics, SIEM |
| **Guardrails** | Immutable sink; 7-year retention policy enforced; no raw PAN in audit records |

### 3.6 Shared Libraries (`src/Shared/`)
| Module | Responsibility |
|---|---|
| `Shared.Masking` | PAN masking utilities (fixed pattern) — used by all services |
| `Shared.Auth` | JWT validation, RBAC attribute helpers, idle-timeout middleware |
| `Shared.Audit` | Structured audit event models, outbox writer |
| `Shared.Resilience` | Polly retry/circuit-breaker policies for DB and downstream calls |
| `Shared.Contracts` | DTOs, API contracts, domain events |

### 3.7 Gateway / Ingress (`src/Gateways/`)
| Aspect | Detail |
|---|---|
| **Purpose** | Define ingress routing rules, rate limiting, and WAF policy |
| **Technology** | NGINX Ingress Controller on AKS + Azure Front Door WAF policy |
| **Responsibilities** | Route `/` → Web, `/api/` → API, `/export/` → Export; enforce TLS; apply rate limits |

---

## 4. Development Plan

### Phase 0 — Foundation (Week 1–2)
- [ ] Provision AKS cluster (multi-AZ, system + user node pools)
- [ ] Set up Azure SQL Database (zone-redundant, Failover Group to secondary region)
- [ ] Configure Azure Front Door + WAF + TLS certificates (Key Vault)
- [ ] Create GitHub repository structure, branch strategy (`main`, `develop`, feature branches)
- [ ] Establish CI pipeline (build, lint, unit tests) per service
- [ ] Set up Azure Container Registry (ACR) and image scanning
- [ ] Define baseline NetworkPolicies (deny-all, allow-list flows)

### Phase 1 — Core Services (Week 3–6)
- [ ] **Database schema**: Apply DDL (`docs/architecture/.../DDL_Azure_Sql_eApproval.sql`), run migrations via EF Core / Flyway
- [ ] **API Service**: Authentication middleware (JWT + RBAC), PAN masking, employee CRUD, appraisal state machine (HR → Manager → Employee → Manager final)
- [ ] **Web Service**: Login page, role-based navigation, HR section (employee info, appraisal initiation), Manager section (comments, assessment), Employee section (self-assessment)
- [ ] **Shared.Auth**: Session timeout (15 min), lockout (3-strike), IT Admin unlock flow
- [ ] **Shared.Audit**: Outbox writer, structured event schema

### Phase 2 — Supporting Services (Week 7–9)
- [ ] **Notification Service**: Outbox consumer, email templates for each workflow step, retry/DLQ
- [ ] **Export Service**: Queue-driven PDF/Excel export, role-based column masking, signed Blob URL
- [ ] **Audit Forwarder**: Event Hub / Log Analytics sink, 7-year retention policy, SIEM alert rules
- [ ] Load test baseline: validate p95 < 2 s at ≥ 1,000 concurrent users

### Phase 3 — Resilience & Security Hardening (Week 10–11)
- [ ] Enable HPA on API and Web (min 3 replicas); separate HPA on Export/Notification by queue depth
- [ ] Configure PodDisruptionBudgets and topology spread constraints
- [ ] Validate secondary ingress path (Traffic Manager + regional App Gateway)
- [ ] Configure PITR retention (28–35 days), LTR policy (12 monthly, 7 yearly), daily Blob export
- [ ] Penetration test / SAST scan; remediate findings
- [ ] Execute first DR drill (planned Failover Group failover + application retry validation)

### Phase 4 — Observability & Compliance (Week 12)
- [ ] Azure Monitor dashboards: p95 latency, error rate, HPA events, DB failover metrics
- [ ] SIEM alert rules: lockout events, PAN unmask, privilege escalation, export anomalies
- [ ] Compliance evidence pack: screenshots, restore logs, RTO/RPO measurements
- [ ] Update RTM (Requirements Traceability Matrix) with test evidence
- [ ] Runbooks: accidental delete (PITR), latent corruption (LTR/export), regional DR (Failover Group)

### Phase 5 — Cutover & Hypercare (Week 13–14)
- [ ] Blue/green rollout to production; validate canary traffic
- [ ] Conduct stress test at 120% expected peak load
- [ ] Verify 7-year audit retention settings in Log Analytics / SIEM
- [ ] Complete quarterly restore drill documentation (PITR + LTR)
- [ ] Hand-off runbooks and operational playbooks to ops team

---

## 5. Architectural Guardrails for AI Agents

When generating or modifying code in this repository, **always** obey the following rules:

### Security
- **Never** log, return, or include raw PAN in any string. Always call `MaskingService.MaskPan()`.
- **Never** bypass the `[Authorize]` attribute or RBAC check. Every controller action and service method that touches employee or appraisal data must validate the caller's role.
- **Never** expose connection strings or secrets in code — use Azure Key Vault references / environment variables via Kubernetes secrets.
- All outbound HTTP calls must use TLS 1.2+. Reject self-signed certificates in non-local environments.

### Data
- All EF Core migrations must be reviewed for data loss risk before applying to production.
- PAN must be stored encrypted at the column level (Always Encrypted or equivalent).
- Audit events are **append-only** — never update or delete audit records.

### Performance
- Export and reporting workloads must be triggered asynchronously via queue — never inline on the API request thread.
- Database calls must use Polly retry with exponential back-off (see `Shared.Resilience`).

### Testing
- All new features must include unit tests (≥ 80% coverage target) and at least one integration test.
- Masking logic must have property-based tests covering edge cases (null, empty, already-masked).

### CI/CD
- PRs to `main` require: green build + all tests pass + no HIGH/CRITICAL vulnerabilities in container scan.
- Feature flags must be used for any incomplete feature deployed to production.

---

## 6. Key Documents

| Document | Path |
|---|---|
| Business Requirements | `docs/architecture/ArchitectureRequirementAnalysisDesign/BRD_e-Appraisal_System_Clean.md` |
| Architectural Guardrails | `docs/architecture/ArchitectureRequirementAnalysisDesign/Architectural_Guardrails_eAppraisal.md` |
| ADR 0001 — Deployment Variant (Kubernetes) | `docs/architecture/ArchitectureRequirementAnalysisDesign/0001-ADR-deployment-variant.md` |
| ADR 0002 — Architectural Style (SBA) | `docs/architecture/ArchitectureRequirementAnalysisDesign/0002-ADR-architectural-style-sba.md` |
| ADR 0004 — Database Selection | `docs/architecture/ArchitectureRequirementAnalysisDesign/0004-ADR-DB-Selection_eAppraisal.md` |
| ADR 0005 — Site Reliability | `docs/architecture/ArchitectureRequirementAnalysisDesign/0005-ADR-Site-Reliability_eApproval.md` |
| ADR 0006 — Backup & Recovery | `docs/architecture/ArchitectureRequirementAnalysisDesign/0006-ADR-Backup-Strategy_eApproval.md` |
| DDL (Azure SQL) | `docs/architecture/ArchitectureRequirementAnalysisDesign/DDL_Azure_Sql_eApproval.sql` |
| C4 System Context | `docs/architecture/ArchitectureRequirementAnalysisDesign/C4_System_Context_eAppraisal.md` |
| Security Risk Register | `docs/architecture/ArchitectureRequirementAnalysisDesign/Security_Risk_Register_eAppraisal.md` |
| RTM / STRIDE | `docs/architecture/ArchitectureRequirementAnalysisDesign/RTM_STRIDE_eAppraisal.md` |
| Test Plan | `docs/testing/test-plan.md` |
| Test Cases | `docs/testing/test-cases.md` |
| Backup & Site Recovery Diagram | `docs/diagrams/mermaid/backup-and-site-recovery.md` |
