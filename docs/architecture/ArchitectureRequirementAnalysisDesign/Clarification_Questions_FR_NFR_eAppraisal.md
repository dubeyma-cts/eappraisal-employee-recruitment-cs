
# Clarification Questions for Functional & Non-Functional Requirements

## Functional Requirement Clarification Questions
| Use-Case ID | Requirement Area | Question | Response (Example) | Stakeholder | Status |
|-------------|------------------|----------|---------------------|-------------|--------|
| UC-01 | Authentication | Should login support MFA in future? | MFA not required in MVP. | IT Security | Open |
| UC-01 | Authentication | What is the session timeout duration? | Likely 15 mins inactivity. | IT Security | Open |
| UC-02 | Employee Master | Should Employees be able to view but not edit PAN/Passport? | Yes, view-only. | HR | Confirmed |
| UC-02 | Employee Master | Should HR upload documents (PAN/Passport scans)? | Not in MVP. | HR | Open |
| UC-03 | Appraisal Assignment | Should HR assign multiple employees in bulk? | Bulk upload not required initially. | HR | Open |
| UC-04 | Manager Review | Should manager see employee salary/CTC at this stage? | No, only in CTC step. | Manager | Confirmed |
| UC-05 | Manager Comments | Should manager be able to edit comments after submission? | Only before final approval. | Manager | Open |
| UC-06 | Employee Feedback | Should employee be notified when manager submits comments? | Email notification planned. | Employee | Open |
| UC-07 | CTC Entry | Are CTC components configurable per region? | Same structure across company. | HR/Finance | Confirmed |
| UC-07 | CTC Entry | Should system compute total CTC automatically? | Yes, auto-calc needed. | Finance | Confirmed |
| UC-08 | Reports | Should reports be exportable to Excel? | Yes, needed. | HR | Confirmed |
| UC-09 | Account Unlock | Should unlock require ticketing workflow? | Manual unlock via IT. | IT Admin | Confirmed |

## Non-Functional Requirement Clarification Questions
| Use-Case ID | Requirement Area | Question | Response (Example) | Stakeholder | Status |
|-------------|------------------|----------|---------------------|-------------|--------|
| NFR-UC-01 | Security | Should users receive email notifications when account is locked? | Optional, to be decided. | IT Security | Open |
| NFR-UC-01 | Security | Should failed attempts be logged centrally? | Yes, part of audit. | IT Security | Confirmed |
| NFR-UC-02 | RBAC | Can a user have multiple roles at once? | Yes, primary + secondary. | HR | Confirmed |
| NFR-UC-02 | RBAC | Should unauthorized access attempts raise alerts? | Only for repeated attempts. | IT Security | Open |
| NFR-UC-03 | UI/UX | Should UI follow WCAG accessibility standards? | Preferably yes. | UX Team | Open |
| NFR-UC-03 | UI/UX | Should app support dark mode? | Not required. | UX Team | Closed |
| NFR-UC-04 | Performance | What is the expected peak load? | Up to 1000 concurrent users. | IT Infra | Confirmed |
| NFR-UC-04 | Performance | Should monitoring integrate with existing SIEM? | Yes, using existing stack. | IT Infra | Confirmed |
| NFR-UC-05 | Data Security | Should PAN masking follow fixed pattern (e.g., XXXX-XX-1234)? | Yes. | Security | Confirmed |
| NFR-UC-05 | Data Security | Should masked data be shown to Managers? | Yes, Manager sees masked PAN. | HR | Confirmed |
| NFR-UC-06 | Audit | What is retention period for audit logs? | 7 years. | Compliance | Confirmed |
| NFR-UC-06 | Audit | Should audit logs be exportable? | Yes, via Admin console. | IT Admin | Confirmed |

