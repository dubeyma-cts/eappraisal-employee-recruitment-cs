
# Architectural Guardrails

e-Appraisal System  
Version 1.0  
Generated: 2026-02-27

This document defines the mandatory architectural guardrails that govern the design, development, deployment, and operation of the e-Appraisal System.  
It consolidates validated artifacts including the BRD, Functional and Non-Functional Use-Cases, Confirmed Clarification Questions, RTM, Security Risk Register, and Compliance Matrix.

---

## 1. Foundational Architecture Principles

1. Security and Privacy by Design  
   - The system shall be designed so that confidentiality, integrity, and availability of data are protected by default. Sensitive data such as PAN shall be masked everywhere unless explicitly permitted.

2. Least Privilege and Role Isolation  
   - Every feature, endpoint, data element, and export must enforce RBAC rules so users see only what they need.

3. End-to-End Auditability  
   - All workflows must emit structured, immutable audit events so approvals, corrections, CTC updates, and access evaluations can be traced for 7 years.

4. Operational Resilience  
   - Application design must support high load (minimum 1000 concurrent users), predictable performance, and rapid recovery from incidents.

5. Configurable, Observable, Testable  
   - All platform behaviors must be driven by configuration, monitored through standardized telemetry, and validated through automated tests and CI policies.

---

## 2. Identity and Access Guardrails

### 2.1 Session and Authentication
- Enforce 15-minute idle session timeout at the application tier (align with IdP/SSO if present).
- Require re-authentication for sensitive operations.
- Trigger account lockout after three consecutive failed attempts.
- Allow unlock only by IT Admin through a documented, auditable procedure.

### 2.2 Role-Based Access Control (RBAC)
- Support multiple roles per user (primary and secondary).
- Restrict full PAN visibility to HR and IT Admin roles only; others must see masked values.
- Enforce field-level authorization at both UI and API layers.

### 2.3 Access Logging
- Log all login failures, lockouts, unlocks, privilege checks, and role changes centrally with user, timestamp, and source.

---

## 3. Data Protection and Privacy Guardrails

### 3.1 Data Masking and Sensitive Attributes
- Mask PAN using a fixed pattern everywhere: UI, API payloads, logs, and exports.
- Do not log raw PAN or other sensitive identifiers; log masked values and reference IDs only.
- Record every unmasking event with actor, reason, and timestamp.

### 3.2 Data Minimization
- Collect only data required for the appraisal process.
- Restrict exported fields by role-based column policies.

### 3.3 Data Retention and Deletion
- Retain audit logs for 7 years.
- Delete transient artifacts (temp files, queues) after processing completes.
- Maintain deletion workflows and automate where feasible.

### 3.4 Standards Alignment
- Align technical controls with security best practices (data masking, DLP, monitoring).
- Satisfy privacy principles (purpose limitation, minimization, integrity, confidentiality).
- Implement reasonable security safeguards and breach notification procedures.

---

## 4. Application Logic Guardrails

### 4.1 CTC Workflow Integrity
- Show CTC details only within the CTC stage; never in earlier stages.
- Auto-calculate total CTC; log all edits with before/after diffs and actor.

### 4.2 Commenting and Approval Lifecycle
- Allow edits to manager comments only until final approval.
- Lock comments after approval and prevent retroactive changes; keep append-only revisions when needed.

### 4.3 Assignment Behaviors
- Do not support bulk appraisal assignment in version 1.
- Log each assignment action with timestamp, actor, and employee-manager pair.

### 4.4 Feedback and Notifications
- Notify employees when managers submit comments.
- Persist durable evidence for notification send and receipt outcomes.

---

## 5. Reporting and Export Guardrails

### 5.1 Policy-Aware Export Service
- Route all exports through a single export service that applies masking and role-based column policies.
- Prohibit exporting unmasked PAN.

### 5.2 Report Filtering and Scope
- Apply RBAC consistently across dashboards and ad-hoc reports.
- Minimize exposure to only necessary fields; default-deny sensitive fields.

---

## 6. Observability, Monitoring, and Audit Guardrails

### 6.1 Logging
- Emit structured logs for login, lockout, unlock, comments, CTC updates, feedback, and exports.
- Include correlation IDs and request identifiers for traceability.

### 6.2 Monitoring and Alerts
- Alert on repeated unauthorized attempts and other anomaly patterns.
- Track performance SLOs; at minimum, p95 page response time < 2 seconds during peak.

### 6.3 SIEM Integration
- Forward security-relevant logs to the enterprise SIEM.
- Maintain anomaly detection rules for failed logins, abnormal CTC edit rates, export spikes, and unlock events.

---

## 7. Performance and Scalability Guardrails

### 7.1 Capacity and Throughput
- Support at least 1000 concurrent users during appraisal windows without degradation.

### 7.2 Performance SLAs
- p95 response time for key pages shall be < 2 seconds under peak test conditions.

### 7.3 Resource Isolation
- Isolate heavy operations (exports, batch jobs) from interactive request paths; employ back-pressure and queues.

---

## 8. Secure SDLC Guardrails

### 8.1 CI and Pipeline Enforcement
- Enforce masking, session timeout, and export policies through CI jobs.
- Block commits that introduce hardcoded secrets or raw PAN logging.
- Require security review for PRs that touch authentication, RBAC, CTC logic, and export policy.

### 8.2 Testing Requirements
- Maintain automated tests for:
  - Masking across UI, API, exports, and logs
  - 15-minute idle timeout behavior
  - Comment immutability after approval
  - Export role-based column policies
  - CTC auto-calculation accuracy
- Run performance tests every release and publish results.

---

## 9. Secrets, Configuration, and Environment Guardrails

### 9.1 Secrets Management
- Store secrets in a dedicated vault; do not commit secrets to the repository.
- Rotate secrets according to policy and monitor for unauthorized access.

### 9.2 Configuration as Code
- Manage authentication, session, export, and masking settings as code (IaC).
- Require peer review for config changes; enforce environment parity where possible.

---

## 10. Backup, Disaster Recovery, and Business Continuity Guardrails

### 10.1 Backups
- Back up application data and audit logs at policy-defined intervals.
- Perform and document quarterly restore tests.

### 10.2 DR Objectives
- Define RPO/RTO aligned with appraisal timelines.
- Maintain DR runbooks and test them annually at minimum.

---

## 11. Vendor and Integration Guardrails

### 11.1 Third-Party Services
- Require vendors to support masking, structured logging, retention controls, and auditability.
- Include breach notification, audit rights, and compliance alignment in contracts.
- Validate that vendors do not log or expose unmasked PAN.

---

## 12. Governance, Evidence, and Audit Readiness

### 12.1 Control-to-Evidence Register
- Maintain a register that maps each guardrail to:
  - Control owner
  - Evidence type and location
  - Review frequency
  - Status, last reviewed, next due
- Update the register every release.

### 12.2 Risk Review Cadence
- Review and update the Security Risk Register each release; adjust mitigations and residual risk ratings.

### 12.3 Change Control
- Manage guardrail updates through pull requests and multi-owner approvals (Security, HR/Compliance, Infra).

---

## 13. Go-Live Compliance Deliverables

1. Statement of Applicability (ISO alignment)  
2. Updated Security Risk Register  
3. Control-to-Evidence Register with links  
4. Test coverage report (masking, timeout, immutability, export, CTC)  
5. Performance test report (concurrency and SLOs)  
6. SIEM integration checklist and alert definitions  
7. DR runbooks and last drill results  
8. Export policy definitions and masked column matrix  
9. RBAC model and permission catalog  
10. Masking policy specification and test proofs

---

## Appendix A: Ownership and Review Cadence (Template)

| Guardrail ID | Area | Owner | Evidence (what) | Evidence (where) | Review Frequency | Status | Last Reviewed | Next Due |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| GA-ID-01 | Session Timeout | IT Security | Config file with IDLE_TIMEOUT=15 | config/app.yaml | Per Release | Pending |  |  |
| GA-DP-01 | PAN Masking | Security | API tests, UI screenshots, log samples | evidence/masking/ | Per Release | Pending |  |  |
| GA-APP-02 | Comment Immutability | Manager | WORM log reference, test report | evidence/immutability/ | Per Release | Pending |  |  |
| GA-OPS-01 | Performance SLO | IT Infra | Load test report | evidence/performance/ | Per Release | Pending |  |  |
| GA-OBS-02 | Audit Retention | Compliance | Storage class, lifecycle policy | platform/storage/ | Quarterly | Pending |  |  |
| GA-CMP-01 | ISO SoA | Compliance | SoA document | docs/iso-soa.xlsx | Quarterly | Pending |  |  |

---

### Usage Notes
- Treat each guardrail as a non-functional requirement and include acceptance checks in design documents.
- Enforce guardrails in CI with policy-as-code rules.
- Keep the Control-to-Evidence Register current to remain audit-ready.
