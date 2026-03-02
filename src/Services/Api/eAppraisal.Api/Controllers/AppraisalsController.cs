using eAppraisal.Shared.Auth;
using eAppraisal.Shared.Contracts;
using eAppraisal.Shared.Data;
using eAppraisal.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eAppraisal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppraisalsController(AppDbContext db) : ControllerBase
{
    // ── HR: initiate ──────────────────────────────────────────────────────────
    [HttpPost("initiate")]
    [Authorize(Roles = AppRoles.HR)]
    public async Task<IActionResult> Initiate([FromBody] InitiateAppraisalRequest req)
    {
        bool exists = await db.Appraisals.AnyAsync(a =>
            a.EmployeeId == req.EmployeeId && a.Year == req.Year);
        if (exists)
            return Conflict(new ApiResult(false, $"Appraisal for year {req.Year} already exists for this employee."));

        var appraisal = new Appraisal
        {
            EmployeeId    = req.EmployeeId,
            ManagerId     = req.ManagerId,
            Year          = req.Year,
            Status        = AppraisalStatus.AwaitingManagerComment,
            InitiatedByHR = User.Identity?.Name ?? "HR"
        };

        db.Appraisals.Add(appraisal);
        db.AuditLogs.Add(new AuditLog
        {
            Actor = User.Identity?.Name ?? "HR", Role = AppRoles.HR,
            Action = "APPRAISAL_INITIATED",
            Details = $"Appraisal {req.Year} initiated for employee {req.EmployeeId}"
        });
        await db.SaveChangesAsync();
        return Ok(new ApiResult(true, "Appraisal initiated.", appraisal.Id));
    }

    // ── Manager: submit comment ───────────────────────────────────────────────
    [HttpPost("manager-comment")]
    [Authorize(Roles = AppRoles.Manager)]
    public async Task<IActionResult> SubmitManagerComment([FromBody] SubmitManagerCommentRequest req)
    {
        var appraisal = await db.Appraisals.FindAsync(req.AppraisalId);
        if (appraisal is null) return NotFound(new ApiResult(false, "Appraisal not found."));
        if (appraisal.Status != AppraisalStatus.AwaitingManagerComment)
            return BadRequest(new ApiResult(false, $"Appraisal is in '{appraisal.Status}' state."));

        var empIdClaim = User.FindFirst("employee_id")?.Value;
        if (appraisal.ManagerId.ToString() != empIdClaim)
            return Forbid();

        appraisal.ManagerComments  = req.Comments;
        appraisal.ManagerCommentAt = DateTime.UtcNow;
        appraisal.Status           = AppraisalStatus.AwaitingEmployeeInput;

        db.AuditLogs.Add(new AuditLog
        {
            Actor = User.Identity?.Name ?? "Manager", Role = AppRoles.Manager,
            Action = "MANAGER_COMMENT_SUBMITTED",
            Details = $"Manager comment submitted for appraisal {req.AppraisalId}"
        });
        await db.SaveChangesAsync();
        return Ok(new ApiResult(true, "Comments submitted. Appraisal forwarded to employee."));
    }

    // ── Employee: self-assessment ─────────────────────────────────────────────
    [HttpPost("employee-input")]
    [Authorize(Roles = AppRoles.Employee)]
    public async Task<IActionResult> SubmitEmployeeInput([FromBody] SubmitEmployeeInputRequest req)
    {
        var appraisal = await db.Appraisals.FindAsync(req.AppraisalId);
        if (appraisal is null) return NotFound(new ApiResult(false, "Appraisal not found."));
        if (appraisal.Status != AppraisalStatus.AwaitingEmployeeInput)
            return BadRequest(new ApiResult(false, $"Appraisal is in '{appraisal.Status}' state."));

        var empIdClaim = User.FindFirst("employee_id")?.Value;
        if (appraisal.EmployeeId.ToString() != empIdClaim)
            return Forbid();

        appraisal.SelfAssessmentInput = req.SelfAssessment;
        appraisal.EmployeeInputAt     = DateTime.UtcNow;
        appraisal.Status              = AppraisalStatus.AwaitingFinalAssessment;

        db.AuditLogs.Add(new AuditLog
        {
            Actor = User.Identity?.Name ?? "Employee", Role = AppRoles.Employee,
            Action = "EMPLOYEE_INPUT_SUBMITTED",
            Details = $"Self-assessment submitted for appraisal {req.AppraisalId}"
        });
        await db.SaveChangesAsync();
        return Ok(new ApiResult(true, "Self-assessment submitted. Forwarded to manager for final assessment."));
    }

    // ── Manager: final assessment ─────────────────────────────────────────────
    [HttpPost("final-assessment")]
    [Authorize(Roles = AppRoles.Manager)]
    public async Task<IActionResult> SubmitFinalAssessment([FromBody] SubmitFinalAssessmentRequest req)
    {
        var appraisal = await db.Appraisals.FindAsync(req.AppraisalId);
        if (appraisal is null) return NotFound(new ApiResult(false, "Appraisal not found."));
        if (appraisal.Status != AppraisalStatus.AwaitingFinalAssessment)
            return BadRequest(new ApiResult(false, $"Appraisal is in '{appraisal.Status}' state."));

        var empIdClaim = User.FindFirst("employee_id")?.Value;
        if (appraisal.ManagerId.ToString() != empIdClaim)
            return Forbid();

        if (req.Rating is < 1 or > 5)
            return BadRequest(new ApiResult(false, "Rating must be between 1 and 5."));

        appraisal.FinalAssessment = req.FinalAssessment;
        appraisal.Rating          = req.Rating;
        appraisal.CompletedAt     = DateTime.UtcNow;
        appraisal.Status          = AppraisalStatus.Completed;

        db.AuditLogs.Add(new AuditLog
        {
            Actor = User.Identity?.Name ?? "Manager", Role = AppRoles.Manager,
            Action = "APPRAISAL_COMPLETED",
            Details = $"Final assessment submitted for appraisal {req.AppraisalId}, Rating: {req.Rating}/5"
        });
        await db.SaveChangesAsync();
        return Ok(new ApiResult(true, "Appraisal completed."));
    }

    // ── Queries ───────────────────────────────────────────────────────────────
    [HttpGet]
    [Authorize(Roles = $"{AppRoles.HR},{AppRoles.ITAdmin}")]
    public async Task<IActionResult> GetAll([FromQuery] int? year = null)
    {
        var query = db.Appraisals
            .Include(a => a.Employee)
            .Include(a => a.Manager)
            .AsQueryable();

        if (year.HasValue) query = query.Where(a => a.Year == year);

        var list = await query.OrderByDescending(a => a.InitiatedAt).ToListAsync();
        return Ok(list.Select(ToDto));
    }

    [HttpGet("my-team")]
    [Authorize(Roles = AppRoles.Manager)]
    public async Task<IActionResult> GetMyTeam()
    {
        var empIdClaim = User.FindFirst("employee_id")?.Value;
        if (!int.TryParse(empIdClaim, out var managerId))
            return Unauthorized();

        var list = await db.Appraisals
            .Include(a => a.Employee)
            .Include(a => a.Manager)
            .Where(a => a.ManagerId == managerId)
            .OrderByDescending(a => a.InitiatedAt)
            .ToListAsync();

        return Ok(list.Select(ToDto));
    }

    [HttpGet("mine")]
    [Authorize(Roles = AppRoles.Employee)]
    public async Task<IActionResult> GetMine()
    {
        var empIdClaim = User.FindFirst("employee_id")?.Value;
        if (!int.TryParse(empIdClaim, out var employeeId))
            return Unauthorized();

        var list = await db.Appraisals
            .Include(a => a.Employee)
            .Include(a => a.Manager)
            .Where(a => a.EmployeeId == employeeId)
            .OrderByDescending(a => a.Year)
            .ToListAsync();

        return Ok(list.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var a = await db.Appraisals
            .Include(x => x.Employee)
            .Include(x => x.Manager)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (a is null) return NotFound(new ApiResult(false, "Appraisal not found."));
        return Ok(ToDto(a));
    }

    [HttpGet("audit")]
    [Authorize(Roles = $"{AppRoles.HR},{AppRoles.ITAdmin}")]
    public async Task<IActionResult> GetAuditLog([FromQuery] int take = 50)
    {
        var logs = await db.AuditLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(take)
            .ToListAsync();
        return Ok(logs);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static AppraisalDto ToDto(Appraisal a) => new(
        a.Id, a.EmployeeId, a.Employee?.Name ?? "",
        a.Employee?.Department ?? "",
        a.ManagerId, a.Manager?.Name ?? "",
        a.Year, a.Status.ToString(),
        a.ManagerComments, a.SelfAssessmentInput,
        a.FinalAssessment, a.Rating,
        a.InitiatedAt, a.CompletedAt);
}
