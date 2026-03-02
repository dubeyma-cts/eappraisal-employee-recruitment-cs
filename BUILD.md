# e-Appraisal System – Build & Run Guide

## Prerequisites
- .NET 8 SDK or later
- No additional databases required (SQLite is embedded)

## Project Location
```
src/Web/eAppraisal/
```

## Quick Start

```bash
cd src/Web/eAppraisal
dotnet run
```

Open your browser at: **http://localhost:5000**

## Demo Credentials

| Role     | Email                                      | Password      |
|----------|--------------------------------------------|---------------|
| HR       | sunita.rao@nanotechnologies.com             | Hr@12345      |
| Manager  | ramesh.kumar@nanotechnologies.com           | Manager@123   |
| Employee | priya.sharma@nanotechnologies.com           | Employee@123  |
| Employee | anil.patel@nanotechnologies.com             | Employee@123  |
| Admin    | admin@nanotechnologies.com                  | Admin@123     |

## Features Implemented

### Authentication (UC-01)
- Role-based login (HR, Manager, Employee, Admin)
- 3-strike account lockout
- Custom session-based navigation

### HR Section (UC-02, UC-03)
- Full Employee Master CRUD (Name, Address, City, Phone, Mobile, Email, DOB, Gender, Marital Status, DOJ, Passport, PAN, Experience, Department, Reports To)
- Appraisal Cycle management (Open/Frozen/Closed)
- Assign appraisals to employees in open cycles

### Manager Section (UC-04, UC-05, UC-07)
- View all assigned appraisals
- Read-only view of employee personal info
- Enter comments: Achievements, Gaps, Suggestions
- Finalize appraisal with Promotion decision + CTC (Basic, DA, HRA, Food Allowance, PF, Next Appraisal Date)

### Employee Section (UC-06)
- View/Edit own profile (except Department and Reports To)
- View manager comments (read-only)
- Submit feedback and self-assessment

### Admin Section (UC-09)
- View all users with lock status
- Unlock/Lock accounts

### Reports (UC-08)
- Upcoming Appraisals (Draft status)
- In-Process Appraisals (ManagerCommented/EmployeeFeedback)
- Completed Appraisals (Final with CTC details)
- All reports are role-filtered

## Database
- SQLite database file: `src/Web/eAppraisal/eappraisal.db`
- Auto-created on first run via EF Core migrations
- Seed data auto-loaded on first run

## Appraisal Workflow
```
HR assigns → Draft
Manager enters comments → ManagerCommented
Employee submits feedback → EmployeeFeedback
Manager finalizes + CTC → Final
```

## Technology Stack
- ASP.NET Core 8 MVC
- Entity Framework Core 8 with SQLite
- ASP.NET Core Identity
- Bootstrap 5 (responsive UI)
- Razor Views

## Build from Scratch
```bash
cd src/Web/eAppraisal
dotnet restore
dotnet build
dotnet ef migrations add InitialCreate  # only if Migrations/ folder missing
dotnet ef database update
dotnet run
```
