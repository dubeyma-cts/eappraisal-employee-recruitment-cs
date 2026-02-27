
# Business Requirements Document (BRD): e-Appraisal System

## 1. Executive Summary
Nano Technologies requires an online e‑Appraisal system to replace its manual, file‑based performance appraisal process. The new system will streamline collaboration among HR, Managers (Appraisers), and Employees (Appraisees), enable end‑to‑end visibility of appraisal status, and capture appraisal inputs and compensation changes digitally.

## 2. Background & Problem Statement
Currently appraisal files physically move between HR → Manager → HR → Employee → HR → Manager, with no tracking of file custody, delays, or visibility. Offices across regions worsen coordination challenges.

## 3. Business Objectives
- Digitize the appraisal workflow.
- Enable visibility into upcoming and in-process appraisals.
- Securely record comments, decisions, and CTC breakdown.
- Ensure consistent UI and minimal clicks.
- Implement account lockout after three failed logins.

## 4. Scope
### In Scope
- Role-based login.
- HR employee master maintenance.
- Manager comments, employee feedback, promotion and CTC entry.
- Upcoming & In-Process appraisal tracking.
- Consistent UI components.
- Security lockout rule.

### Out of Scope
- Multi-rater reviews, KPIs.
- Payroll integration automation.
- Mobility apps.

## 5. Stakeholders
- HR
- Manager (Appraiser)
- Employee (Appraisee)
- IT/Admin

## 6. Current vs Future State
### As-Is
Manual routing of physical files with no tracking.

### To-Be
Digital workflow with role-based stages, visibility, and audit trails.

## 7. Functional Requirements
### 7.1 Authentication
- Users must log in.
- 3-strike lockout.
- Role-based menus.

### 7.2 HR Features
- Maintain employee master: Name, Address, City, Phone, Mobile, Email, DOB, Gender, Marital Status, DOJ, Passport, PAN, Work Experience, Reports To, Department.
- View & assign upcoming appraisals.

### 7.3 Manager Features
- View upcoming appraisals.
- View employee personal info (read-only).
- Enter comments (achievements, gaps, suggestions).
- View employee feedback.
- Enter promotion decision and CTC (Basic, DA, HRA, Food Allowance, PF, Next Appraisal Date).

### 7.4 Employee Features
- Edit personal info except Reports To, Department.
- View manager comments.
- Submit feedback.

## 8. Non-Functional Requirements
- Lockout after invalid logins.
- Role-based access.
- Consistent look-and-feel across pages.
- Performance & usability expectations.

## 9. Data & Security
- Entities: Employee, Appraisal Case, Comments, CTC, User Roles.
- PII storage and masking (e.g., PAN).
- Manual unlock of locked accounts.

## 10. Reporting
- Upcoming Appraisals.
- In-Process Appraisals.
- Completed Appraisals with CTC updates.
- Audit logs.

## 11. Assumptions
- One manager per employee.
- Manual payroll updates.

## 12. Risks
- Master data issues.
- User adoption.

## 13. Acceptance Criteria
- HR can assign appraisals.
- Manager completes appraisal cycle.
- Employee can provide feedback.

## Appendix A: Sample RTM
| Req ID | Requirement | Source | UAT |
|-------|-------------|--------|------|
| FR-HR-01 | Employee master | Case Study | TC-HR-01 |
| FR-MGR-05 | Promotion & CTC | Case Study | TC-MGR-05 |
| FR-EMP-04 | Employee Feedback | Case Study | TC-EMP-04 |
| FR-Auth-02 | 3-strike lockout | Security | TC-SEC-02 |

