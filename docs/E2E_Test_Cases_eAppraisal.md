# e-Appraisal System - End-to-End Test Case Document

**Application:** e-Appraisal System (Nano Technologies Pvt. Ltd.)
**Version:** 1.0
**Date:** 2026-03-01
**Environment:** API on http://localhost:5100 | Web on http://localhost:5200
**Database:** SQLite (eappraisal.db)

---

## Table of Contents

1. [Test Environment Setup](#1-test-environment-setup)
2. [Demo Accounts](#2-demo-accounts)
3. [TC-01: Admin - Create User Account](#3-tc-01-admin-create-user-account)
4. [TC-02: User Login and Role-Based Routing](#4-tc-02-user-login-and-role-based-routing)
5. [TC-03: Three-Strike Account Lockout](#5-tc-03-three-strike-account-lockout)
6. [TC-04: Admin Unlock Locked Account](#6-tc-04-admin-unlock-locked-account)
7. [TC-05: Session Timeout (15 Minutes)](#7-tc-05-session-timeout)
8. [TC-06: HR - Employee Master Management](#8-tc-06-hr-employee-master-management)
9. [TC-07: HR - Appraisal Cycle Management](#9-tc-07-hr-appraisal-cycle-management)
10. [TC-08: HR - Assign Appraisals to Employees](#10-tc-08-hr-assign-appraisals)
11. [TC-09: Manager - Review Employee Personal Info](#11-tc-09-manager-review-personal-info)
12. [TC-10: Manager - Enter Comments](#12-tc-10-manager-enter-comments)
13. [TC-11: Employee - Submit Feedback](#13-tc-11-employee-submit-feedback)
14. [TC-12: Manager - Finalize Appraisal with CTC](#14-tc-12-manager-finalize-appraisal)
15. [TC-13: Comment Immutability Post-Finalization](#15-tc-13-comment-immutability)
16. [TC-14: CTC Auto-Calculation](#16-tc-14-ctc-auto-calculation)
17. [TC-15: PAN Masking by Role](#17-tc-15-pan-masking-by-role)
18. [TC-16: Employee - Profile Self-Edit Restrictions](#18-tc-16-employee-profile-self-edit)
19. [TC-17: Reports - Upcoming, In-Process, Completed](#19-tc-17-reports)
20. [TC-18: Audit Trail Verification](#20-tc-18-audit-trail)
21. [TC-19: Notification Queue Verification](#21-tc-19-notification-queue)
22. [TC-20: Full End-to-End Appraisal Workflow](#22-tc-20-full-e2e-workflow)
23. [TC-21: Negative and Edge Case Scenarios](#23-tc-21-negative-and-edge-cases)

---

## 1. Test Environment Setup

### Pre-requisites

| Item | Requirement |
|------|-------------|
| .NET SDK | 8.0 or later |
| Browser | Chrome / Edge / Firefox (latest) |
| Ports | 5100 (API), 5200 (Web) must be free |
| Database | SQLite (auto-created on first run) |

### Steps to Start the Application

1. Double-click `start.bat` in the project root folder
2. Wait for the console to display "Press any key to stop all services"
3. Browser will automatically open to http://localhost:5200

### Alternative Manual Start

```
Terminal 1: cd src/Gateways/eAppraisal.Api && dotnet run --urls http://localhost:5100
Terminal 2: cd src/Web/eAppraisal.Web && dotnet run --urls http://localhost:5200
```

---

## 2. Demo Accounts

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@nanotechnologies.com | Admin@123 |
| HR | sunita.rao@nanotechnologies.com | Hr@12345 |
| Manager | ramesh.kumar@nanotechnologies.com | Manager@123 |
| Employee 1 | priya.sharma@nanotechnologies.com | Employee@123 |
| Employee 2 | anil.patel@nanotechnologies.com | Employee@123 |

### Seeded Data

- **Appraisal Cycle:** "2025 Annual Appraisal" (Apr 1 - Jun 30, 2025), State: Open
- **Employees:** Ramesh Kumar (Manager, Engineering), Priya Sharma (Engineering), Anil Patel (QA), Sunita Rao (HR)
- **Manager Hierarchy:** Priya and Anil report to Ramesh

---

## 3. TC-01: Admin - Create User Account

**Objective:** Verify that Admin can create new user accounts with any role (Employee, Manager, HR, Admin). Self-registration has been removed; only Admin can create users.

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to http://localhost:5200 | Login page is displayed. No "Register" link visible. Message: "Contact your Admin to create a new account." |
| 2 | Login as Admin: admin@nanotechnologies.com / Admin@123 | Admin Dashboard with "User Management", "Create User", and "Reports" cards |
| 3 | Click "Create User" in sidebar (or dashboard card) | Create User form with fields: Full Name, Email, Role (dropdown: Employee/Manager/HR/Admin), Password, Confirm Password |
| 4 | Leave all fields empty and click "Create User" | Validation errors shown for required fields |
| 5 | Enter Full Name: "Deepa Singh", Email: "deepa.singh@nanotechnologies.com", Role: "Employee", Password: "Deepa@123", Confirm Password: "Deepa@123" | |
| 6 | Click "Create User" | Success message: "User 'deepa.singh@nanotechnologies.com' created successfully with role 'Employee'." Redirected to Users list |
| 7 | Verify Deepa Singh appears in Users list | Email: deepa.singh@nanotechnologies.com, Role: Employee, Status: Active |
| 8 | Logout | Login page |
| 9 | Login with deepa.singh@nanotechnologies.com / Deepa@123 | Redirected to Employee Dashboard |
| 10 | Click "My Profile" in sidebar | Profile page shows: Name: Deepa Singh, Department: Unassigned |
| 11 | Logout. Login as Admin again | Admin Dashboard |
| 12 | Try creating user with same email "deepa.singh@nanotechnologies.com" | Error: "An account with this email already exists." |
| 13 | Try creating user with password "weak" | Error about password requirements (uppercase, digit, special char) |
| 14 | Create a Manager: Full Name: "Ravi Nair", Email: "ravi.nair@nanotechnologies.com", Role: "Manager", Password: "Manager@123" | Success message with role 'Manager' |
| 15 | Create an HR user: Full Name: "Anjali Gupta", Email: "anjali.gupta@nanotechnologies.com", Role: "HR", Password: "Hr@12345" | Success message with role 'HR' |
| 16 | Verify all 3 new users in User Management list | All 3 shown with correct roles and Active status |

---

## 4. TC-02: User Login and Role-Based Routing

**Objective:** Verify login works for all roles and routes to the correct dashboard.

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to http://localhost:5200/Account/Login | Login page displayed with demo account info |
| 2 | Login as **Admin**: admin@nanotechnologies.com / Admin@123 | Redirected to `/Admin` - Admin Dashboard with "User Management", "Create User", and "Reports" cards |
| 3 | Verify sidebar shows: Dashboard, User Management, Create User, Reports | Sidebar shows Admin-specific navigation |
| 4 | Logout | Redirected to Login page |
| 5 | Login as **HR**: sunita.rao@nanotechnologies.com / Hr@12345 | Redirected to `/HR` - HR Dashboard with Employees, Cycles, Assign cards |
| 6 | Verify sidebar shows: Dashboard, Employees, Appraisal Cycles, Assign Appraisals, Reports | Sidebar shows HR-specific navigation |
| 7 | Logout | Redirected to Login page |
| 8 | Login as **Manager**: ramesh.kumar@nanotechnologies.com / Manager@123 | Redirected to `/Manager` - Manager Dashboard with My Appraisals, Reports cards |
| 9 | Verify sidebar shows: Dashboard, My Appraisals, Reports | Sidebar shows Manager-specific navigation |
| 10 | Logout | Redirected to Login page |
| 11 | Login as **Employee**: priya.sharma@nanotechnologies.com / Employee@123 | Redirected to `/Employee` - Employee Dashboard with My Profile, My Appraisals, Reports cards |
| 12 | Verify sidebar shows: Dashboard, My Profile, My Appraisals, Reports | Sidebar shows Employee-specific navigation |
| 13 | Try login with invalid email: "invalid@test.com" / "Test@123" | Error: "Invalid email or password." |
| 14 | Try login with valid email, wrong password | Error: "Invalid email or password." |

---

## 5. TC-03: Three-Strike Account Lockout

**Objective:** Verify account locks after 3 consecutive failed login attempts.

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Login page | Login page displayed |
| 2 | Enter anil.patel@nanotechnologies.com with wrong password "Wrong@111" and submit | Error: "Invalid email or password." |
| 3 | Enter same email with wrong password "Wrong@222" and submit | Error: "Invalid email or password." |
| 4 | Enter same email with wrong password "Wrong@333" and submit | Error: "Account locked after 3 failed attempts. Contact IT Admin." |
| 5 | Try login with correct password "Employee@123" | Error: "Account is locked. Contact IT Admin." (Account remains locked even with correct password) |
| 6 | Login as Admin: admin@nanotechnologies.com / Admin@123 | Admin Dashboard displayed |
| 7 | Go to Admin > User Management | Users list displayed. Anil Patel shows Status: "Locked", Failed Attempts: 3 |

---

## 6. TC-04: Admin Unlock Locked Account

**Objective:** Verify Admin can unlock a locked account.

**Pre-condition:** TC-03 completed (Anil Patel is locked)

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Login as Admin: admin@nanotechnologies.com / Admin@123 | Admin Dashboard |
| 2 | Navigate to User Management | Users list with Anil Patel showing "Locked" badge (red) |
| 3 | Click "Unlock" button next to Anil Patel | Page refreshes. Anil Patel now shows "Active" badge (green), Failed Attempts: 0 |
| 4 | Logout | Login page |
| 5 | Login as anil.patel@nanotechnologies.com / Employee@123 | Redirected to Employee Dashboard (login successful) |
| 6 | Login as Admin again | Admin Dashboard |
| 7 | Go to User Management, click "Lock" next to Priya Sharma | Priya shows "Locked" badge |
| 8 | Logout, try login as Priya | Error: "Account is locked. Contact IT Admin." |
| 9 | Login as Admin, unlock Priya | Priya shows "Active" again |

---

## 7. TC-05: Session Timeout

**Objective:** Verify 15-minute idle session timeout behavior.

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Login as HR: sunita.rao@nanotechnologies.com / Hr@12345 | HR Dashboard |
| 2 | Navigate to HR > Employees | Employees list loaded (API call succeeds) |
| 3 | Wait 15+ minutes without any interaction | Session expires (no visible change yet) |
| 4 | Click any sidebar link (e.g., Reports) | Redirected to Login page (session expired, API returns 401) |
| 5 | Re-login with same credentials | HR Dashboard displayed, full access restored |

**Note:** The session cookie has `IdleTimeout = 15 minutes` and `SlidingExpiration = true`. Each page interaction resets the timeout.

---

## 8. TC-06: HR - Employee Master Management

**Objective:** Verify HR can create, view, and edit employee records.

### TC-06a: View Employee List

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Login as HR | HR Dashboard |
| 2 | Click "Employees" in sidebar | Employee list displayed with columns: Name, Email, Department, City, Manager, PAN, Actions |
| 3 | Verify 4 employees listed | Ramesh Kumar, Priya Sharma, Anil Patel, Sunita Rao |
| 4 | Verify PAN column shows full PAN for HR role | PAN values shown unmasked (e.g., ABCDE1234F) |

### TC-06b: Create New Employee

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "Add Employee" button | Create Employee form displayed with optional "Login Account" section at bottom |
| 2 | Fill in: First Name: "Vikram", Last Name: "Mehta", Email: "vikram.mehta@nanotechnologies.com" | |
| 3 | Set Department: "Finance", City: "Mumbai", Gender: "Male", Date of Joining: today | |
| 4 | Set Manager dropdown to "Ramesh Kumar (Engineering)" | |
| 5 | Set PAN: "ZZZZZ9999Z" | |
| 6 | Click "Create" (without checking "Create login account") | Redirected to Employees list. Vikram Mehta visible. No login account created. |
| 7 | Try creating employee with empty First Name | Validation error: First Name is required |

### TC-06d: Create Employee with Login Account

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "Add Employee" button | Create Employee form displayed |
| 2 | Fill in: First Name: "Neha", Last Name: "Kapoor", Email: "neha.kapoor@nanotechnologies.com", Department: "Marketing" | |
| 3 | Check "Create login account for this employee" checkbox | Login fields appear: Role dropdown, Password field |
| 4 | Select Role: "Employee", Enter Password: "Neha@12345" | |
| 5 | Click "Create" | Success message: "Employee and login account created successfully for neha.kapoor@nanotechnologies.com." |
| 6 | Verify Neha appears in Employees list | Neha Kapoor visible with Department: Marketing |
| 7 | Logout. Login as neha.kapoor@nanotechnologies.com / Neha@12345 | Redirected to Employee Dashboard (login works) |
| 8 | HR: Create another employee with "Create login account" checked, Role: "Manager", Password: "Test@12345" | Employee created with Manager role login |
| 9 | Login as Admin, verify the new users in User Management | New users visible with correct roles |

### TC-06c: Edit Employee

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "Edit" next to Vikram Mehta | Edit form pre-filled with Vikram's data |
| 2 | Change City from "Mumbai" to "Delhi" | |
| 3 | Change Department from "Finance" to "Accounting" | |
| 4 | Click "Save Changes" | Redirected to Employees list. Vikram shows City: Delhi, Department: Accounting |
| 5 | Click "Cancel" on edit form | Redirected to Employees list without changes |

---

## 9. TC-07: HR - Appraisal Cycle Management

**Objective:** Verify HR can view, create, edit, and close appraisal cycles.

### TC-07a: View Cycles

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Login as HR | HR Dashboard |
| 2 | Click "Appraisal Cycles" in sidebar | Cycles list displayed with columns: Name, Start Date, End Date, State, Actions |
| 3 | Verify seeded cycle | "2025 Annual Appraisal", Start: 2025-04-01, End: 2025-06-30, State: "Open" (green badge), Edit button |

### TC-07b: Create New Cycle

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "New Cycle" button | Create Cycle form displayed |
| 2 | Enter Name: "2026 Mid-Year Review", Start Date: 2026-01-01, End Date: 2026-03-31 | |
| 3 | Click "Create Cycle" | Redirected to Cycles list. New cycle visible with State: "Open" |
| 4 | Try creating with empty Name | Validation error displayed |

### TC-07c: Edit Cycle

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "Edit" button next to "2026 Mid-Year Review" | Edit Cycle form pre-filled with cycle data (Name, Start Date, End Date, Status dropdown) |
| 2 | Change Name to "2026 Mid-Year Performance Review" | |
| 3 | Change End Date to 2026-04-30 | |
| 4 | Click "Save Changes" | Redirected to Cycles list. Updated name and end date visible |
| 5 | Click "Edit" next to "2025 Annual Appraisal" | Edit form displayed |
| 6 | Change Status from "Open" to "Closed" | |
| 7 | Click "Save Changes" | Redirected to Cycles list. State shows "Closed" (grey badge) |
| 8 | Go to Assign Appraisals | "2025 Annual Appraisal" no longer listed in open cycles dropdown (only "2026 Mid-Year Performance Review" is open) |
| 9 | Go back to Cycles, Edit "2025 Annual Appraisal", change Status back to "Open" | State shows "Open" again |
| 10 | Click "Cancel" on edit form | Redirected to Cycles list without changes |

---

## 10. TC-08: HR - Assign Appraisals

**Objective:** Verify HR can assign employees to appraisal cycles.

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Login as HR | HR Dashboard |
| 2 | Click "Assign Appraisals" in sidebar | Page shows unassigned employees with "Assign to [Cycle]" buttons |
| 3 | Verify Priya Sharma and Anil Patel are listed | Both shown with Department and Manager columns |
| 4 | Click "Assign to 2025 Annual Appraisal" next to Priya Sharma | Page refreshes. Priya Sharma no longer in the unassigned list |
| 5 | Click "Assign to 2025 Annual Appraisal" next to Anil Patel | Page refreshes. Anil Patel no longer in the list |
| 6 | Verify the page shows "All employees have been assigned" message | Success alert displayed |
| 7 | Navigate to Reports > Upcoming | Two appraisals listed: Priya Sharma (Draft) and Anil Patel (Draft) |

---

## 11. TC-09: Manager - Review Employee Personal Info

**Objective:** Verify Manager can view employee details for assigned appraisals.

**Pre-condition:** TC-08 completed (appraisals assigned to Priya and Anil)

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Login as Manager: ramesh.kumar@nanotechnologies.com / Manager@123 | Manager Dashboard |
| 2 | Click "My Appraisals" in sidebar | List of appraisals: Priya Sharma (Draft), Anil Patel (Draft) |
| 3 | Click "View" next to Priya Sharma | Appraisal Details page showing: Employee Info (Name, Dept, Email, DOJ), Appraisal Info (Cycle, Status: Draft, Manager: Ramesh Kumar) |
| 4 | Verify all employee fields are read-only (no edit buttons) | Information displayed in card format, no form inputs |
| 5 | Click "Back" button | Returns to My Appraisals list |

---

## 12. TC-10: Manager - Enter Comments

**Objective:** Verify Manager can enter achievements, gaps, and suggestions for an employee's appraisal.

**Pre-condition:** Appraisal for Priya Sharma exists in "Draft" status

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Login as Manager | Manager Dashboard |
| 2 | Go to My Appraisals | Priya Sharma listed with Status: "Draft" |
| 3 | Click "Comments" next to Priya Sharma | Enter Comments form displayed with: Achievements (textarea), Gaps (textarea), Suggestions (textarea) |
| 4 | Enter Achievements: "Excellent delivery of Q1 product features. Led the team on 3 critical releases." | |
| 5 | Enter Gaps: "Needs improvement in documentation and knowledge sharing." | |
| 6 | Enter Suggestions: "Attend leadership training. Mentor junior developers." | |
| 7 | Click "Save Comments" | Redirected to My Appraisals. Priya's status changed from "Draft" to "ManagerCommented" (yellow badge) |
| 8 | Click "Comments" again for Priya | Form shows previously saved data (Achievements, Gaps, Suggestions pre-filled) |
| 9 | Edit Achievements to append "Outstanding peer reviews." | |
| 10 | Click "Save Comments" | Data updated. Status remains "ManagerCommented" |
| 11 | Try submitting with empty Achievements field | Validation error: Achievements is required |

---

## 13. TC-11: Employee - Submit Feedback

**Objective:** Verify Employee can view manager comments and submit feedback.

**Pre-condition:** TC-10 completed (Manager entered comments for Priya, status = ManagerCommented)

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Login as Priya: priya.sharma@nanotechnologies.com / Employee@123 | Employee Dashboard |
| 2 | Click "My Appraisals" in sidebar | One appraisal listed: "2025 Annual Appraisal", Manager: Ramesh Kumar, Status: "ManagerCommented" |
| 3 | Click "View" | Appraisal detail page showing Manager's Comments section (Achievements, Gaps, Suggestions as read-only) |
| 4 | Click "Back", then click "Submit Feedback" button | Submit Feedback form displayed |
| 5 | Verify Manager's Comments displayed at top for reference | Card showing: Achievements, Gaps, Suggestions from manager |
| 6 | Enter Feedback: "I agree with the assessment. I have taken the initiative to mentor 2 junior developers this quarter." | |
| 7 | Enter Self Assessment: "I rate my performance at 4 out of 5. I have consistently met deadlines and improved code quality." | |
| 8 | Click "Submit Feedback" | Redirected to My Appraisals. Status changed to "EmployeeFeedback" |
| 9 | Click "View" to verify | Appraisal detail shows both Manager Comments and My Feedback sections |
| 10 | Try submitting with empty Feedback Text | Validation error: FeedbackText is required |

---

## 14. TC-12: Manager - Finalize Appraisal with CTC

**Objective:** Verify Manager can finalize appraisal with compensation details.

**Pre-condition:** TC-11 completed (Priya's status = EmployeeFeedback)

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Login as Manager | Manager Dashboard |
| 2 | Go to My Appraisals | Priya listed with "EmployeeFeedback" status. "Finalize" button visible |
| 3 | Click "Finalize" next to Priya | Finalize form showing: Employee name/dept, CTC fields (Basic, DA, HRA, Food Allowance, PF), IsPromoted checkbox, Next Appraisal Date |
| 4 | Enter Basic: 60000, DA: 12000, HRA: 24000, FoodAllowance: 6000, PF: 7200 | |
| 5 | Check "Employee is promoted" checkbox | |
| 6 | Set Next Appraisal Date: 2026-04-01 | |
| 7 | Click "Finalize Appraisal" | Redirected to My Appraisals. Priya's status: "Final" (green badge) |
| 8 | Click "View" next to Priya | Detail page shows all sections: Employee Info, Appraisal Info (Status: Final, Finalised date set), Manager Comments (with "Locked" badge), Employee Feedback, CTC Details (Promoted: Yes, Basic: 60,000.00, DA: 12,000.00, HRA: 24,000.00, Food Allowance: 6,000.00, PF: 7,200.00, **Total CTC: 109,200.00**) |
| 9 | Verify "Comments" button is no longer visible for Priya | Only "View" button shown (no edit options for finalized appraisal) |

---

## 15. TC-13: Comment Immutability Post-Finalization

**Objective:** Verify comments cannot be modified after appraisal is finalized.

**Pre-condition:** TC-12 completed (Priya's appraisal finalized)

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Login as Manager | Manager Dashboard |
| 2 | Go to My Appraisals | Priya listed with Status: "Final" |
| 3 | Click "View" for Priya | Manager Comments section shows "Locked" badge (red) |
| 4 | Verify no "Comments" edit button is available on My Appraisals list | Only "View" button visible (no "Comments" or "Finalize" buttons for Final status) |
| 5 | **API Test:** Send POST to `http://localhost:5100/api/appraisals/comment` with Priya's appraisalId | Response: 500 error with message "Comments are locked after finalization" |
| 6 | **API Test:** Send POST to `http://localhost:5100/api/appraisals/finalize` with Priya's appraisalId | Response: 500 error with message "Appraisal already finalized" |

---

## 16. TC-14: CTC Auto-Calculation

**Objective:** Verify Total CTC is calculated server-side as sum of all components.

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Complete TC-08 to assign Anil Patel to appraisal cycle | Anil's appraisal created in Draft |
| 2 | Login as Manager, enter comments for Anil | Status: ManagerCommented |
| 3 | Login as Anil, submit feedback | Status: EmployeeFeedback |
| 4 | Login as Manager, click "Finalize" for Anil | Finalize form displayed |
| 5 | Enter: Basic: 45000, DA: 9000, HRA: 18000, FoodAllowance: 4500, PF: 5400 | |
| 6 | Click "Finalize Appraisal" | Success |
| 7 | Click "View" for Anil | CTC Details section shows: Total CTC = 81,900.00 (45000 + 9000 + 18000 + 4500 + 5400) |
| 8 | **API Test:** GET `http://localhost:5100/api/appraisals/{anilAppraisalId}` | Response JSON: `"totalCTC": 81900` (calculated server-side) |
| 9 | Verify TotalCTC is not a user input field on the form | TotalCTC is computed property, not an editable field |

---

## 17. TC-15: PAN Masking by Role

**Objective:** Verify PAN (Permanent Account Number) is masked for non-HR/Admin roles.

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Login as **HR** (sunita.rao@nanotechnologies.com) | HR Dashboard |
| 2 | Go to Employees | PAN column shows **full** values: ABCDE1234F, FGHIJ5678K, KLMNO9012P, QRSTU3456V |
| 3 | Logout | |
| 4 | Login as **Admin** (admin@nanotechnologies.com) | Admin Dashboard |
| 5 | **API Test:** GET `http://localhost:5100/api/employees` with Admin auth cookie | PAN values **unmasked** (ABCDE1234F etc.) |
| 6 | Logout | |
| 7 | Login as **Manager** (ramesh.kumar@nanotechnologies.com) | Manager Dashboard |
| 8 | **API Test:** GET `http://localhost:5100/api/employees` with Manager auth cookie | PAN values **masked**: `******234F`, `******678K`, `******012P`, `******456V` |
| 9 | Logout | |
| 10 | Login as **Employee** (priya.sharma@nanotechnologies.com) | Employee Dashboard |
| 11 | Go to My Profile | PAN field shows **masked** value (e.g., `******678K`) |
| 12 | **API Test:** GET `http://localhost:5100/api/employees/2` with Employee auth cookie | `"panNo": "******678K"` (masked) |

---

## 18. TC-16: Employee - Profile Self-Edit Restrictions

**Objective:** Verify employees can edit personal fields but NOT Department or Manager.

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Login as Employee: priya.sharma@nanotechnologies.com / Employee@123 | Employee Dashboard |
| 2 | Click "My Profile" in sidebar | Profile page shows all fields including Department: Engineering, Manager: Ramesh Kumar |
| 3 | Click "Edit Profile" | Edit form displayed |
| 4 | Verify message: "You can update personal information. Department and Manager are managed by HR." | Informational text visible |
| 5 | Verify **no** Department or Manager fields on the form | Only personal fields: First Name, Last Name, Mobile, Phone, Address, City, Gender, Marital Status, DOB, PAN, Passport |
| 6 | Change City from "Pune" to "Hyderabad" | |
| 7 | Change Mobile to "9876500000" | |
| 8 | Click "Save Changes" | Redirected to My Profile. City: Hyderabad, Mobile: 9876500000 |
| 9 | Verify Department still shows "Engineering" | Unchanged (not editable by employee) |
| 10 | Verify Manager still shows "Ramesh Kumar" | Unchanged (not editable by employee) |

---

## 19. TC-17: Reports - Upcoming, In-Process, Completed

**Objective:** Verify reports filter correctly by status and role.

**Pre-condition:** At least one appraisal in each status (Draft, ManagerCommented/EmployeeFeedback, Final)

### TC-17a: HR Reports (sees all)

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Login as HR | HR Dashboard |
| 2 | Click "Reports" in sidebar | Reports page with 3 cards: Upcoming, In Process, Completed |
| 3 | Click "Upcoming" | Lists all appraisals with Status: Draft |
| 4 | Click "Back to Reports" > "In Process" | Lists appraisals with Status: ManagerCommented or EmployeeFeedback |
| 5 | Click "Back to Reports" > "Completed" | Lists appraisals with Status: Final |
| 6 | Verify table columns | Employee, Department, Manager, Cycle, Status (badge), Created, Finalised |

### TC-17b: Manager Reports (filtered)

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Login as Manager | Manager Dashboard |
| 2 | Click Reports > Upcoming | Shows only appraisals where Manager is Ramesh Kumar, Status: Draft |
| 3 | Click Reports > In Process | Shows only appraisals where Manager is Ramesh Kumar, Status: ManagerCommented/EmployeeFeedback |
| 4 | Click Reports > Completed | Shows only appraisals where Manager is Ramesh Kumar, Status: Final |

### TC-17c: Employee Reports (filtered)

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Login as Employee (Priya) | Employee Dashboard |
| 2 | Click Reports > Upcoming | Shows only Priya's appraisals with Status: Draft (empty if none) |
| 3 | Click Reports > Completed | Shows only Priya's appraisals with Status: Final |

---

## 20. TC-18: Audit Trail Verification

**Objective:** Verify all key actions are recorded in the audit log.

| Step | Action | Expected Result (verify via DB) |
|------|--------|------|
| 1 | Perform a successful login | AuditEvents table has entry: Action="LoginSuccess", EntityType="User", ActorEmail=(login email) |
| 2 | Perform a failed login | AuditEvents: Action="LoginFailed", ActorEmail=(attempted email) |
| 3 | Lock account (3 failed attempts) | AuditEvents: 3 "LoginFailed" entries for the same email |
| 4 | Admin unlocks account | AuditEvents: Action="AccountUnlocked", ActorEmail=(admin email), DetailsJson contains userId |
| 5 | Admin locks account manually | AuditEvents: Action="AccountLocked" |
| 6 | HR assigns appraisal | AuditEvents: Action="Assigned", EntityType="Appraisal", EntityId=(appraisalId) |
| 7 | Manager saves comments | AuditEvents: Action="Saved", EntityType="Comment" |
| 8 | Employee submits feedback | AuditEvents: Action="Saved", EntityType="Feedback" |
| 9 | Manager finalizes appraisal | AuditEvents: Action="Finalized", EntityType="Appraisal" |
| 10 | User logs out | AuditEvents: Action="Logout" |

**DB Query to verify:**
```sql
SELECT Id, EntityType, EntityId, Action, ActorEmail, At, IpAddress, DetailsJson
FROM AuditEvents ORDER BY At DESC;
```

---

## 21. TC-19: Notification Queue Verification

**Objective:** Verify notifications are queued for key events.

| Step | Action | Expected Result (verify via DB) |
|------|--------|------|
| 1 | HR assigns appraisal to employee | Notifications table: Topic="AppraisalAssigned", RecipientEmail=(employee email), Status="Queued" |
| 2 | Manager finalizes appraisal | Notifications table: Topic="AppraisalFinalized", RecipientEmail=(employee email), Status="Queued" |

**DB Query to verify:**
```sql
SELECT Id, RecipientEmail, Topic, Status, AboutAppraisalId, CreatedAt
FROM Notifications ORDER BY CreatedAt DESC;
```

---

## 22. TC-20: Full End-to-End Appraisal Workflow

**Objective:** Execute the complete appraisal lifecycle from assignment to finalization in a single flow.

| Step | Actor | Action | Expected Result |
|------|-------|--------|-----------------|
| 1 | HR | Login as sunita.rao@nanotechnologies.com / Hr@12345 | HR Dashboard |
| 2 | HR | Go to Employees > Add Employee | Create Employee form |
| 3 | HR | Create: Meera Joshi, meera.joshi@nanotechnologies.com, Dept: Development, Manager: Ramesh Kumar | Employee created |
| 4 | HR | Go to Assign Appraisals | Meera Joshi listed as unassigned |
| 5 | HR | Click "Assign to 2025 Annual Appraisal" for Meera | Meera assigned, removed from unassigned list |
| 6 | HR | Go to Reports > Upcoming | Meera's appraisal listed with Status: Draft |
| 7 | HR | Logout | |
| 8 | Manager | Login as ramesh.kumar@nanotechnologies.com / Manager@123 | Manager Dashboard |
| 9 | Manager | Go to My Appraisals | Meera Joshi listed with Status: Draft |
| 10 | Manager | Click "View" next to Meera | Employee info displayed (read-only) |
| 11 | Manager | Click "Back", then "Comments" for Meera | Enter Comments form |
| 12 | Manager | Enter Achievements: "Strong technical contributor. Delivered microservices migration on time." | |
| 13 | Manager | Enter Gaps: "Communication with cross-functional teams needs improvement." | |
| 14 | Manager | Enter Suggestions: "Present in monthly all-hands. Take a public speaking course." | |
| 15 | Manager | Click "Save Comments" | Status changes to "ManagerCommented" |
| 16 | Manager | Go to Reports > In Process | Meera listed |
| 17 | Manager | Logout | |
| 18 | HR | Go to Employees > Add Employee > Create Meera's employee with "Create login account" checked, Role: Employee, Password: "Meera@123" | Employee and login account created |
| 18b | *(Alternative)* Admin | Login as Admin, Create User: meera.joshi@nanotechnologies.com, Role: Employee, Password: "Meera@123" | User created |
| 19 | Employee | Login as meera.joshi@nanotechnologies.com / Meera@123 | Employee Dashboard |
| 20 | Employee | Go to My Appraisals | One appraisal listed: Status "ManagerCommented", "Submit Feedback" button visible |
| 21 | Employee | Click "Submit Feedback" | Feedback form with Manager's comments shown for reference |
| 22 | Employee | Enter Feedback: "I appreciate the recognition. I am committed to improving cross-team communication." | |
| 23 | Employee | Enter Self Assessment: "I believe my performance has been excellent this year, 4.5/5." | |
| 24 | Employee | Click "Submit Feedback" | Status changes to "EmployeeFeedback" |
| 25 | Employee | Click "View" to verify | Both Manager Comments and My Feedback sections visible |
| 26 | Employee | Logout | |
| 27 | Manager | Login as Manager | Manager Dashboard |
| 28 | Manager | Go to My Appraisals | Meera: Status "EmployeeFeedback", "Finalize" button visible |
| 29 | Manager | Click "Finalize" | CTC form displayed |
| 30 | Manager | Enter: Basic: 55000, DA: 11000, HRA: 22000, FoodAllowance: 5500, PF: 6600. Check "Promoted". Next Appraisal: 2026-04-01 | |
| 31 | Manager | Click "Finalize Appraisal" | Status: "Final" |
| 32 | Manager | Click "View" for Meera | Complete detail: Comments (Locked badge), Feedback, CTC (Total: 100,100.00), Promoted: Yes, Next Appraisal: 2026-04-01 |
| 33 | Manager | Go to Reports > Completed | Meera listed |
| 34 | Manager | Logout | |
| 35 | Employee | Login as Meera | Employee Dashboard |
| 36 | Employee | Go to My Appraisals | Status: "Final" (green badge) |
| 37 | Employee | Click "View" | CTC Details shown: Total CTC: 100,100.00, Promoted: Yes |
| 38 | Employee | Go to Reports > Completed | Own appraisal listed |

---

## 23. TC-21: Negative and Edge Case Scenarios

### TC-21a: Unauthorized Access

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Without logging in, navigate to http://localhost:5200/HR/Employees | Redirected to Login page |
| 2 | Without logging in, navigate to http://localhost:5200/Manager/MyAppraisals | Redirected to Login page |
| 3 | Without logging in, navigate to http://localhost:5200/Admin/Users | Redirected to Login page |
| 4 | Without logging in, navigate to http://localhost:5200/Reports/Upcoming | Redirected to Login page |

### TC-21b: API Without Authentication

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | GET http://localhost:5100/api/employees (no auth cookie) | HTTP 401 Unauthorized |
| 2 | GET http://localhost:5100/api/appraisals/1 (no auth cookie) | HTTP 401 Unauthorized |
| 3 | POST http://localhost:5100/api/auth/login (valid credentials) | HTTP 200 with success response |
| 4 | GET http://localhost:5100/api/auth/me (no auth cookie) | HTTP 401 Unauthorized |

### TC-21c: Invalid Data Scenarios

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | HR: Create employee without required First Name | Validation error on form |
| 2 | Manager: Enter comment with Achievements > 2000 characters | Validation error: max 2000 chars |
| 3 | Manager: Finalize with negative Basic salary | Validation error: Range(0, max) |
| 4 | Admin: Create user with password "123" (too short, no uppercase/special) | Error: password requirements not met |
| 5 | Admin: Create user with non-email format in email field | Validation error: invalid email |
| 6 | Navigate to http://localhost:5200/Account/Register | Page not found or redirected (self-registration removed) |

### TC-21d: Workflow Constraint Violations

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Employee tries to submit feedback on Draft appraisal (before manager comments) | Status remains Draft (no transition to EmployeeFeedback) |
| 2 | Manager tries to finalize same appraisal twice | Error: "Appraisal already finalized" |
| 3 | Manager tries to edit comments on finalized appraisal via API | Error: "Comments are locked after finalization" |
| 4 | Employee tries to modify feedback on finalized appraisal via API | Error: "Cannot modify feedback on finalized appraisal" |
| 5 | HR tries to assign employee with no manager set | Error: "Employee has no manager assigned" |

### TC-21e: Concurrent/Boundary Scenarios

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Two browser windows: Login as HR in one, Manager in other | Both sessions work independently |
| 2 | HR assigns same employee twice to same cycle | Second assignment creates duplicate (or application handles gracefully) |
| 3 | Root URL (http://localhost:5200/) | Redirected to Login page |
| 4 | Non-existent route (http://localhost:5200/xyz) | 404 or redirect to Login |

---

## Appendix A: Appraisal Workflow State Diagram

```
   [None]
     |
     | HR Assigns Appraisal
     v
   [Draft] ─────────────────────────┐
     |                              |
     | Manager Enters Comments      | Manager can also
     v                              | Finalize directly
   [ManagerCommented]               |
     |                              |
     | Employee Submits Feedback    |
     v                              |
   [EmployeeFeedback]              |
     |                              |
     | Manager Finalizes            |
     v                              |
   [Final] <────────────────────────┘
     |
     |── Comments.IsLocked = true
     |── CtcSnapshot.ApprovedAt set
     |── Appraisal.FinalisedAt set
     |── Notification queued
```

## Appendix B: API Quick Reference

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | /api/auth/login | No | User login |
| POST | /api/auth/register | Yes | Create user (Admin/HR) |
| POST | /api/auth/logout | Yes | User logout |
| GET | /api/auth/me | Yes | Current user info |
| GET | /api/employees | Yes | List all employees |
| GET | /api/employees/{id} | Yes | Get employee by ID |
| POST | /api/employees | Yes | Create employee |
| PUT | /api/employees/{id} | Yes | Update employee |
| PUT | /api/employees/{id}/profile | Yes | Self-edit profile |
| GET | /api/appraisals/by-manager/{id} | Yes | Appraisals for manager |
| GET | /api/appraisals/{id} | Yes | Appraisal detail |
| POST | /api/appraisals/assign | Yes | Assign appraisal |
| POST | /api/appraisals/comment | Yes | Save manager comment |
| POST | /api/appraisals/feedback | Yes | Save employee feedback |
| POST | /api/appraisals/finalize | Yes | Finalize appraisal |
| GET | /api/appraisals/by-status | Yes | Filter by status/role |
| GET | /api/appraisals/by-employee/{id} | Yes | Appraisals for employee |
| GET | /api/cycles | Yes | List all cycles |
| GET | /api/cycles/{id} | Yes | Get cycle by ID |
| GET | /api/cycles/open | Yes | List open cycles |
| POST | /api/cycles | Yes | Create cycle |
| PUT | /api/cycles/{id} | Yes | Update cycle (name, dates, state) |
| POST | /api/cycles/unassigned-employees | Yes | Unassigned employees |
| GET | /api/admin/users | Yes | List all users |
| POST | /api/admin/users/{id}/unlock | Yes | Unlock user |
| POST | /api/admin/users/{id}/lock | Yes | Lock user |
