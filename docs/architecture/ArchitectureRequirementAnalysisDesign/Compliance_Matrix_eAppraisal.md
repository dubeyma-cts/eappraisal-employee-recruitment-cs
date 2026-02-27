# Consolidated Compliance Matrix (e-Appraisal System) - CLEAN

Generated on: 2026-02-27

Scope: This matrix maps confirmed requirements and use-cases to ISO/IEC 27001:2022 Annex A, SOC 2 (2017 Trust Services Criteria), GDPR (Articles 5 and 32), and India DPDP Act/Rules. This file intentionally contains no citations, special characters, or non-ASCII symbols so it renders cleanly on GitHub and in basic viewers.

## A. Functional Requirements - Compliance Matrix

| Use-Case ID | Confirmed Requirement | ISO/IEC 27001:2022 Annex A | SOC 2 TSC | GDPR | DPDP Act/Rules | Evidence/Notes |
|-------------|-----------------------|-----------------------------|-----------|------|---------------|----------------|
| UC-01 | Session timeout 15 min (idle logout) | Monitoring activities; authentication and session controls | CC6 Logical access; CC7 System operations | Article 32 security of processing | Security safeguards in Act/Rules | Align app and SSO idle timeout; test idle logout |
| UC-02 | PAN/Passport view-only for employees; masking | Data masking; data loss prevention; access control | CC6 access; CC7 monitoring | Article 5 integrity/confidentiality; Article 32 | Security safeguards; display minimisation | Field-level masking; HR-only unmasked access with audit |
| UC-03 | No bulk assignment (single assign only) | Configuration/change management; least privilege | CC8 change management; CC7 ops | Article 5 data minimisation | Purpose-limited processing | Guard-rails in UI/service to prevent mass actions |
| UC-04 | Hide CTC during review (no early exposure) | Access control; monitoring attempted access | CC6 logical access | Article 32 confidentiality; Article 5 minimisation | Need-to-know access | Role-based UI/API filters; negative tests in UAT |
| UC-05 | Comments editable only before final approval | Secure coding; monitoring; immutability of approvals | CC7 ops; CC8 change | Article 32 integrity | Auditability and safeguards | Write-once log for approvals; prevent retro edits |
| UC-06 | Email notify employee when manager submits comments | Logging and monitoring of events | CC7 ops notifications | Article 5 transparency and accuracy | Notice and grievance mechanisms | Store notification events; failure alerts |
| UC-07 | CTC auto-calc; audit CTC updates | Configuration management; monitoring; secure coding | CC8 change; CC7 ops; processing integrity | Article 32 integrity and resilience | Accountability for processing accuracy | Dual-control on CTC changes; checksum totals |
| UC-08 | Export to Excel (mask sensitive fields) | Data masking; DLP | CC6 and CC7 | Article 32 security; Article 5 minimisation/confidentiality | Security safeguards; purpose-limited export | Export layer applies role-based column policy |
| UC-09 | Manual unlock by IT Admin with audit | Access governance; monitoring unlock events | CC6 access; CC7 ops | Article 32 access control and accountability | Oversight and logging expectations | Break-glass procedure; ticket linkage required |

## B. Non-Functional Requirements - Compliance Matrix

| NFR UC ID | Confirmed Requirement | ISO/IEC 27001:2022 Annex A | SOC 2 TSC | GDPR | DPDP Act/Rules | Evidence/Notes |
|-----------|-----------------------|-----------------------------|-----------|------|---------------|----------------|
| NFR-UC-01 | Central logging of failed login attempts | Monitoring activities | CC7 system operations | Article 32 | Security safeguards; incident reporting | Retain failed login logs; correlate with SIEM |
| NFR-UC-02 | Multi-role RBAC with least privilege | Access control and authentication | CC6 logical access | Article 32 access control | Purpose limitation and access governance | Role model and periodic access reviews |
| NFR-UC-02 | Alerts for repeated unauthorized attempts | Monitoring and alerting | CC7 anomaly detection and alerting | Article 32 testing/evaluation | Rules on monitoring posture | SIEM threshold-based lock and alert to SOC |
| NFR-UC-04 | Support 1000 concurrent users; SIEM integration | ICT readiness; monitoring | Availability and security criteria | Article 32 availability and resilience | Safeguards and monitoring integration | Load test report; SIEM onboarding checklist |
| NFR-UC-05 | PAN masking pattern; managers see masked only | Data masking; DLP | Confidentiality criteria; CC6/CC7 | Article 32 pseudonymisation; Article 5 confidentiality | Data display minimisation | Unit tests on masking pattern; export masking verified |
| NFR-UC-06 | Audit retention 7 years; exportable logs | Monitoring and information deletion (lifecycle) | CC7 logging; CC1 accountability | Article 5(2) accountability; Article 32 | Governance and documentation | Retention policy; legal hold exceptions documented |

---
Implementation Note: Final ISO control selection should be documented in the Statement of Applicability (SoA). Keep evidence references (log names, report IDs, tickets) for each row to support audits.