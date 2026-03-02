using eAppraisal.Shared.Contracts;
using eAppraisal.Shared.Data;
using eAppraisal.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace eAppraisal.Web.Services;

public class AppraisalService(AppDbContext db, BlazorAuthStateProvider authState)
{
    // ── HR ────────────────────────────────────────────────────────────────────
    public async Task<List<AppraisalDto>> GetAllAsync(int? year = null)
    {
        var q = db.Appraisals
            .Include(a => a.Employee)
            .Include(a => a.Manager)
            .AsQueryable();
        if (year.HasValue) q = q.Where(a => a.Year == year);
        return (await q.OrderByDescending(a => a.InitiatedAt).ToListAsync())
               .Select(ToDto).ToList();
    }

    public async Task<(bool ok, string msg)> InitiateAsync(int employeeId, int managerId, int year)
    {
        bool exists = await db.Appraisals.AnyAsync(a => a.EmployeeId == employeeId && a.Year == year);
        if (exists) return (false, $"Appraisal for {year} already exists for this employee.");

        db.Appraisals.Add(new Appraisal
        {
            EmployeeId = employeeId, ManagerId = managerId, Year = year,
            Status = AppraisalStatus.AwaitingManagerComment,
            InitiatedByHR = authState.CurrentUsername!
        });
        db.AuditLogs.Add(new AuditLog
        {
            Actor = authState.CurrentUsername!, Role = authState.CurrentRole!,
            Action = "APPRAISAL_INITIATED",
            Details = $"Appraisal {year} initiated for employee {employeeId}"
        });
        await db.SaveChangesAsync();
        return (true, "Appraisal cycle initiated successfully.");
    }

    // ── Manager ───────────────────────────────────────────────────────────────
    public async Task<List<AppraisalDto>> GetMyTeamAsync()
    {
        var managerId = authState.CurrentEmployeeId;
        if (managerId is null) return [];
        return (await db.Appraisals
            .Include(a => a.Employee)
            .Include(a => a.Manager)
            .Where(a => a.ManagerId == managerId)
            .OrderByDescending(a => a.InitiatedAt)
            .ToListAsync())
            .Select(ToDto).ToList();
    }

    public async Task<(bool ok, string msg)> SubmitManagerCommentAsync(int appraisalId, string comments)
    {
        var a = await db.Appraisals.FindAsync(appraisalId);
        if (a is null) return (false, "Appraisal not found.");
        if (a.Status != AppraisalStatus.AwaitingManagerComment)
            return (false, $"Appraisal is in '{a.Status}' state — cannot add comments now.");
        if (a.ManagerId != authState.CurrentEmployeeId)
            return (false, "You are not assigned as the manager for this appraisal.");

        a.ManagerComments  = comments;
        a.ManagerCommentAt = DateTime.UtcNow;
        a.Status           = AppraisalStatus.AwaitingEmployeeInput;
        db.AuditLogs.Add(new AuditLog
        {
            Actor = authState.CurrentUsername!, Role = authState.CurrentRole!,
            Action = "MANAGER_COMMENT_SUBMITTED",
            Details = $"Manager comment submitted for appraisal {appraisalId}"
        });
        await db.SaveChangesAsync();
        return (true, "Comments submitted. Appraisal forwarded to employee.");
    }

    public async Task<(bool ok, string msg)> SubmitFinalAssessmentAsync(int appraisalId, string assessment, int rating)
    {
        var a = await db.Appraisals.FindAsync(appraisalId);
        if (a is null) return (false, "Appraisal not found.");
        if (a.Status != AppraisalStatus.AwaitingFinalAssessment)
            return (false, $"Appraisal is in '{a.Status}' state.");
        if (rating is < 1 or > 5) return (false, "Rating must be between 1 and 5.");
        if (a.ManagerId != authState.CurrentEmployeeId)
            return (false, "You are not assigned as the manager for this appraisal.");

        a.FinalAssessment = assessment;
        a.Rating          = rating;
        a.CompletedAt     = DateTime.UtcNow;
        a.Status          = AppraisalStatus.Completed;
        db.AuditLogs.Add(new AuditLog
        {
            Actor = authState.CurrentUsername!, Role = authState.CurrentRole!,
            Action = "APPRAISAL_COMPLETED",
            Details = $"Final assessment submitted, Rating: {rating}/5 for appraisal {appraisalId}"
        });
        await db.SaveChangesAsync();
        return (true, "Appraisal completed successfully.");
    }

    // ── Employee ──────────────────────────────────────────────────────────────
    public async Task<List<AppraisalDto>> GetMineAsync()
    {
        var empId = authState.CurrentEmployeeId;
        if (empId is null) return [];
        return (await db.Appraisals
            .Include(a => a.Employee)
            .Include(a => a.Manager)
            .Where(a => a.EmployeeId == empId)
            .OrderByDescending(a => a.Year)
            .ToListAsync())
            .Select(ToDto).ToList();
    }

    public async Task<(bool ok, string msg)> SubmitSelfAssessmentAsync(int appraisalId, string input)
    {
        var a = await db.Appraisals.FindAsync(appraisalId);
        if (a is null) return (false, "Appraisal not found.");
        if (a.Status != AppraisalStatus.AwaitingEmployeeInput)
            return (false, $"Appraisal is in '{a.Status}' state.");
        if (a.EmployeeId != authState.CurrentEmployeeId)
            return (false, "This appraisal does not belong to you.");

        a.SelfAssessmentInput = input;
        a.EmployeeInputAt     = DateTime.UtcNow;
        a.Status              = AppraisalStatus.AwaitingFinalAssessment;
        db.AuditLogs.Add(new AuditLog
        {
            Actor = authState.CurrentUsername!, Role = authState.CurrentRole!,
            Action = "EMPLOYEE_INPUT_SUBMITTED",
            Details = $"Self-assessment submitted for appraisal {appraisalId}"
        });
        await db.SaveChangesAsync();
        return (true, "Self-assessment submitted. Forwarded to your manager for final review.");
    }

    public async Task<AppraisalDto?> GetByIdAsync(int id)
    {
        var a = await db.Appraisals
            .Include(x => x.Employee).Include(x => x.Manager)
            .FirstOrDefaultAsync(x => x.Id == id);
        return a is null ? null : ToDto(a);
    }

    public async Task<List<AuditLog>> GetAuditLogsAsync(int take = 50) =>
        await db.AuditLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(take)
            .ToListAsync();

    private static AppraisalDto ToDto(Appraisal a) => new(
        a.Id, a.EmployeeId, a.Employee?.Name ?? "", a.Employee?.Department ?? "",
        a.ManagerId, a.Manager?.Name ?? "", a.Year, a.Status.ToString(),
        a.ManagerComments, a.SelfAssessmentInput, a.FinalAssessment, a.Rating,
        a.InitiatedAt, a.CompletedAt);
}
