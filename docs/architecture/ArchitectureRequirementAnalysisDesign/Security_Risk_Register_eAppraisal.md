
# Consolidated Security Risk Register

Generated on: 2026-02-27

This register consolidates risks across Functional (FR) and Non-Functional (NFR) requirements. It only reflects items confirmed by stakeholders in clarifications, plus directly stated behaviors in the use-cases.

**Scales used**
- Likelihood: Low (L), Medium (M), High (H)
- Impact: Low (L), Medium (M), High (H)
- Inherent Risk: pre-control rating (L/M/H)
- Residual Risk: post-control rating (L/M/H)

## A. Functional Requirements Risks

| Risk ID | Source (UC) | STRIDE | Risk Description | Existing/Planned Controls | Control Type | Likelihood | Impact | Inherent Risk | Residual Risk | Owner | Status | Target Date |
|--------|--------------|--------|------------------|---------------------------|--------------|------------|--------|---------------|---------------|-------|--------|------------|
| FR-01 | UC-01 | S | Unauthorized use of logged-in session on idle terminals | Session timeout 15 min; re-authenticate on sensitive actions | Preventive | M | M | M | L | IT Security | Open | TBD |
| FR-02 | UC-01 | R | Dispute over login attempts with no trace | Login success/failure audit logging | Detective | M | M | M | L | IT Security | Open | TBD |
| FR-03 | UC-02 | I | Exposure of PAN/Passport to employees | View-only for employees; data masking on UI | Preventive | M | H | H | M | HR | Planned | TBD |
| FR-04 | UC-03 | D | Bulk assignment creating load spikes or errors | Disable bulk assignment (single assignment only) | Preventive | L | M | M | L | HR | Planned | TBD |
| FR-05 | UC-04 | I | Manager views CTC before CTC step | Hide CTC fields in review stage | Preventive | M | H | H | L | HR | Planned | TBD |
| FR-06 | UC-05 | T | Editing comments after final approval | Lock comments post-approval; capture edit audit trail | Preventive/Detective | M | M | M | L | Manager | Planned | TBD |
| FR-07 | UC-06 | R | Employee disputes receiving manager comments | Send email notification on manager submission; log event | Detective | M | M | M | L | HR | Planned | TBD |
| FR-08 | UC-07 | T | Incorrect CTC due to manual edits | Auto-calculate total CTC; field validation; audit CTC updates | Preventive/Detective | M | H | H | M | Finance | Planned | TBD |
| FR-09 | UC-08 | I | Sensitive data leakage via Excel export | Mask sensitive fields in exports; restrict export columns | Preventive | M | H | H | M | HR | Planned | TBD |
| FR-10 | UC-09 | E | Unauthorized account unlock | Manual unlock restricted to IT Admin; record unlock audit | Preventive/Detective | M | H | H | M | IT Admin | Open | TBD |

## B. Non-Functional Requirements Risks

| Risk ID | Source (NFR UC) | STRIDE | Risk Description | Existing/Planned Controls | Control Type | Likelihood | Impact | Inherent Risk | Residual Risk | Owner | Status | Target Date |
|--------|------------------|--------|------------------|---------------------------|--------------|------------|--------|---------------|---------------|-------|--------|------------|
| NFR-01 | NFR-UC-01 | S,R | Spoofed login attempts and lack of forensic trail | Centralized logging of failed attempts; monitoring | Detective | M | M | M | L | IT Security | Planned | TBD |
| NFR-02 | NFR-UC-02 | E | Privilege escalation via role misconfiguration | Strong RBAC; least privilege; multi-role support with review | Preventive | M | H | H | M | HR/IT Security | Planned | TBD |
| NFR-03 | NFR-UC-02 | R | Repeated unauthorized access attempts go unnoticed | SIEM alerts for repeated denials | Detective | M | M | M | L | IT Security | Planned | TBD |
| NFR-04 | NFR-UC-04 | D | Outage or slowdown under peak load | Capacity planning for 1000 users; performance testing | Preventive | M | H | H | M | IT Infra | Planned | TBD |
| NFR-05 | NFR-UC-04 | I | Insufficient telemetry during incidents | Integrate monitoring with SIEM | Detective | M | M | M | L | IT Infra | Planned | TBD |
| NFR-06 | NFR-UC-05 | I | PAN exposure in UI or logs | Fixed-pattern masking in UI and exports | Preventive | M | H | H | M | Security | Planned | TBD |
| NFR-07 | NFR-UC-06 | R | Inability to prove actions during disputes | Retain audit logs 7 years; exportable logs | Detective | M | M | M | L | Compliance | Planned | TBD |

## C. Mitigation Backlog (Next Actions)

| Item | Action | Owner | Due |
|------|--------|-------|-----|
| MB-01 | Implement session timeout and test idle logout flows | IT Security | Sprint +1 |
| MB-02 | Apply PAN masking pattern across UI, exports | Security | Sprint +2 |
| MB-03 | Enable audit logs for login, comments, CTC changes | IT Security | Sprint +2 |
| MB-04 | Wire SIEM alerts for repeated unauthorized attempts | IT Security | Sprint +2 |
| MB-05 | Run 1000-user performance test and tune | IT Infra | Sprint +3 |

## D. Assumptions
- Controls are implemented as per confirmed clarifications and use-case behaviors.
- Ratings reflect current understanding and will be refined during design and testing.
