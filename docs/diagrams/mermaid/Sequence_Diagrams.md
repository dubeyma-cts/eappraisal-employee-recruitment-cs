# eAppraisal System - Sequence Diagrams

---

## 1. User Login Flow

```mermaid
sequenceDiagram
    actor User as User (Browser)
    participant Web as eAppraisal.Web<br/>(Port 5200)
    participant ApiClient as ApiClient<br/>(HTTP Client)
    participant API as eAppraisal.Api<br/>(Port 5100)
    participant AuthSvc as AuthService
    participant UserStore as IdentityUserStore<br/>(ASP.NET Identity)
    participant AuditSvc as AuditLogCollector
    participant DB as SQLite Database

    User->>Web: GET /Account/Login
    Web-->>User: Render Login Page

    User->>Web: POST /Account/Login<br/>{email, password}
    Web->>ApiClient: LoginAsync("api/auth/login", LoginDto)
    ApiClient->>API: POST /api/auth/login<br/>(JSON Body)

    API->>AuthSvc: LoginAsync(dto, ipAddress)
    AuthSvc->>UserStore: AuthenticateAsync(email, password, ip)
    UserStore->>DB: FindByEmailAsync(email)

    alt User Not Found
        DB-->>UserStore: null
        UserStore-->>AuthSvc: {Success: false, Error: "Invalid email or password"}
        AuthSvc->>AuditSvc: LogAsync("LoginFailed")
        AuditSvc->>DB: INSERT AuditEvent
        AuthSvc-->>API: AuthResultDto {Success: false}
        API-->>ApiClient: 401 Unauthorized (JSON)
        ApiClient-->>Web: AuthResultDto {Success: false}
        Web-->>User: Render Login Page + Error Message
    end

    alt Account Locked (3 failed attempts or manual lock)
        DB-->>UserStore: user (IsManuallyLocked=true OR FailedAttempts >= 3)
        UserStore-->>AuthSvc: {Success: false, Error: "Account is locked"}
        AuthSvc->>AuditSvc: LogAsync("LoginFailed")
        AuditSvc->>DB: INSERT AuditEvent
        AuthSvc-->>API: AuthResultDto {Success: false}
        API-->>ApiClient: 401 Unauthorized
        ApiClient-->>Web: AuthResultDto
        Web-->>User: Render Login Page + "Account Locked" Error
    end

    alt Wrong Password
        DB-->>UserStore: user found
        UserStore->>DB: CheckPasswordSignInAsync(user, password)
        DB-->>UserStore: Failed
        UserStore->>DB: UPDATE FailedLoginAttempts++
        UserStore-->>AuthSvc: {Success: false, Error: "Invalid email or password"}
        AuthSvc->>AuditSvc: LogAsync("LoginFailed")
        AuditSvc->>DB: INSERT AuditEvent
        AuthSvc-->>API: AuthResultDto {Success: false}
        API-->>ApiClient: 401 Unauthorized
        ApiClient-->>Web: AuthResultDto
        Web-->>User: Render Login Page + Error Message
    end

    rect rgb(200, 255, 200)
        Note over User,DB: Successful Login
        DB-->>UserStore: user found
        UserStore->>DB: CheckPasswordSignInAsync(user, password)
        DB-->>UserStore: Succeeded
        UserStore->>DB: UPDATE FailedLoginAttempts = 0
        UserStore->>DB: SignInAsync(user) - Sets Auth Cookie
        UserStore-->>AuthSvc: {Success: true, Role, FullName, EmployeeId}
        AuthSvc->>AuditSvc: LogAsync("LoginSuccess")
        AuditSvc->>DB: INSERT AuditEvent
        AuthSvc-->>API: AuthResultDto {Success: true}
        API-->>ApiClient: 200 OK + Set-Cookie: eAppraisal.Auth
        ApiClient->>ApiClient: CaptureResponseCookies()<br/>Store cookie in Session
        ApiClient-->>Web: AuthResultDto {Success: true}
        Web->>Web: Store in Session:<br/>UserEmail, UserRole,<br/>UserFullName, EmployeeId
        Web-->>User: 302 Redirect to Role Dashboard<br/>(Admin/HR/Manager/Employee)
    end
```

---

## 2. Full Appraisal Workflow (Assign --> Comment --> Feedback --> Finalize)

```mermaid
sequenceDiagram
    actor HR as HR User
    actor Mgr as Manager
    actor Emp as Employee
    participant Web as eAppraisal.Web
    participant API as eAppraisal.Api
    participant WorkflowSvc as AppraisalWorkflow<br/>Service
    participant CommentsSvc as Comments<br/>Service
    participant CtcSvc as CtcService
    participant ReportingSvc as Reporting<br/>Service
    participant AuditSvc as AuditLog<br/>Collector
    participant NotifSvc as Notification<br/>Service
    participant DB as SQLite DB

    %% =============================================
    %% PHASE 1: HR ASSIGNS APPRAISAL
    %% =============================================
    rect rgb(230, 240, 255)
        Note over HR,DB: PHASE 1 - HR Assigns Appraisal (Status: None --> Draft)

        HR->>Web: GET /HR/UpcomingAppraisals
        Web->>API: GET /api/cycles/open
        API-->>Web: Open Cycles
        Web->>API: POST /api/cycles/unassigned-employees
        API-->>Web: Employees without appraisals
        Web-->>HR: Show unassigned employees list

        HR->>Web: POST /HR/AssignAppraisal<br/>{EmployeeId, CycleId}
        Web->>API: POST /api/appraisals/assign
        API->>WorkflowSvc: AssignAsync(empId, cycleId, assignedBy)

        WorkflowSvc->>DB: Find Employee (verify has Manager)
        DB-->>WorkflowSvc: Employee record
        WorkflowSvc->>DB: INSERT Appraisal (Status="Draft")
        WorkflowSvc->>DB: INSERT StageHistory<br/>(None --> Draft)
        WorkflowSvc->>AuditSvc: LogAsync("Appraisal", "Assigned")
        AuditSvc->>DB: INSERT AuditEvent
        WorkflowSvc->>NotifSvc: QueueAsync(employee, "AppraisalAssigned")
        NotifSvc->>DB: INSERT Notification
        WorkflowSvc-->>API: AppraisalDto
        API-->>Web: 200 OK
        Web-->>HR: "Appraisal assigned successfully"
    end

    %% =============================================
    %% PHASE 2: MANAGER ENTERS COMMENTS
    %% =============================================
    rect rgb(255, 245, 230)
        Note over Mgr,DB: PHASE 2 - Manager Enters Comments (Status: Draft --> ManagerCommented)

        Mgr->>Web: GET /Manager/MyAppraisals
        Web->>API: GET /api/appraisals/by-manager/{empId}
        API->>ReportingSvc: GetByManagerAsync(empId)
        ReportingSvc->>DB: SELECT Appraisals WHERE ManagerEmployeeId=empId
        DB-->>ReportingSvc: List of appraisals
        ReportingSvc-->>API: List of AppraisalDto
        API-->>Web: Appraisals list
        Web-->>Mgr: Show appraisals table (Status: Draft)

        Mgr->>Web: GET /Manager/EnterComments/{id}
        Web->>API: GET /api/appraisals/{id}
        API->>ReportingSvc: GetDetailAsync(id)
        ReportingSvc->>DB: SELECT Appraisal + Employee + Comment + Feedback + CTC
        ReportingSvc-->>API: AppraisalDetailDto
        API-->>Web: Appraisal details
        Web-->>Mgr: Show comments form

        Mgr->>Web: POST /Manager/EnterComments<br/>{AppraisalId, Achievements, Gaps, Suggestions}
        Web->>API: POST /api/appraisals/comment
        API->>CommentsSvc: SaveCommentAsync(dto, changedBy)

        CommentsSvc->>DB: Find Appraisal + ManagerComment
        Note over CommentsSvc: Check: IsLocked != true
        CommentsSvc->>DB: INSERT/UPDATE Comment
        CommentsSvc->>DB: UPDATE Appraisal Status="ManagerCommented"
        CommentsSvc->>DB: INSERT StageHistory<br/>(Draft --> ManagerCommented)
        CommentsSvc->>AuditSvc: LogAsync("Comment", "Saved")
        AuditSvc->>DB: INSERT AuditEvent
        CommentsSvc-->>API: OK
        API-->>Web: 200 OK
        Web-->>Mgr: Redirect to MyAppraisals
    end

    %% =============================================
    %% PHASE 3: EMPLOYEE SUBMITS FEEDBACK
    %% =============================================
    rect rgb(230, 255, 230)
        Note over Emp,DB: PHASE 3 - Employee Submits Feedback (Status: ManagerCommented --> EmployeeFeedback)

        Emp->>Web: GET /Employee/MyAppraisal
        Web->>API: GET /api/appraisals/by-employee/{empId}
        API->>ReportingSvc: GetByEmployeeAsync(empId)
        ReportingSvc->>DB: SELECT Appraisals WHERE EmployeeId=empId
        ReportingSvc-->>API: Appraisals list
        API-->>Web: Appraisals list
        Web-->>Emp: Show appraisals (Status: ManagerCommented)

        Emp->>Web: GET /Employee/SubmitFeedback/{id}
        Web->>API: GET /api/appraisals/{id}
        API->>ReportingSvc: GetDetailAsync(id)
        ReportingSvc-->>API: Detail with manager comments visible
        API-->>Web: AppraisalDetailDto
        Web-->>Emp: Show feedback form<br/>(Manager comments displayed read-only)

        Emp->>Web: POST /Employee/SubmitFeedback<br/>{AppraisalId, FeedbackText, SelfAssessment}
        Web->>API: POST /api/appraisals/feedback
        API->>CommentsSvc: SaveFeedbackAsync(dto, changedBy)

        CommentsSvc->>DB: Find Appraisal + EmployeeFeedback
        Note over CommentsSvc: Check: Status != "Final"
        CommentsSvc->>DB: INSERT/UPDATE EmployeeFeedback
        CommentsSvc->>DB: UPDATE Appraisal Status="EmployeeFeedback"
        CommentsSvc->>DB: INSERT StageHistory<br/>(ManagerCommented --> EmployeeFeedback)
        CommentsSvc->>AuditSvc: LogAsync("Feedback", "Saved")
        AuditSvc->>DB: INSERT AuditEvent
        CommentsSvc-->>API: OK
        API-->>Web: 200 OK
        Web-->>Emp: Redirect to MyAppraisal
    end

    %% =============================================
    %% PHASE 4: MANAGER FINALIZES WITH CTC
    %% =============================================
    rect rgb(255, 230, 230)
        Note over Mgr,DB: PHASE 4 - Manager Finalizes Appraisal (Status: EmployeeFeedback --> Final)

        Mgr->>Web: GET /Manager/Finalize/{id}
        Web->>API: GET /api/appraisals/{id}
        API->>ReportingSvc: GetDetailAsync(id)
        ReportingSvc-->>API: Full detail with feedback
        API-->>Web: AppraisalDetailDto
        Web-->>Mgr: Show finalization form<br/>(CTC fields: Basic, DA, HRA, Food, PF)

        Mgr->>Web: POST /Manager/Finalize<br/>{AppraisalId, IsPromoted, Basic, DA,<br/>HRA, FoodAllowance, PF, NextAppraisalDate}
        Web->>API: POST /api/appraisals/finalize
        API->>WorkflowSvc: FinalizeAsync(dto, changedBy)

        WorkflowSvc->>DB: Find Appraisal
        Note over WorkflowSvc: Check: Status != "Final"

        WorkflowSvc->>CtcSvc: CreateOrUpdateSnapshotAsync(id, dto)
        CtcSvc->>DB: INSERT/UPDATE CtcSnapshot<br/>(Basic, DA, HRA, Food, PF)<br/>TotalCTC = Sum of all components
        CtcSvc-->>WorkflowSvc: Done

        WorkflowSvc->>CommentsSvc: LockCommentsAsync(id)
        CommentsSvc->>DB: UPDATE Comment SET IsLocked=true
        CommentsSvc-->>WorkflowSvc: Done

        WorkflowSvc->>DB: UPDATE Appraisal<br/>Status="Final", FinalisedAt=now
        WorkflowSvc->>DB: INSERT StageHistory<br/>(EmployeeFeedback --> Final)
        WorkflowSvc->>AuditSvc: LogAsync("Appraisal", "Finalized")
        AuditSvc->>DB: INSERT AuditEvent
        WorkflowSvc->>NotifSvc: QueueAsync(employee, "AppraisalFinalized")
        NotifSvc->>DB: INSERT Notification
        WorkflowSvc-->>API: OK
        API-->>Web: 200 OK
        Web-->>Mgr: Redirect to MyAppraisals<br/>(Status now shows "Final")
    end
```

---

## 3. Admin Creates a New User Account

```mermaid
sequenceDiagram
    actor Admin as Admin User
    participant Web as eAppraisal.Web
    participant API as eAppraisal.Api
    participant AuthSvc as AuthService
    participant UserStore as IdentityUserStore
    participant AuditSvc as AuditLogCollector
    participant DB as SQLite DB

    Admin->>Web: GET /Admin/CreateUser
    Web-->>Admin: Render Create User form<br/>(FullName, Email, Role, Password)

    Admin->>Web: POST /Admin/CreateUser<br/>{FullName, Email, Role, Password}
    Web->>API: POST /api/auth/register<br/>(RegisterRequest JSON)
    API->>API: Validate ModelState

    API->>AuthSvc: RegisterUserAsync(email, fullName, password, role)
    AuthSvc->>UserStore: CreateUserAsync(email, fullName, password, role)

    UserStore->>DB: FindByEmailAsync(email)
    alt Email Already Exists
        DB-->>UserStore: existing user found
        UserStore-->>AuthSvc: {Success: false, Error: "Email already exists"}
        AuthSvc-->>API: {Success: false, Error}
        API-->>Web: 400 BadRequest {message: error}
        Web-->>Admin: Render form + error message
    end

    rect rgb(200, 255, 200)
        Note over Admin,DB: Successful User Creation
        DB-->>UserStore: null (no existing user)

        UserStore->>DB: INSERT Employee<br/>(FirstName, LastName, Email,<br/>Department="Unassigned")
        DB-->>UserStore: Employee created (Id assigned)

        UserStore->>DB: INSERT AspNetUser<br/>(UserName=email, FullName, AppRole,<br/>EmployeeId, Password hash)

        alt Password Policy Violation
            DB-->>UserStore: CreateAsync failed
            UserStore->>DB: DELETE Employee (rollback)
            UserStore-->>AuthSvc: {Success: false, Error: "password errors"}
            AuthSvc-->>API: {Success: false, Error}
            API-->>Web: 400 BadRequest
            Web-->>Admin: Render form + password error
        end

        DB-->>UserStore: CreateAsync succeeded
        UserStore-->>AuthSvc: {Success: true, EmployeeId}

        AuthSvc->>AuditSvc: LogAsync("User", empId, "Registered")
        AuditSvc->>DB: INSERT AuditEvent
        AuthSvc-->>API: {Success: true}
        API-->>Web: 200 OK {message: "Registration successful"}
        Web-->>Admin: Redirect to Users list<br/>+ "User created successfully" message
    end
```

---

## 4. HR Creates Employee with Optional Login Account

```mermaid
sequenceDiagram
    actor HR as HR User
    participant Web as eAppraisal.Web
    participant API as eAppraisal.Api
    participant EmpSvc as EmployeeService
    participant AuthSvc as AuthService
    participant DB as SQLite DB

    HR->>Web: GET /HR/CreateEmployee
    Web-->>HR: Render employee form<br/>+ optional "Create Login Account" checkbox

    HR->>Web: POST /HR/CreateEmployee<br/>{EmployeeDto + createLogin + loginRole + loginPassword}
    Web->>API: POST /api/employees (EmployeeDto)
    API->>EmpSvc: CreateAsync(dto)
    EmpSvc->>DB: INSERT Employee
    DB-->>EmpSvc: Employee created
    EmpSvc-->>API: EmployeeDto (with Id)
    API-->>Web: 201 Created

    alt createLogin = true
        Note over Web,API: HR checked "Create Login Account"
        Web->>API: POST /api/auth/register<br/>{Email, FullName, Password, Role}
        API->>AuthSvc: RegisterUserAsync(...)
        AuthSvc->>DB: CreateUserAsync(...)

        alt Login Creation Succeeded
            DB-->>AuthSvc: User created
            AuthSvc-->>API: {Success: true}
            API-->>Web: 200 OK
            Web-->>HR: Redirect to Employees<br/>+ "Employee created with login account"
        end

        alt Login Creation Failed
            DB-->>AuthSvc: Error
            AuthSvc-->>API: {Success: false, Error}
            API-->>Web: 400 BadRequest
            Web-->>HR: Redirect to Employees<br/>+ "Employee created but login failed: {error}"
        end
    end

    alt createLogin = false
        Note over Web: No login account requested
        Web-->>HR: Redirect to Employees<br/>+ "Employee created successfully"
    end
```

---

## 5. PAN Masking Flow (Policy Engine)

```mermaid
sequenceDiagram
    actor User as Any Authenticated User
    participant Web as eAppraisal.Web
    participant API as eAppraisal.Api
    participant EmpCtrl as EmployeesController
    participant EmpSvc as EmployeeService
    participant Masking as PolicyMaskingEngine
    participant DB as SQLite DB

    User->>Web: View Employee List/Details
    Web->>API: GET /api/employees<br/>(Cookie includes AppRole claim)
    API->>EmpCtrl: GetAll()

    EmpCtrl->>EmpSvc: GetAllAsync()
    EmpSvc->>DB: SELECT * FROM Employees
    DB-->>EmpSvc: Employee list (PAN: "ABCDE1234F")
    EmpSvc-->>EmpCtrl: List of EmployeeDto

    EmpCtrl->>EmpCtrl: GetUserRole() from Claims

    loop For each employee
        EmpCtrl->>Masking: ApplyMasking(pan, role)

        alt role = "HR" or "Admin"
            Masking-->>EmpCtrl: "ABCDE1234F" (Full PAN visible)
        end

        alt role = "Manager" or "Employee"
            Masking-->>EmpCtrl: "XXXXXXX34F" (Masked - last 3 chars only)
        end
    end

    EmpCtrl-->>API: List with masked PAN
    API-->>Web: JSON response
    Web-->>User: Display employee table<br/>(PAN masked based on role)
```

---

## 6. Reports Flow (Role-Filtered Data)

```mermaid
sequenceDiagram
    actor User as HR / Manager / Employee
    participant Web as eAppraisal.Web<br/>(ReportsController)
    participant API as eAppraisal.Api
    participant ReportingSvc as ReportingService
    participant DB as SQLite DB

    User->>Web: GET /Reports/Completed

    Web->>Web: Read session:<br/>role, employeeId

    Web->>API: GET /api/appraisals/by-status<br/>?status=Final&role={role}&employeeId={empId}
    API->>ReportingSvc: GetByStatusAsync("Final", role, empId)

    ReportingSvc->>DB: SELECT Appraisals WHERE Status="Final"

    alt role = "HR"
        Note over ReportingSvc: No employee filter - sees ALL finalized
        ReportingSvc->>DB: (no additional WHERE clause)
    end

    alt role = "Manager"
        Note over ReportingSvc: Filtered to managed employees only
        ReportingSvc->>DB: AND ManagerEmployeeId = {empId}
    end

    alt role = "Employee"
        Note over ReportingSvc: Filtered to own appraisals only
        ReportingSvc->>DB: AND EmployeeId = {empId}
    end

    DB-->>ReportingSvc: Filtered results
    ReportingSvc-->>API: List of AppraisalDto
    API-->>Web: JSON response
    Web-->>User: Render completed appraisals table
```
