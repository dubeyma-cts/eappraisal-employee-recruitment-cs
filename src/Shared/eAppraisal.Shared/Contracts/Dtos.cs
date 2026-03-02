namespace eAppraisal.Shared.Contracts;

// ── Auth ──────────────────────────────────────────────────────────────────────
public record LoginRequest(string Username, string Password);

public record LoginResponse(
    string Token,
    string Username,
    string Role,
    int? EmployeeId,
    string Message = "");

// ── Employee ──────────────────────────────────────────────────────────────────
public record EmployeeDto(
    int Id,
    string Name,
    string Email,
    string Mobile,
    string Department,
    string City,
    string MaskedPan,       // PAN is ALWAYS masked in transport
    int? ReportsToId,
    string? ReportsToName,
    decimal CTC,
    int WorkExperienceYears,
    DateTime DateOfJoining,
    bool IsActive);

public record CreateEmployeeRequest(
    string Name,
    string Address,
    string City,
    string PhoneNumber,
    string Mobile,
    string Email,
    DateTime DateOfBirth,
    string Gender,
    string MaritalStatus,
    DateTime DateOfJoining,
    string PassportNo,
    string PanNo,           // accepted raw only on create; never returned raw
    int WorkExperienceYears,
    int? ReportsToId,
    string Department,
    decimal CTC);

// ── Appraisal ─────────────────────────────────────────────────────────────────
public record AppraisalDto(
    int Id,
    int EmployeeId,
    string EmployeeName,
    string EmployeeDepartment,
    int ManagerId,
    string ManagerName,
    int Year,
    string Status,
    string? ManagerComments,
    string? SelfAssessmentInput,
    string? FinalAssessment,
    int? Rating,
    DateTime InitiatedAt,
    DateTime? CompletedAt);

public record InitiateAppraisalRequest(int EmployeeId, int ManagerId, int Year);

public record SubmitManagerCommentRequest(int AppraisalId, string Comments);

public record SubmitEmployeeInputRequest(int AppraisalId, string SelfAssessment);

public record SubmitFinalAssessmentRequest(
    int AppraisalId,
    string FinalAssessment,
    int Rating);

// ── Generic response ────────────────────────────────────────────────────────
public record ApiResult(bool Success, string Message, object? Data = null);

// ── User summary (IT Admin) ───────────────────────────────────────────────────
public record UserSummaryDto(
    int Id,
    string Username,
    string Role,
    bool IsLocked,
    int FailedLoginAttempts,
    DateTime? LastLoginAt);
