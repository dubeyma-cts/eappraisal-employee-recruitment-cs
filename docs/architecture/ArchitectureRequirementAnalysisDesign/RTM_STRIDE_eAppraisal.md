
# RTM and STRIDE Threat-to-Control Mapping

Generated on: 2026-02-27

This document contains clean, GitHub-compatible RTM and STRIDE tables with **no special characters**.

## 1. Functional Requirements — RTM

| Use-Case ID | Requirement Area | Confirmed Requirement | Source | Impacted Modules | Priority |
|-------------|------------------|------------------------|--------|------------------|----------|
| UC-01 | Authentication | Session timeout = 15 min inactivity | Clarification | Login, Session Mgmt | High |
| UC-02 | Employee Master | PAN/Passport = view-only | Clarification | Employee Profile | High |
| UC-03 | Appraisal Assignment | Bulk assignment not required | Clarification | HR Assignment | Medium |
| UC-04 | Manager Review | Manager must not see CTC in review | Clarification | Manager Review Screen | High |
| UC-05 | Manager Comments | Comments editable only before approval | Clarification | Manager Workflow | High |
| UC-06 | Employee Feedback | Email notification required | Clarification | Notification Service | Medium |
| UC-07 | CTC Entry | Standard CTC structure | Clarification | HR/Finance | High |
| UC-07 | CTC Entry | Auto-calc total CTC | Clarification | CTC Engine | High |
| UC-08 | Reports | Export to Excel | Clarification | Reporting Engine | Medium |
| UC-09 | Account Unlock | Manual unlock by IT | Clarification | IT Admin | Medium |

## 2. Functional — STRIDE Threat Mapping

| Use-Case ID | Threat Type | Threat Description | Mitigation |
|-------------|-------------|--------------------|------------|
| UC-01 | Spoofing | Unauthorized login via idle sessions | Session timeout |
| UC-01 | Repudiation | User denies login attempt | Audit logging |
| UC-02 | Info Disclosure | PAN/Passport leakage | View-only enforcement |
| UC-03 | DoS | Bulk assignment misuse | Disable bulk actions |
| UC-04 | Info Disclosure | Manager views CTC early | Hide CTC fields |
| UC-05 | Tampering | Comments changed post-approval | Lock after approval |
| UC-06 | Repudiation | Employee denies comment receipt | Email notification |
| UC-07 | Tampering | Incorrect CTC entries | Auto-calculation |
| UC-08 | Info Disclosure | Sensitive fields in export | Mask during export |
| UC-09 | Elevation | Unauthorized unlock | Restrict to IT admin |

## 3. Non-Functional Requirements — RTM

| NFR ID | Requirement Area | Confirmed Requirement | Impact | Priority |
|--------|------------------|------------------------|--------|----------|
| NFR-UC-01 | Security | Central logging for failed attempts | Security Logs | High |
| NFR-UC-02 | RBAC | Multi-role support | RBAC Engine | High |
| NFR-UC-02 | RBAC | Alerts for repeated unauthorized attempts | Monitoring | High |
| NFR-UC-04 | Performance | Support 1000 concurrent users | Infra Sizing | High |
| NFR-UC-04 | Monitoring | Integrate with SIEM | Observability | High |
| NFR-UC-05 | Data Security | PAN masking pattern required | Data Masking | High |
| NFR-UC-05 | Data Security | Managers see masked PAN only | UI Layer | High |
| NFR-UC-06 | Audit | Retain logs 7 years | Compliance | High |
| NFR-UC-06 | Audit | Exportable logs | Reporting | High |

## 4. Non-Functional — STRIDE Threat Mapping

| NFR ID | Threat Type | Threat Description | Mitigation |
|--------|-------------|--------------------|------------|
| NFR-UC-01 | Spoofing | Login spoofing | Central logging |
| NFR-UC-01 | Repudiation | Failed attempts not traceable | Log failed attempts |
| NFR-UC-02 | Elevation | Unauthorized privilege gains | Strong RBAC |
| NFR-UC-02 | Repudiation | Repeated access attempts | SIEM alerts |
| NFR-UC-04 | DoS | System slowdown under load | Load testing |
| NFR-UC-04 | Info Disclosure | Missing monitoring | SIEM integration |
| NFR-UC-05 | Info Disclosure | PAN exposed | Mask PAN |
| NFR-UC-06 | Repudiation | No audit trail | Store logs for 7 years |

